using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using wIRC.Config;
using wIRC.IRC;
using wIRC.Util;

namespace wIRC
{
    internal class Program
    {
        private static readonly List<WIrcClient> Clients = new List<WIrcClient>();
        private static WIrcClient _active;
        private static string _quitMessage;

        private static void Main(string[] args)
        {
            var running = true;

            foreach (Server server in Conf.IrcConfig.Servers)
            {
                var wIrcClient = new WIrcClient(server.Endpoint, server.Port, server.Name)
                {
                    Nick = server.Nick,
                    AutoJoinChannels = server.ChannelsList
                };
                wIrcClient.Connect();
                _active = wIrcClient;
                Clients.Add(wIrcClient);
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
                            if (IrcUtils.CheckArgs(parts, 3))
                            {
                                // /msg foo bar
                                _active.Message(parts.ElementAt(1), string.Join(" ", parts.Skip(2)));
                            }
                            break;
                        case "ctcp":
                            if (IrcUtils.CheckArgs(parts, 3))
                            {
                                // ctcp foo version
                                _active.SendCtcpRequest(parts.ElementAt(1), string.Join(" ", parts.Skip(2)));
                            }
                            break;
                        case "exit":
                            _quitMessage = string.Join(" ", parts.Skip(1));
                            running = false;
                            break;
                        case "quit":
                            _active.Disconnect(string.Join(" ", parts.Skip(1)));
                            break;
                        case "server":
                        case "connect":

                            break;
                        default:
                            IrcUtils.WriteOutput("Unknown command {0}\r\n", command);
                            break;
                    }
                    continue;
                }

                // Raw message to the server
                if (input[0] == '|')
                {
                    _active.Send(input.Substring(1));
                    continue;
                }

                // Default chat to current active target
                _active.Chat(input);
            }

            Clients.ForEach(c => c.Disconnect(_quitMessage));
        }
    }
}