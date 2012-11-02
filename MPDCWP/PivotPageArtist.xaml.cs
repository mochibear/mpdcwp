/*
 * MPDCWP - MPD Client for Windows Phone 7
 * (c) Matti Ahinko
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace MPDCWP
{
    /// <summary>
    /// PivoPageArtist class
    /// Inherits PhoneApplicationPage
    /// All info about selected artist
    /// </summary>
    public partial class PivotPageArtist : PhoneApplicationPage
    {
        /// <summary>
        /// Selected artist
        /// </summary>
        public Artist SelectedArtist { get { return (Application.Current as App).SelectedArtist; } set { (Application.Current as App).SelectedArtist = value; } }


        /// <summary>
        /// Constructor
        /// </summary>
        public PivotPageArtist()
        {
            if (SelectedArtist == null)
                GoBack();
            InitializeComponent();
            pivotMain.Title = SelectedArtist.Name;
            Loaded += new RoutedEventHandler(PivotPageArtist_Loaded);
        }


        private void PivotPageArtist_Loaded(object sender, RoutedEventArgs e)
        {
            listBoxAlbums.ItemsSource = SelectedArtist.Albums;
            listBoxTracks.ItemsSource = SelectedArtist.GetAllTracks();
        }

        private void GoBack()
        {
            SelectedArtist = null;
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void listBoxAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Album album = (Album)listBoxAlbums.SelectedItem;
            if (album == null)
                return;
            listBoxTracks.ItemsSource = album.Tracks;
            pivotMain.SelectedItem = pivotItemTracks;
        }

        private void ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null)
                return;
            FrameworkElement fe = VisualTreeHelper.GetParent(menuItem) as FrameworkElement;
            if (fe == null)
                return;
            if (fe.DataContext is Track)
            {
                (Application.Current as App).Playlist.Add((Track)fe.DataContext);
            }
            else if (fe.DataContext is Album)
            {
                foreach (Track track in ((Album)fe.DataContext).Tracks)
                {
                    (Application.Current as App).Playlist.Add(track);
                }
            }
            else if (fe.DataContext is Artist)
            {
                foreach (Track track in ((Artist)fe.DataContext).GetAllTracksAsList())
                {
                    (Application.Current as App).Playlist.Add(track);
                }
            }
        }

    }
}