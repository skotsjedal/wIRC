using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using wIRC.Config;
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
        private string _activeTarget;
        private ConnectionState _state;

        public ConnectionState State
        {
            get { return _state; }
        }

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
                    throw new InvalidOperationException("Cannot change Port unless disconnected");
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
                if (string.IsNullOrWhiteSpace(value)) return;
                _nick = value;
                if (State.Equals(ConnectionState.Connected))
                {
                    ChangeNick(_nick);
                }
            }
        }

        public string Name { get; set; }
        public List<String> AutoJoinChannels { get; set; }

        public WIrcClient(String endpoint, int port)
        {
            Nick = Conf.IrcConfig.DefaultNick;
            Endpoint = endpoint;
            Port = port;
            _state = ConnectionState.Disconnected;
        }

        public void Connect()
        {
            IrcUtils.WriteOutput("Connecting to {0}\r\n", Name);
            _client = new TcpClient(Endpoint, Port);
            _state = ConnectionState.Connecting;

            var listener = new ConnectionListener {TcpClient = _client, IrcClient = this};

            _listenerThread = new Thread(listener.Listen);
            _listenerThread.Start();
            
            SendConnectInfo();
            ChangeNick(Nick);
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

        public void Disconnect(string message = null)
        {
            if (State.Equals(ConnectionState.Disconnected))
                return;
            IrcUtils.WriteOutput("Disconnecting\r\n");
            var command = "QUIT";
            if (!String.IsNullOrWhiteSpace(message))
                command += string.Format(" {0}", message);
            _client.SendCommand(command);

            _listenerThread.Abort();
            _listenerThread.Join();
            _client.Close();
            _state = ConnectionState.Disconnected;
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
                    Debug.WriteLine("Connection established");
                    break;
                case "001":
                    _state = ConnectionState.Connected;
                    foreach (var channel in AutoJoinChannels)
                    {
                        Join(channel);    
                    }
                    break;
                case "NOTICE":
                    if (parsedResponse.Args.Last()[0] == 1)
                    {
                        HandleCtcpReply(parsedResponse);
                        break;
                    }

                {
                    IrcUtils.WriteOutput("-{0}- {1}", parsedResponse.Nick, parsedResponse.Args.Last());
                }
                    break;
                case "PRIVMSG":
                    if (parsedResponse.Args.Last()[0] == 1)
                    {
                        HandleCtcp(parsedResponse);
                        break;
                    }

                {
                    IrcUtils.WriteOutput("<{0}> {1}", parsedResponse.Nick, parsedResponse.Args.Last());
                }
                    break;
                case "421":
                    IrcUtils.WriteOutput(parsedResponse.Args.Last());
                    break;
                case "PING":
                    _client.SendCommand(String.Format("PONG {0}", parsedResponse.Args.First()));
                    break;
                case "JOIN":
                    _activeTarget = parsedResponse.Args.First();
                    break;
                default:
                    IrcUtils.WriteOutput("{0}\r\n", parsedResponse.Response);
                    break;
            }
        }

        private void HandleCtcpReply(IrcResponse parsedResponse)
        {
            // TODO
            IrcUtils.WriteOutput("{0}\r\n", parsedResponse.Response);
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
                    SendCtcpReply(parsedResponse.Nick, string.Format("PING {0}", string.Join(" ", args)));
                    break;
                case CTCP.VERSION:
                    var assemly = Assembly.GetEntryAssembly().GetName();
                    var name = assemly.Name;
                    var version = assemly.Version;
                    var env = string.Format("C# .NET {0} on {1}", Environment.Version, Environment.OSVersion);
                    SendCtcpReply(parsedResponse.Nick, string.Format("VERSION {0} {1} under {2}", name, version, env));
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

        public void Message(string target, string message)
        {
            _activeTarget = target;
            _client.SendCommand(String.Format("PRIVMSG {0} {1}", target, message), true);
            IrcUtils.WriteOutput(String.Format("<{0}> {1}\r\n", Nick, message));
        }

        public void Chat(string message)
        {
            if (String.IsNullOrWhiteSpace(_activeTarget))
                IrcUtils.WriteOutput("Not in a channel or query\r\n");
            Message(_activeTarget, message);
        }

        public static void Disconnect(WIrcClient client)
        {
            client.Disconnect();
        }
    }
}