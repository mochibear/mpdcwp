using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MPDConnectLibrary
{
    public class ListEventArgs : EventArgs
    {
        public string[] Playlist { get; private set; }

        public ListEventArgs(string[] playlist)
        {
            this.Playlist = playlist;
        }
    }
}
