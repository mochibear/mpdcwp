/*
 * MPDClient
 * (c) Matti Ahinko 2012
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
 * 
 * TODO status-komennolla state
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
using System.Collections.Generic;

namespace MPDConnectLibrary
{
    public class MPDClient
    {
        // Buffer size
        private const int bufferSize = 2048;


        // Next task args (command)
        private NextEventArgs nextArgs;


        // Trailing message for messages listener
        private string trailingMessage;


        // Connection socket
        private Socket connection;


        /// <summary>
        /// After message is received pass it on
        /// </summary>
        public EventHandler<MessageArrayEventArgs> MessagePass;


        /// <summary>
        /// After message is received pass it on for test mode
        /// </summary>
        public EventHandler<MessageArrayEventArgs> TestMessagesReceived;
        

        /// <summary>
        /// Next command
        /// </summary>
        public static string NEXT = "next";


        /// <summary>
        /// Previous command
        /// </summary>
        public static string PREVIOUS = "previous";


        /// <summary>
        /// Set volume command
        /// </summary>
        public static string SETVOL = "setvol";


        /// <summary>
        /// Stop command
        /// </summary>
        public static string STOP = "stop";


        /// <summary>
        /// Play command
        /// </summary>
        public static string PLAY = "play";


        /// <summary>
        /// Pause command
        /// </summary>
        public static string PAUSE = "pause";


        /// <summary>
        /// Seek command
        /// </summary>
        public static string SEEK = "seek";


        /// <summary>
        /// Next task to perform after connection
        /// </summary>
        public event EventHandler<NextEventArgs> NextTaskToPorform;


        /// <summary>
        /// If message is received from server
        /// </summary>
        public event EventHandler<MessageArrayEventArgs> MessageReceived;


        /// <summary>
        /// Next task args
        /// </summary>        
        public NextEventArgs NextArgs { get; set; }


        /// <summary>
        /// Connection established
        /// </summary>
        public event EventHandler<CreateConnectionAsyncArgs> CreateConnectionCompleted;


        /// <summary>
        /// Connection failed
        /// </summary>
        public event EventHandler<CreateConnectionAsyncArgs> CreateConnectionFailed;


        /// <summary>
        /// Server address
        /// </summary>
        public string Server { get; set; }


        /// <summary>
        /// Server port
        /// </summary>
        public int Port { get; set; }


        /// <summary>
        /// Server username
        /// </summary>
        public string Username { get; set; }


        /// <summary>
        /// Server password
        /// </summary>
        public string Password { get; set; }


        /// <summary>
        /// Is client connected to a server
        /// </summary>
        public bool IsConnected
        {
            get { if (this.connection != null) return this.connection.Connected; else return false; }
        }


        /// <summary>
        /// Connect to the server with predefined address and port
        /// </summary>
        public void Connect()
        {
            this.Connect(this.Server, this.Port);
        }


        /// <summary>
        /// Connect to the server
        /// </summary>
        /// <param name="serverAddress">Server address</param>
        /// <param name="port">Server port</param>
        public void Connect(string serverAddress, int port)
        {
            if (serverAddress == null)
                return;
            if (this.IsConnected)
                this.Disconnect();
            this.connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs connectionOperation = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(serverAddress, port) };
            connectionOperation.Completed += OnConnectionToServerCompleted;            
            this.connection.ConnectAsync(connectionOperation);
        }


        // Connection completed
        private void OnConnectionToServerCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                if (CreateConnectionFailed != null)
                    CreateConnectionFailed(this, new CreateConnectionAsyncArgs(false, e.SocketError.ToString()));

                return;
            }
            
            if (NextTaskToPorform != null && nextArgs != null)
            {
                NextTaskToPorform(this, nextArgs);
                this.NextTaskToPorform = null;
            }
            else if (CreateConnectionCompleted != null)
                CreateConnectionCompleted(this, new CreateConnectionAsyncArgs(true, "Success"));
            StartReceivingMessages();
        }


        /// <summary>
        /// Disconnect
        /// Close socket and release all associated resources
        /// </summary>
        public void Disconnect()
        {
            this.connection.Close();
        }


        /// <summary>
        /// Send given command and a parameter to the server
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="singleAttribute">Parameter</param>
        public void SendCommand(string command, string singleAttribute)
        {
            this.SendCommand(command, new string[] { singleAttribute });
        }


        /// <summary>
        /// Start receiving messages
        /// </summary>
        public void StartReceivingMessages()
        {            
            SocketAsyncEventArgs responseListener = new SocketAsyncEventArgs();
            responseListener.Completed += OnMessageReceivedFromServer;
            byte[] responseBuffer = new byte[bufferSize];
            responseListener.SetBuffer(responseBuffer, 0, bufferSize);
            connection.ReceiveAsync(responseListener);
        }


        /// <summary>
        /// Send given command and parameters to the server
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="attributes">Attributes</param>
        public void SendCommand(string command, string[] attributes = null)
        {
            if (this.IsConnected)
            {
                StringBuilder sb = new StringBuilder(command);
                if (attributes != null)
                {                    
                    foreach (string attr in attributes)
                    {
                        sb.Append(" " + attr);
                    }
                }
                SocketAsyncEventArgs asyncEvent = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(this.Server, this.Port) };

                byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString() + Environment.NewLine);
                
                    asyncEvent.Completed += asyncEvent_Completed;
                asyncEvent.SetBuffer(buffer, 0, buffer.Length);
                connection.SendAsync(asyncEvent);
            }
            else
            {
                this.nextArgs = new NextEventArgs(command, attributes);
                this.NextTaskToPorform += MPDClient_NextTaskToPorform;
                this.Connect();
            }
        }


        // Asynchronic command sending completed
        private void asyncEvent_Completed(object sender, SocketAsyncEventArgs e)
        {

        }

        
        // When message received from server
        private void OnMessageReceivedFromServer(object sender, SocketAsyncEventArgs e)
        {           
            string message = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);

            if (!string.IsNullOrWhiteSpace(trailingMessage))
            {
                message = trailingMessage + message;
                trailingMessage = null;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                // Connection lost            
                return;
            }

            // Convert the received string into a string array
            List<string> lines = new List<string>(message.Split("\n\r".ToCharArray(), StringSplitOptions.None));

            if (lines.Count > 0)
            {
                string lastLine = lines[lines.Count - 1];
                bool isBufferFull = !string.IsNullOrWhiteSpace(lastLine);
                if (isBufferFull)
                {
                    trailingMessage = lastLine;
                    lines.Remove(lastLine);
                }

                MessageArrayEventArgs args = new MessageArrayEventArgs(lines.ToArray());
                if (MessagePass != null)
                {
                    MessagePass(this, args);                    
                }
                if (MessageReceived != null)
                {
                    MessageReceived(this, args); 
                }
                if (TestMessagesReceived != null)
                {
                    TestMessagesReceived(this, args);
                }
                
            }
            // Start listening for the next message
            StartReceivingMessages();
        }


        // Next task after successful connection
        private void MPDClient_NextTaskToPorform(object sender, NextEventArgs e)
        {
            this.SendCommand(e.Command, e.Attributes);
        }


        /// <summary>
        /// Get status of mpd
        /// Not implemented
        /// </summary>
        /// <param name="key">Status key</param>
        /// <returns>Required info</returns>
        public string GetStatus(string key)
        {
            return "pause"; // testidata
        }
    }
}
