using System;
using System.Linq;
using wIRC.IRC;

namespace wIRC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new WIrcClient("192.168.1.191", 6602) { Nick = "test" };
            client.Connect();

            string input;

            while ((input = Console.ReadLine()) != "/exit")
            {
                if (input == null) continue;
                var parts = input.Split();
                if (parts.First().Equals("/ctcp"))
                {
                    client.SendCtcpRequest(parts.ElementAt(1), string.Join("", parts.Skip(2)));
                    continue;
                }
                client.Send(input);
            }

            client.Disconnect();
        }
    }
}