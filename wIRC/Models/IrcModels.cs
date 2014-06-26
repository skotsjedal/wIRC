using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wIRC.Models
{
    public class IrcResponse
    {
        public string Source { get; set; }
        public string Command { get; set; }
        public List<string> Args { get; set; }
        public string Response { get; set; }
    }
}
