using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using wIRC.Util;

namespace wIRC.IRC
{
    public class ConnectionListener
    {
        public TcpClient TcpClient { get; set; }
        public WIrcConnection IrcConnection { get; set; }

        public async void Listen()
        {
            if (IrcConnection == null || TcpClient == null)
            {
                throw new NullReferenceException("IrcConnection or TcpClient is null");
            }

            if (!TcpClient.Connected)
            {
                throw new InvalidProgramException("TcpClient not connected after starting listener");
            }

            var reader = new StreamReader(TcpClient.GetStream());

            while (true)
            {
                if (!TcpClient.Connected)
                    break;

                string response;
                try
                {
                    response = await reader.ReadLineAsync();
                }
                catch (ObjectDisposedException)
                {
                    // ReadLineAsync won't listen to cancellationtokens
                    IrcUtils.WriteOutputLine("Listener: Client shut down connection");
                    return;
                }

                if (response != null)
                {
                    Debug.WriteLine(string.Format("<INN> {0}", response));
                    IrcConnection.HandleResponse(response);
                }
                else
                {
                    break;
                }
            }

            IrcUtils.WriteOutputLine("Listener: Client lost connection");
            IrcConnection.NotifyLostConnection();
        }
    }
}