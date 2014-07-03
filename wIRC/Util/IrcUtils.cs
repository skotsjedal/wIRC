using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using wIRC.Models;

namespace wIRC.Util
{
    public static class IrcUtils
    {
        private static readonly object WriteLock = new object();
        public static TextWriter Writer { get; set; }

        public static IrcResponse ParseIrcResponse(string response)
        {
            var ircResponse = new IrcResponse {Response = response};
            if (ircResponse.Response[0] == ':')
            {
                var el = ircResponse.Response.Split(new[] {' '}, 2);
                ircResponse.Source = el[0].Substring(1);
                ircResponse.Response = el[1];
            }

            if (ircResponse.Response.Contains(" :"))
            {
                var el = ircResponse.Response.Split(new[] {" :"}, 2, StringSplitOptions.RemoveEmptyEntries);
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
        
        public static void WriteOutput(string output, params object[] values)
        {
            output = string.Format(output, values);
            if (Writer != null)
            {
                lock (WriteLock)
                {
                    Writer.Write(output);
                }
                return;
            }
            Console.Write(output);
        }

        public static void WriteOutputLine(string output, params object[] values)
        {
            WriteOutput(output + Environment.NewLine, values);
        }

        public static void PrintRaw(string text)
        {
            var parsed =
                String.Concat(text.ToList().Select(c => string.Format("{0}_{1} ", c, Convert.ToByte(c).ToString("x"))));
            Debug.WriteLine(parsed);
        }

        public static bool CheckArgs(IList args, int i, string source = null)
        {
            if (args.Count >= i) return true;
            WriteOutputLine("Insufficient parameters for command {0}", source);
            return false;
        }
    }
}