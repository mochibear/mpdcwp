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
    /// Artist class
    /// </summary>
    public class Artist
    {
        private string name;
        private List<Album> albums = new List<Album>();


        /// <summary>
        /// Name of the artist
        /// </summary>
        public string Name { get { return name; } set { this.name = value; } }
             

        /// <summary>
        /// Albums of the artist
        /// </summary>
        public List<Album> Albums { get { return this.albums; } set { this.albums = value; } }


        /// <summary>
        /// Returns all tracks in all albums a IEnumerable
        /// </summary>
        /// <returns>All tracks</returns>
        public System.Collections.IEnumerable GetAllTracks()
        {
            return GetAllTracksAsList();
        }


        /// <summary>
        /// Returns all tracks in all albums as a list
        /// </summary>
        /// <returns></returns>
        public List<Track> GetAllTracksAsList()
        {
            List<Track> tracks = new List<Track>();
            foreach (Album album in albums)
            {
                tracks.AddRange(album.Tracks);
            }
            return tracks;
        }
    }
}
