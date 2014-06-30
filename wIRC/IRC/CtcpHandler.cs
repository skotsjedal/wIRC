using System;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using wIRC.Models;
using wIRC.Util;

namespace wIRC.IRC
{
    public class CtcpHandler
    {
        private readonly TcpClient _client;

        public CtcpHandler(TcpClient client)
        {
            _client = client;
        }

        public void HandleCtcpReply(IrcResponse parsedResponse)
        {
            // TODO
            IrcUtils.WriteOutputLine(parsedResponse.Response);
        }

        public void HandleCtcpRequest(IrcResponse parsedResponse)
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
                IrcUtils.WriteOutputLine("CTCP command {0} not supported yet", ctcpCommand);
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
                    var env = string.Format(".NET {0}", Environment.Version);
                    SendCtcpReply(parsedResponse.Nick, string.Format("VERSION {0} {1} runnning on {2}", name, version, env));
                    break;
            }
        }

        public void SendCtcpRequest(string destination, string command)
        {
            SendCtcp("PRIVMSG", destination, command);
        }

        private void SendCtcpReply(string destination, string command)
        {
            SendCtcp("NOTICE", destination, command);
        }

        private void SendCtcp(string type, string destination, string command)
        {
            var message = string.Format("{3} {0} :{2}{1}{2}", destination, command, Constants.X01, type);
            _client.SendCommand(message);
        }
    }
}