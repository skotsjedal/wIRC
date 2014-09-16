using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using wIRC.Config;
using wIRC.IRC;
using wIRC.Util;

namespace wIRC.Gui
{
    public partial class WIrcGui : Form
    {
        private WIrcConnection _activeConnection;
        private readonly List<WIrcConnection> _connections = new List<WIrcConnection>();
        private string _quitMessage;

        private readonly List<string> _historyInput = new List<string>();
        private int _historyCursor = 0;

        public WIrcGui()
        {
            InitializeComponent();
            IrcUtils.Writer = new GuiUtil.TextBoxStreamWriter(output);
        }

        private void WIrcGui_Load(object sender, EventArgs e)
        {
            foreach (Server server in Conf.IrcConfig.Servers)
            {
                var wIrcClient = new WIrcConnection(server.Endpoint, server.Port, server.Name)
                {
                    Nick = server.Nick,
                    AutoJoinChannels = server.ChannelsList
                };
                wIrcClient.Connect();
                _activeConnection = wIrcClient;
                _connections.Add(wIrcClient);
            }
        }

        private void textInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                return;
            }
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    HandleInput(textInput.Text);
                    textInput.Text = string.Empty;
                    break;
            }
        }

        private void HandleInput(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) return;

            _historyInput.Add(input);
            _historyCursor = _historyInput.Count;

            if (input[0] == '/')
            {
                var parts = input.Split();
                var command = parts[0].Substring(1);
                switch (command)
                {
                    case "msg":
                        if (IrcUtils.CheckArgs(parts, 3))
                        {
                            // /msg foo bar
                            _activeConnection.Message(parts.ElementAt(1), string.Join(" ", parts.Skip(2)));
                        }
                        break;
                    case "ctcp":
                        if (IrcUtils.CheckArgs(parts, 3))
                        {
                            // ctcp foo version
                            _activeConnection.SendCtcpRequest(parts.ElementAt(1), string.Join(" ", parts.Skip(2)));
                        }
                        break;
                    case "nick":
                        if (IrcUtils.CheckArgs(parts, 2))
                        {
                            _activeConnection.Nick = parts[1];
                        }
                        break;
                    case "join":
                        if (IrcUtils.CheckArgs(parts, 2))
                        {
                            _activeConnection.Join(parts[1]);
                        }
                        break;
                    case "exit":
                        _quitMessage = string.Join(" ", parts.Skip(1));
                        Application.Exit();
                        break;
                    case "quit":
                        _activeConnection.Disconnect(string.Join(" ", parts.Skip(1)));
                        Debug.WriteLine("Exited Disconnect");
                        break;
                    case "server":
                    case "connect":
                        // connect irc.x.x 6667 TODO pwd
                        if (IrcUtils.CheckArgs(parts, 3))
                        {
                            var endpoint = parts[1];
                            var port = int.Parse(parts[2]);
                            _activeConnection.ChangeServer(endpoint, port);
                        }
                        break;
                    default:
                        IrcUtils.WriteOutputLine("Unknown command {0}", command);
                        break;
                }
                return;
            }

            // Raw message to the server
            if (input[0] == '|')
            {
                _activeConnection.Send(input.Substring(1));
                return;
            }

            // Default chat to current active target
            _activeConnection.Chat(input);
        }

        private void textInput_KeyDown(object sender, KeyEventArgs e)
        {

            // Inputhistory
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
                if (_historyInput.Count == 0)
                    return;

                switch (e.KeyCode)
                {
                    case Keys.Up:
                        if (--_historyCursor < 0)
                            _historyCursor = _historyInput.Count - 1;
                        break;
                    case Keys.Down:
                        if (++_historyCursor > _historyInput.Count - 1)
                            _historyCursor = 0;
                        break;
                }

                var last = _historyInput[_historyCursor];
                textInput.Text = last;
                textInput.Select(last.Length, 0);
                return;
            }

            if (!e.Control) return;
            statusLabel1.Text = e.KeyCode.ToString();
        }

        private void WIrcGui_FormClosing(object sender, FormClosingEventArgs e)
        {
            _connections.ForEach(c => c.Disconnect(_quitMessage));
        }

        private void WIrcGui_Shown(object sender, EventArgs e)
        {
            textInput.Focus();
        }
    }
}