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

namespace MPDCWP
{
    /// <summary>
    /// Track class
    /// </summary>
    public class Track
    {

        // Album
        // "No album" is default
        private string album = "No album";


        // Artist
        // "No artist" is default
        private string artist = "No artist";


        // Track title
        // "No title" is default
        private string title = "No title";


        /// <summary>
        /// Track ID in playlist
        /// </summary>
        public string ID { get; set; }


        /// <summary>
        /// Track length
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Track file name
        /// </summary>
        public string File { get; set; }
        
        
        /// <summary>
        /// Track number
        /// </summary>
        public int Number { get; set; }

        
        /// <summary>
        /// Track title
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }



        /// <summary>
        /// Track as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Title;
        }
        

        /// <summary>
        /// Track artist
        /// </summary>
        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }


        /// <summary>
        /// Track album
        /// </summary>
        public string Album
        {
            get { return album; }
            set { album = value; }
        }
        
    }
}
