using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using wIRC.Config;
using wIRC.Util;

namespace wIRC.IRC
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting
    }

    public class WIrcClient : IDisposable
    {
        private string _activeTarget;
        private ConnectionState _state;

        public ConnectionState State
        {
            get { return _state; }
            private set
            {
                Debug.WriteLine("State for {0} changed from {1} to {2}", Name, State, value);
                _state = value;
            }
        }

        private TcpClient _client;
        private String _endpoint;

        public String Endpoint
        {
            get { return _endpoint; }
            set
            {
                if (State != ConnectionState.Disconnected)
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
                if (State != ConnectionState.Disconnected)
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
                if (State == ConnectionState.Connected)
                {
                    ChangeNick(_nick);
                }
            }
        }

        public string Name { get; set; }
        public List<String> AutoJoinChannels { get; set; }

        public CtcpHandler CtcpHandler { get; private set; }

        public WIrcClient(string endpoint, int port, string name = null)
        {
            name = string.IsNullOrWhiteSpace(name) ? endpoint : name;
            Nick = Conf.IrcConfig.DefaultNick;
            Endpoint = endpoint;
            Port = port;
            Name = name;
            State = ConnectionState.Disconnected;
        }

        public void Connect()
        {
            IrcUtils.WriteOutputLine("Connecting to {0}", Name);
            _client = new TcpClient(Endpoint, Port);
            CtcpHandler = new CtcpHandler(_client);
            State = ConnectionState.Connecting;

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
            if (State == ConnectionState.Disconnected)
                return;

            State = ConnectionState.Disconnecting;
            IrcUtils.WriteOutputLine("Disconnecting");
            Quit(message);

            StopListener();
            _client.Close();
            State = ConnectionState.Disconnected;
            IrcUtils.WriteOutputLine("Disconnected");
        }

        private void Quit(string message)
        {
            var command = "QUIT";
            if (!String.IsNullOrWhiteSpace(message))
                command += string.Format(" :{0}", message);
            _client.SendCommand(command);
        }

        private void StopListener()
        {
            _listenerThread.Abort();
            _listenerThread.Join();
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
                    State = ConnectionState.Connected;
                    foreach (var channel in AutoJoinChannels)
                    {
                        Join(channel);
                    }
                    break;
                case "NOTICE":
                    if (parsedResponse.Args.Last()[0] == 1)
                    {
                        CtcpHandler.HandleCtcpReply(parsedResponse);
                        break;
                    }
                {
                    var target = parsedResponse.Args.First();
                    if (target.Equals(Nick))
                    {
                        IrcUtils.WriteOutput("-{0}- {1}", parsedResponse.Nick, parsedResponse.Args.Last());
                        break;
                    }
                    IrcUtils.WriteOutput("-{0}:{1}- {2}", parsedResponse.Nick, target, parsedResponse.Args.Last());
                }
                    break;
                case "PRIVMSG":
                    if (parsedResponse.Args.Last()[0] == 1)
                    {
                        CtcpHandler.HandleCtcpRequest(parsedResponse);
                        break;
                    }
                {
                    var target = parsedResponse.Args.First();
                    if (target.Equals(Nick))
                    {
                        IrcUtils.WriteOutput("*{0}* {1}", parsedResponse.Nick, parsedResponse.Args.Last());
                        break;
                    }
                    IrcUtils.WriteOutput("{0} <{1}> {2}", target, parsedResponse.Nick, parsedResponse.Args.Last());
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
                case "ERROR":
                    if (parsedResponse.Args.First().StartsWith("Closing Link"))
                    {
                        IrcUtils.WriteOutputLine("Client disconnected from {0}", Name);
                        break;
                    }
                    IrcUtils.WriteOutputLine("{0}", parsedResponse.Response);
                    break;
                default:
                    IrcUtils.WriteOutputLine("{0}", parsedResponse.Response);
                    break;
            }
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
            _client.SendCommand(String.Format("PRIVMSG {0} :{1}", target, message), true);
        }

        public void Chat(string message)
        {
            if (String.IsNullOrWhiteSpace(_activeTarget))
                IrcUtils.WriteOutputLine("Not in a channel or query");
            Message(_activeTarget, message);
        }

        public static void Disconnect(WIrcClient client)
        {
            client.Disconnect();
        }

        public void SendCtcpRequest(string destination, string command)
        {
            if (State != ConnectionState.Connected)
                return;
            CtcpHandler.SendCtcpRequest(destination, command);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (State == ConnectionState.Disconnected)
                return;

            if (isDisposing)
            {
                Disconnect();
            }
        }

        public void NotifyLostConnection()
        {
            if (State == ConnectionState.Disconnecting || State == ConnectionState.Disconnected)
            {
                Debug.WriteLine(string.Format("Listener lost connection after disconnect from {0}", Name));
                return;
            }
            State = ConnectionState.Disconnected;
            var message = String.Format("Unexpected loss of connection to server {0}", Name);
            Debug.WriteLine(message);
            IrcUtils.WriteOutputLine(message);
        }
    }
}