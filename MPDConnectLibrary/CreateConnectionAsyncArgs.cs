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
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPDConnectLibrary
{
    public class CreateConnectionAsyncArgs : EventArgs
    {
        public bool ConnectionEstablished { get; private set; }
        public string Message { get; private set; }

        public CreateConnectionAsyncArgs(bool connectionEstablished, string message)
        {
            ConnectionEstablished = connectionEstablished;
            this.Message = message;
        }
    }
}
