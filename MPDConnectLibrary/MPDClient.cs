/*
 * MPDClient
 * (c) Matti Ahinko
 * matti.m.ahinko@student.jyu.fi
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
    public class MPDClient
    {
        public static string NEXT = "next";
        public static string PREVIOUS = "previous";
        public static string SETVOL = "setvol";
        public static string STOP = "stop";
        public static string PLAY = "play";
        public static string PAUSE = "pause";
        public static string SEEK = "seek";

        public event EventHandler<NextEventArgs> NextEventToPorform;

        private NextEventArgs nextArgs;

        public NextEventArgs NextArgs
        {
            get { return nextArgs; }
            set { nextArgs = value; }
        }
        

        //public event EventHandler CommandFailed;

        public event EventHandler<CreateConnectionAsyncArgs> CreateConnectionCompleted;

        private Socket connection;

        private string address;

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        private int port;

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        private string username;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        private string password;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public bool IsConnected
        {
            get { if (this.connection != null) return this.connection.Connected; else return false; }            
        }

        public void Connect()
        {
            this.Connect(this.address, this.port);
        }

        public void Connect(string serverAddress, int port)
        {
            this.connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var connectionOperation = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(serverAddress, port) };
            connectionOperation.Completed += OnConnectionToServerCompleted;            
            this.connection.ConnectAsync(connectionOperation);
        }

        private void OnConnectionToServerCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                if (CreateConnectionCompleted != null)
                    CreateConnectionCompleted(this, new CreateConnectionAsyncArgs(false));
                if (NextEventToPorform != null && nextArgs != null)
                {
                    NextEventToPorform(this, nextArgs);
                    this.NextEventToPorform = null;
                }
                return;
            }

            if (CreateConnectionCompleted != null)
                CreateConnectionCompleted(this, new CreateConnectionAsyncArgs(true));
        }



        public void Disconnect()
        {
            this.connection.Close();
        }

        public void SendCommand(string command, string[] attributes = null)
        {
            if (this.IsConnected)
            {
                StringBuilder sb = new StringBuilder(command);
                if (attributes != null)
                {
                    string space = "";
                    foreach (string attr in attributes)
                    {
                        sb.Append(space + attr);
                        space = " ";
                    }
                }
                var asyncEvent = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(this.address, this.port) };

                var buffer = Encoding.UTF8.GetBytes(sb.ToString() + Environment.NewLine);
                asyncEvent.SetBuffer(buffer, 0, buffer.Length);
                connection.SendAsync(asyncEvent);
            }
            else
            {               
                this.nextArgs = new NextEventArgs(command, attributes);
                this.NextEventToPorform += MPDClient_NextEventToPorform;
                this.Connect();
            }
        }

        void MPDClient_NextEventToPorform(object sender, NextEventArgs e)
        {
            this.SendCommand(e.Command, e.Attributes);
        }
        
    }
}
