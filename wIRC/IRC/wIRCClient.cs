using System;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using wIRC.Models;
using wIRC.Util;

namespace wIRC.IRC
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected
    }

    public class WIrcClient
    {
        public ConnectionState State { get; set; }
        private TcpClient _client;
        private String _endpoint;

        public String Endpoint
        {
            get { return _endpoint; }
            set
            {
                if (!State.Equals(ConnectionState.Disconnected))
                {
                    throw new InvalidOperationException("Cannot change endpoint unless disconnected");
                }
                _endpoint = value;
            }
        }

        private int _port;
        private Thread _listenerThread;


        public int Port
        {
            get { return _port; }
            set
            {
                if (!State.Equals(ConnectionState.Disconnected))
                {
                    throw new InvalidOperationException("Cannot change port unless disconnected");
                }
                _port = value;
            }
        }

        private string _nick;

        public String Nick
        {
            get { return _nick; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _nick = value;
                if (State.Equals(ConnectionState.Connected))
                {
                    ChangeNick(_nick);
                }
            }
        }

        public WIrcClient(String endpoint, int port)
        {
            Endpoint = endpoint;
            Port = port;
            State = ConnectionState.Disconnected;
        }

        public void Connect()
        {
            IrcUtils.WriteOutput("Connecting\r\n");
            _client = new TcpClient(Endpoint, Port);
            State = ConnectionState.Connecting;

            var listener = new ConnectionListener {TcpClient = _client, IrcClient = this};

            _listenerThread = new Thread(listener.Listen);
            _listenerThread.Start();
        }

        private void SendConnectInfo()
        {
            _client.SendCommand(String.Format("PASS {0}", Guid.NewGuid()));
            _client.SendCommand(String.Format("USER {0} 0 * :No Name Hear", Nick));
        }

        private void ChangeNick(string nick)
        {
            _client.SendCommand(String.Format("NICK {0}", nick));
        }

        public void Disconnect()
        {
            IrcUtils.WriteOutput("Disconnecting\r\n");
            _client.SendCommand("QUIT");
            _listenerThread.Abort();
            _listenerThread.Join();
            _client.Close();
            State = ConnectionState.Disconnected;
            IrcUtils.WriteOutput("Disconnected\r\n");
        }

        public void HandleResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return;

            var parsedResponse = IrcUtils.ParseIrcResponse(response);
            switch (parsedResponse.Command)
            {
                case "020":
                    SendConnectInfo();
                    ChangeNick(Nick);
                    break;
                case "001":
                    State = ConnectionState.Connected;
                    const string channel = "#test";
                    Join(channel);
                    break;
                case "NOTICE":
                    if (parsedResponse.Args.Last()[0] == 1)
                    {
                        HandleCtcpReply(parsedResponse);
                        break;
                    }

                {
                    var nick = new string(parsedResponse.Source.TakeWhile(c => c != '!' && c != '@').ToArray());
                    IrcUtils.WriteOutput("-{0}- {1}", nick, parsedResponse.Args.Last());
                }
                    break;
                case "PRIVMSG":
                    if (parsedResponse.Args.Last()[0] == 1)
                    {
                        HandleCtcp(parsedResponse);
                        break;
                    }

                {
                    var nick = new string(parsedResponse.Source.TakeWhile(c => c != '!' && c != '@').ToArray());
                    IrcUtils.WriteOutput("<{0}> {1}", nick, parsedResponse.Args.Last());
                }
                    break;
                case "421":
                    IrcUtils.WriteOutput(parsedResponse.Args.Last());
                    break;
                case "PING":
                    _client.SendCommand(String.Format("PONG {0}", parsedResponse.Args.First()));
                    break;
                default:
                    IrcUtils.WriteOutput("{0}", parsedResponse.Response);
                    break;
            }
        }

        private void HandleCtcpReply(IrcResponse parsedResponse)
        {
            // TODO
            IrcUtils.WriteOutput("{0}", parsedResponse.Response);
        }

        private void HandleCtcp(IrcResponse parsedResponse)
        {
            var commandString = parsedResponse.Args.Last();

            var parts = commandString.Substring(1, commandString.Length - 2).Split();
            var ctcpCommand = parts[0];
            var args = parts.Skip(1);
            CTCP ctcp;

            try
            {
                ctcp = (CTCP) Enum.Parse(typeof (CTCP), ctcpCommand, false);
            }
            catch (ArgumentException ex)
            {
                IrcUtils.WriteOutput("CTCP command {0} not supported yet", ctcpCommand);
                return;
            }

            switch (ctcp)
            {
                case CTCP.PING:
                    SendCtcpReply(parsedResponse.Source, string.Format("PING {0}", args.First()));
                    break;
                case CTCP.VERSION:
                    var assemly = Assembly.GetEntryAssembly().GetName();
                    var name = assemly.Name;
                    var version = assemly.Version;
                    var env = string.Format("C# .NET {0} on {1}", Environment.Version, Environment.OSVersion);
                    SendCtcpReply(parsedResponse.Source, string.Format("VERSION {0} {1} under {2}", name, version, env));
                    break;
            }
        }

        public void SendCtcpRequest(string destination, string command)
        {
            SendCtcp("PRIVMSG", destination, command);
        }

        public void SendCtcpReply(string destination, string command)
        {
            SendCtcp("NOTICE", destination, command);
        }

        private void SendCtcp(string type, string destination, string command)
        {
            var message = string.Format("{3} {0} :{2}{1}{2}", destination, command, Constants.X01, type);
            _client.SendCommand(message);
        }

        private void Join(string channel)
        {
            if (String.IsNullOrWhiteSpace(channel))
                return;
            if (channel[0] != '#' && channel[0] != '&')
                channel = "#" + channel;
            _client.SendCommand(String.Format("JOIN {0}", channel));
        }

        public void Send(string input)
        {
            _client.SendCommand(input, true);
        }
    }
}