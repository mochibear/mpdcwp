/*
 * PlayerControl
 * (c) Matti Ahinko 2012
 * matti.m.ahinko@student.jyu.fi
 * 
 * This file is part of PlayerControl.
 *
 * PlayerControl is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * PlayerControl is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with PlayerControl.  If not, see <http://www.gnu.org/licenses/>.
 *
 */
using System;
using System.Net;
using System.Windows;

namespace MusicPlayerLibrary
{
    /// <summary>
    /// Cancelable RoutedEventArgs
    /// WP7 Silverlight does not have RoutedEventArgs that can be cancelled so here's 
    /// </summary>
    public class CancelRoutedEventArgs : RoutedEventArgs
    {
        // Cancel event
        private bool cancel;

        /// <summary>
        /// Event cancelled
        /// </summary>        
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }
        
        
        /// <summary>
        /// Event message
        /// </summary>
        public string Message { get; private set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public CancelRoutedEventArgs() { }


        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message">Message</param>
        public CancelRoutedEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
