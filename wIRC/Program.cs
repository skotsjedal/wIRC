using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wIRC.IRC;

namespace wIRC
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new WIrcClient("192.168.1.191", 6602) {Nick = "test"};
            client.Connect();

            string input;

            while ((input = Console.ReadLine()) != "/exit")
            {
                if (input != null)
                {
                    var parts = input.Split();
                    if (parts.First().Equals("/ctcp"))
                    {
                        client.SendCtcpCommand(parts.ElementAt(1), string.Join("", parts.Skip(2)));
                    }
                }
                client.Send(input);
            }

            client.Disconnect();
        }
    }
}
