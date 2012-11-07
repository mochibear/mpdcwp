/*
 * MPDCWP - MPD Client for Windows Phone 7
 * (c) Matti Ahinko 2012
 * matti.m.ahinko@student.jyu.fi
 * 
 * This file is part of MPDCWP.
 *
 * MPDCWP is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPDCWP is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with MPDCWP.  If not, see <http://www.gnu.org/licenses/>.
 *
 */
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
using System.Collections.Generic;

namespace MPDCWP
{
    /// <summary>
    /// Album class
    /// </summary>
    public class Album
    {
        // Album title
        private string title;


        // Album tracks
        private List<Track> tracks = new List<Track>();

        
        /// <summary>
        /// Title of the album
        /// </summary>
        public string Title { get { return title; } set { this.title = value; } }
                

        /// <summary>
        /// List of tracks
        /// </summary>
        public List<Track> Tracks { get { return this.tracks; } set { this.tracks = value; } }


        /// <summary>
        /// Album as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Title;
        }
    }
}
