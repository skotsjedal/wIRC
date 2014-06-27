using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace wIRC.Util
{
    public static class TcpClientExtensions
    {
        private static readonly ASCIIEncoding Encoding = new ASCIIEncoding();

        public static void SendCommand(this TcpClient client, string command, bool silent = false)
        {
            if (!client.Connected)
            {
                throw new InvalidProgramException("Not connected");
            }
            command += Constants.Crlf;
            Debug.Write(string.Format("<OUT> {0}", command));
            if (!silent)
                IrcUtils.WriteOutput(command);
            var s = client.GetStream();
            var package = Encoding.GetBytes(command);
            s.Write(package, 0, package.Length);
        }
    }
}