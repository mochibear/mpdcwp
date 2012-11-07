using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPDConnectLibrary
{
    public class NextEventArgs : EventArgs
    {
        public string Command;
        public string[] Attributes;

        public NextEventArgs(string command, string[] attributes)
        {
            this.Command = command;
            this.Attributes = attributes;
        }
    }
}
