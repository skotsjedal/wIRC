using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using wIRC.Config;
using wIRC.IRC;
using wIRC.Util;

namespace wIRC
{
    internal class Program
    {
        private static readonly List<WIrcClient> _clients = new List<WIrcClient>();
        private static WIrcClient _active;

        private static void Main(string[] args)
        {
            var running = true;

            foreach (Server server in Conf.IrcConfig.Servers)
            {
                var wIrcClient = new WIrcClient(server.Endpoint, server.Port) {Nick = server.Nick, Name = server.Name};
                wIrcClient.Connect();
                _active = wIrcClient;
                _clients.Add(wIrcClient);
            }

            while (running)
            {
                var input = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(input)) continue;

                if (input[0] == '/')
                {
                    var parts = input.Split();
                    var command = parts[0].Substring(1);
                    switch (command)
                    {
                        case "msg":
                            _active.Message(parts.ElementAt(1), string.Join(" ", parts.Skip(2)));
                            break;
                        case "ctcp":
                            _active.SendCtcpRequest(parts.ElementAt(1), string.Join(" ", parts.Skip(2)));
                            break;
                        case "exit":
                            running = false;
                            break;
                        case "quit":
                            _active.Disconnect(string.Join(" ", parts.Skip(1)));
                            break;
                        default:
                            IrcUtils.WriteOutput("Unknown command {0}\r\n", command);
                            break;
                    }
                    continue;
                }
                if (input[0] == '§')
                {
                    _active.Send(input.Substring(1));
                    continue;
                }
                _active.Chat(input);
            }

            _clients.ForEach(WIrcClient.Disconnect);
        }
    }
}