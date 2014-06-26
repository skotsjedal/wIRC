using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wIRC.Models;
using wIRC.Util;

namespace wIRC.IRC
{
    public enum ConnectionState
    {
        Disconnected, Connecting, Connected
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
        public String Nick { get; set; }

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
        
        private void SendPass()
        {
            _client.SendCommand(String.Format("PASS {0}", Guid.NewGuid()));
        }

        private void SendUser()
        {
            _client.SendCommand(String.Format("USER {0} 0 * :No Name Hear", Nick));
        }

        private void SendNick()
        {
            _client.SendCommand(String.Format("NICK {0}", Nick));
        }

        public void Disconnect()
        {
            IrcUtils.WriteOutput("Disconnecting\r\n");
            _listenerThread.Abort();
            _listenerThread.Join();
            _client.Close();
            State = ConnectionState.Disconnected;
            IrcUtils.WriteOutput("Disconnected\r\n");
        }

        public void Idle()
        {
            Thread.Sleep(500);
        }

        public void HandleResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return;

            var parsedResponse = IrcUtils.ParseIrcResponse(response);
            switch (parsedResponse.Command)
            {
                case "020":
                    SendPass();
                    SendNick();
                    SendUser();
                    break;
                case "001":
                    const string channel = "#test";
                    Join(channel);
                    break;
                case "NOTICE":
                case "PRIVMSG":
                    if (parsedResponse.Args.Last()[0] == 1)
                    {
                        HandleCtcp(parsedResponse);
                        break;
                    }

                    var nick = new string(parsedResponse.Source.TakeWhile(c => c != '!' && c != '@').ToArray());
                    IrcUtils.WriteOutput("<{0}> {1}", nick, parsedResponse.Args.Last());
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

        private void HandleCtcp(IrcResponse parsedResponse)
        {
            var commandString = parsedResponse.Args.Last();
            var parts = commandString.Substring(1, commandString.Length - 2).Split();
            var ctcpCommand = parts[0];
            var args = parts.Skip(1);
            switch (ctcpCommand)
            {
                case "PING":
                    SendCtcpCommand(parsedResponse.Source, string.Format("PING {0}", args.First()));
                    break;
                default:
                    IrcUtils.WriteOutput("CTCP command {0} not supported yet", ctcpCommand);
                    break;
            }
        }

        public void SendCtcpCommand(string destination, string command)
        {
            _client.SendCommand(string.Format("PRIVMSG {0} :\x01{1}\x01", destination, command));
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

    public class ConnectionListener
    {
        public TcpClient TcpClient { get; set; }
        public WIrcClient IrcClient { get; set; }

        public async void Listen()
        {
            if (IrcClient == null || TcpClient == null)
            {
                throw new NullReferenceException("IrcClient or TcpClient is null");
            }

            var reader = new StreamReader(TcpClient.GetStream());

            while (true)
            {
                if (!TcpClient.Connected)
                    throw new InvalidProgramException("Listener lost connection");

                var response = await reader.ReadLineAsync();
                if (response != null)
                {
                    Debug.WriteLine(string.Format("<INN> {0}", response));
                    IrcClient.HandleResponse(response);
                }
                else
                {
                    break;
                }

            }
            IrcUtils.WriteOutput("Client lost connection or called quit");
            IrcClient.Disconnect();
        }
    }

    public static class TcpClientExtensions
    {
        private static readonly ASCIIEncoding Encoding = new ASCIIEncoding();
        
        public static void SendCommand(this TcpClient client, string command, bool silent = false)
        {
            if (!client.Connected)
            {
                throw new InvalidProgramException("Not connected");
            }
            command += "\x0D\x0A";
            Debug.WriteLine(string.Format("<OUT> {0}", command));
            if (!silent)
                IrcUtils.WriteOutput(command);
            var s = client.GetStream();
            var package = Encoding.GetBytes(command);
            s.Write(package, 0, package.Length);
        }
    }
    
}
