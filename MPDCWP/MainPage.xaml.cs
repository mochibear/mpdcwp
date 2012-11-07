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
 * 
 * TODO Status/State mukaan, onko soitto päällä vai ei
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
using MusicPlayerLibrary;
using MPDConnectLibrary;
using System.IO.IsolatedStorage;

namespace MPDCWP
{
    /// <summary>
    /// MainPage class
    /// Inherits PhoneApplicationPage
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // Page loaded
        private bool loaded;


        // Connection to the server
        public MPDClient Connection
        {
            get { return (Application.Current as App).Connection; }
            set { (Application.Current as App).Connection = value; }
        }


        /// <summary>
        /// Playlist
        /// </summary>
        public PlaylistCollection Playlist
        {
            get { return (Application.Current as App).Playlist; }
            set { (Application.Current as App).Playlist = value; }
        }


        /// <summary>
        /// Status of the player
        /// </summary>
        public string PlayerStatus { get { return textBlockStatus.Text; } set { textBlockStatus.Text = value; } }


        /// <summary>
        /// All artists
        /// </summary>
        public List<Artist> Artists { get { return (Application.Current as App).Artists; } set { (Application.Current as App).Artists = value; } }


        /// <summary>
        /// Selected artist
        /// </summary>
        public Artist SelectedArtist { get { return (Application.Current as App).SelectedArtist; } set { (Application.Current as App).SelectedArtist = value; } }


        /// <summary>
        /// tmp
        /// </summary>
        private string imageSourceUrl = "http://www.amazon.com/s/ref=nb_sb_noss?url=search-alias%3Daps&field-keywords=";


