using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wIRC.Models;

namespace wIRC.Util
{
    public static class IrcUtils
    {
        public static IrcResponse ParseIrcResponse(string response)
        {
            var ircResponse = new IrcResponse {Response = response};
            if (ircResponse.Response[0] == ':')
            {
                var el = ircResponse.Response.Split(new[] { ' ' }, 2);
                ircResponse.Source = el[0].Substring(1);
                ircResponse.Response = el[1];
            }

            if (ircResponse.Response.Contains(" :"))
            {
                var el = ircResponse.Response.Split(new[] { " :" }, 2, StringSplitOptions.RemoveEmptyEntries);
                var el2 = el[0].Split();
                ircResponse.Command = el2[0];
                ircResponse.Args = el2.Skip(1).ToList();
                ircResponse.Args.Add(el[1]);
            }
            else
            {
                var el = ircResponse.Response.Split();
                ircResponse.Command = el[0];
                ircResponse.Args = el.Skip(1).ToList();
            }

            return ircResponse;
        }

        public static void WriteOutput(string output)
        {
            Console.Write(output);
        }

        public static void WriteOutput(string output, params object[] values)
        {
            Console.WriteLine(output, values);
        }
    }
}
