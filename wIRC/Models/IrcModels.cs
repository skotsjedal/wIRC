using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wIRC.Models
{
    public struct IrcResponse
    {
        public string Source { get; set; }

        public string Nick
        {
            get
            {
                return new string(Source.TakeWhile(c => c != '!' && c != '@').ToArray());
            }
        }

        public string Command { get; set; }
        public List<string> Args { get; set; }
        public string Response { get; set; }
    }
}
