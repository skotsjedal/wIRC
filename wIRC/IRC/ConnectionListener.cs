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
        public WIrcClient IrcClient { get; set; }

        public async void Listen()
        {
            if (IrcClient == null || TcpClient == null)
            {
                throw new NullReferenceException("IrcClient or TcpClient is null");
            }

            if (!TcpClient.Connected)
            {
                throw new InvalidProgramException("TcpClient not connected after starting listener");
            }

            var reader = new StreamReader(TcpClient.GetStream());
            try
            {
                while (true)
                {
                    if (!TcpClient.Connected)
                        break;

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
            }
            catch (ThreadAbortException)
            {
                IrcUtils.WriteOutputLine("Listener: Client shut down connection");
                return;
            }
            IrcUtils.WriteOutputLine("Listener: Client lost connection");
            IrcClient.NotifyLostConnection();
        }
    }
}