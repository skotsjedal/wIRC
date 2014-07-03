using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wIRC.Util
{
    internal class GuiUtil
    {
        public class TextBoxStreamWriter : TextWriter
        {
            private readonly TextBox _output;
            private readonly StringBuilder _buffer = new StringBuilder();

            public TextBoxStreamWriter(TextBox output)
            {
                _output = output;
            }

            public override void Write(char value)
            {
                _buffer.Append(value);
                if (value != '\n') return;

                if (_output.IsDisposed)
                    return;

                Action a = () => _output.AppendText(_buffer.ToString());
                _output.Invoke(a);
                _buffer.Clear();
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }
    }
}