        /// <summary>
        /// Constructor
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            listBoxPlaylist.ItemsSource = Playlist;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }


        // Page loaded
        // Load settings
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded)
                return;
            this.AddDemoData();
            listBoxBrowse.ItemsSource = Artists;
            listBoxSearch.ItemsSource = Artists;
            imageDownloader1.ImagePageUrl = imageSourceUrl + "anathema+weather+systems";

            // TODO TryGetValue

            if (IsolatedStorageSettings.ApplicationSettings.Contains("username"))
                this.Connection.Username = (string)IsolatedStorageSettings.ApplicationSettings["username"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("password"))
                this.Connection.Password = (string)IsolatedStorageSettings.ApplicationSettings["password"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("server"))
                this.Connection.Server = (string)IsolatedStorageSettings.ApplicationSettings["server"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("port"))
                this.Connection.Port = (int)IsolatedStorageSettings.ApplicationSettings["port"];
            this.Connection.CreateConnectionCompleted += Connection_CreateConnectionCompleted;
            this.Connection.CreateConnectionFailed += Connection_CreateConnectionFailed;
            if (!Connection.IsConnected && (!IsolatedStorageSettings.ApplicationSettings.Contains("autoconnect") || !(bool)IsolatedStorageSettings.ApplicationSettings["autoconnect"]))
                NavigationService.Navigate(new Uri("/PageSettings.xaml", UriKind.Relative));
            else
                this.Connection.Connect();
            this.loaded = true;
        }


        // If connection fails
        private void Connection_CreateConnectionFailed(object sender, CreateConnectionAsyncArgs e)
        {
            MessageBox.Show("Connection failed: " + e.Message);
        }


        // If connection is success
        private void Connection_CreateConnectionCompleted(object sender, CreateConnectionAsyncArgs e)
        {
            PlayerStatus = "Connection established to " + Connection.Server;
        }


        // Add demo data
        private void AddDemoData()
        {
            string[] names = new string[] { "Anathema", "Eva & Manu", "In flames", "Opeth", "Pain", "Porcupine Tree", "Storm Corrosion", "Soilwork" };
            foreach (string name in names)
            {
                Artist artist = new Artist();
                artist.Name = name;
                artist.Albums.Add(new Album() { Title = "Albumi 1", Tracks = new List<Track> { new Track() { Number = 1, Title = "Kappale 1", Album = "Albumi 1", Artist = artist.Name }, new Track() { Number = 2, Title = "Kappale 2", Album = "Albumi 1", Artist = artist.Name } } });
                artist.Albums.Add(new Album() { Title = "Albumi 2", Tracks = new List<Track> { new Track() { Number = 1, Title = "Kappale 1", Album = "Albumi 2", Artist = artist.Name }, new Track() { Number = 2, Title = "Kappale 2", Album = "Albumi 2", Artist = artist.Name } } });
                Artists.Add(artist);
            }
        }


        // Player control pauses
        private void playerControl_Pause(object sender, EventArgs e)
        {
            Connection.SendCommand(MPDClient.PAUSE);
            PlayerStatus = "Paused";
        }


        // Player control previous
        private void playerControl_Previous(object sender, EventArgs e)
        {
            Connection.SendCommand(MPDClient.PREVIOUS);
            PlayerStatus = "Previous";
        }


        // Player control pauses
        private void playerControl_Rewind(object sender, EventArgs e)
        {
            PlayerStatus = "Rewind";
        }


        // Player control stop
        private void playerControl_Stop(object sender, EventArgs e)
        {
            Connection.SendCommand(MPDClient.STOP);
            PlayerStatus = "Stopped";
        }


        // Player control next
        private void playerControl_Next(object sender, EventArgs e)
        {
            Connection.SendCommand(MPDClient.NEXT);
            PlayerStatus = "Next";
        }


        // Player control forward
        private void playerControl_Forward(object sender, EventArgs e)
        {
            PlayerStatus = "Forward";
        }


        // Player control volume changed
        private void playerControl_VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Connection.SendCommand(MPDClient.SETVOL, e.NewValue + "");
            PlayerStatus = "VolumeChanged: " + e.NewValue;
        }


        // Browse Selection changed
        private void listBoxBrowse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Artist artist = (Artist)listBoxBrowse.SelectedItem;
            if (artist == null)
                return;
            SelectedArtist = artist;
            NavigationService.Navigate(new Uri("/PivotPageArtist.xaml", UriKind.Relative)); // voisi miettiä myös esim. id:n viemistä urlissa
        }


        // Search textbox text changed
        // Perform quick search
        private void textBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO Tee viive, ennen kuin käynnistetään haku tai sitten haun keskeytys
            //this.Find(textBoxSearch.Text);
        }


        // Open settings
        private void appbar_buttonSettings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PageSettings.xaml", UriKind.Relative));
        }


        // Open info
        private void appbar_buttonInfo_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PageInfo.xaml", UriKind.Relative));
        }


        // Context menu clicked
        // If selection is a track, add it to the playlist.
        // If it is an album add all tracks to the playlist
        // If it is an artist add all tracks in all albums to the playlist
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
                this.Playlist.Add((Track)fe.DataContext);
            }
            else if (fe.DataContext is Album)
            {
                foreach (Track track in ((Album)fe.DataContext).Tracks)
                {
                    this.Playlist.Add(track);
                }
            }
            else if (fe.DataContext is Artist)
            {
                foreach (Track track in ((Artist)fe.DataContext).GetAllTracksAsList())
                {
                    this.Playlist.Add(track);
                }
            }
        }


        /// <summary>
        /// PlayerControl play-handler
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void playerControl_Play(object sender, CancelRoutedEventArgs e)
        {
            if (Playlist.Count == 0)
            {
                MessageBox.Show("No songs in playlist");
                e.Cancel = true;
            }
            else
            {
                if (this.playerControl.Playing)
                    Connection.SendCommand(MPDClient.PAUSE);
                else
                    Connection.SendCommand(MPDClient.PLAY);
                PlayerStatus = "Playing";
            }
        }


        // If not connected, but needs to play
        // After successfull connection this method is being called
        private void mpdclient_nextEventToPorform(object sender, NextEventArgs e)
        {
            Connection.SendCommand(MPDClient.PLAY);
        }

        
        /// <summary>
        /// Finds tracks containing given term and return results as a IEnumerableList
        /// </summary>
        /// <param name="term">Search term</param>
        /// <returns>Search results</returns>
        public IEnumerable<Track> Find(string term)
        {
            //str = str.ToLower();
            //List<Artist> artists = new List<Artist>();
            //List<Track> tracks = new List<Track>();
            //List<Album> albums = new List<Album>();
            //foreach (Artist artist in Artists)
            //{
            //    if (artist.Name.ToLower().Contains(str))
            //        artists.Add(artist);
            //    foreach (Album album in artist.Albums)
            //    {
            //        if (album.Title.ToLower().Contains(str))
            //            albums.Add(album);
            //        foreach (Track track in album.Tracks)
            //        {
            //            if (track.Title.ToLower().Contains(str))
            //                tracks.Add(track);
            //        }
            //    }
            //}

            //listBoxSearchArtist.ItemsSource = artists;
            //listBoxSearchAlbum.ItemsSource = albums;
            //listBoxSearchTrack.ItemsSource = tracks;
            return null;
        }
    }
}