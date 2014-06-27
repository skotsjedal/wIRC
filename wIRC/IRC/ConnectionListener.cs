﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using wIRC.Util;

namespace wIRC.IRC
{
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
        }
    }
}
