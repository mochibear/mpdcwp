using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPDConnectLibrary
{
    public class CreateConnectionAsyncArgs : EventArgs
    {
        public bool ConnectionEstablished { get; private set; }

        public CreateConnectionAsyncArgs(bool connectionEstablished)
        {
            ConnectionEstablished = connectionEstablished;
        }
    }
}
