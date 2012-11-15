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
 * TODO Hae soitettavan kappaleen indeksi
 * TODO Uudelleenyhdistettäessä voisi miettiä muuttujien tyhjäämistä, jos esimerkiksi palvelinta on muutettu
 * TODO Jos refreshaa playlistin ladataan myös database
 * 
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
using Microsoft.Phone.Shell;

namespace MPDCWP
{
    /// <summary>
    /// MainPage class
    /// Inherits PhoneApplicationPage
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // Page loaded, something is being loaded, Is test mode enabled
        private bool loaded, loading, testmode;

        
        // Temporary list for downloading playlist lines
        private List<string> playlistLines = new List<string>();


        // Temporary list for downloading all tracks
        private List<string> lines = new List<string>();


        // Get all tracks event handler
        private EventHandler getAllTracksEvent;
        

        // Index of current track in playlist
        private int currentTrack;


        // Progressbar
        private ProgressIndicator progressIndicator;


        /// <summary>
        /// Server connection socket
        /// </summary>
        public MPDClient Connection
        {
            get { return (Application.Current as App).Connection; }
            set { (Application.Current as App).Connection = value; }
        }


        /// <summary>
        /// All tracks from database
        /// </summary>
        public List<Track> AllTracks
        {
            get { return (Application.Current as App).AllTracks; }
            set { (Application.Current as App).AllTracks = value; }
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
        public Dictionary<string, Artist> Artists { get { return (Application.Current as App).Artists; } set { (Application.Current as App).Artists = value; } }


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
            SystemTray.SetIsVisible(this, false);
            SystemTray.SetOpacity(this, 0.5);
            progressIndicator = new ProgressIndicator();
            progressIndicator.IsVisible = false;
            progressIndicator.IsIndeterminate = true;
            progressIndicator.Text = "Loading...";
            SystemTray.SetProgressIndicator(this, progressIndicator);
        }


        // Page loaded
        // Load settings
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("testmode"))
                this.testmode = (bool)IsolatedStorageSettings.ApplicationSettings["testmode"];
            if (testmode)
            {
                if (Connection != null && Connection.TestMessagesReceived == null)
                    Connection.TestMessagesReceived += TestMessagesReceived;
                controlTestMode.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                if (Connection != null)
                    Connection.TestMessagesReceived = null;
                controlTestMode.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (loaded)
                return;
            //this.AddDemoData();
            listBoxBrowse.ItemsSource = Artists;
            listBoxSearch.ItemsSource = Artists;
            // TODO TryGetValue

            if (IsolatedStorageSettings.ApplicationSettings.Contains("password"))
                this.Connection.Password = (string)IsolatedStorageSettings.ApplicationSettings["password"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("server"))
                this.Connection.Server = (string)IsolatedStorageSettings.ApplicationSettings["server"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("port"))
                this.Connection.Port = (int)IsolatedStorageSettings.ApplicationSettings["port"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("testmode"))
                this.testmode = (bool)IsolatedStorageSettings.ApplicationSettings["testmode"];
            this.Connection.CreateConnectionCompleted += Connection_CreateConnectionCompleted;
            this.Connection.CreateConnectionFailed += Connection_CreateConnectionFailed;

            if (!Connection.IsConnected && (!IsolatedStorageSettings.ApplicationSettings.Contains("autoconnect") || !(bool)IsolatedStorageSettings.ApplicationSettings["autoconnect"]))
            {
                if (MessageBox.Show("Not connected to server. Connect now?", "Connection", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    NavigationService.Navigate(new Uri("/PageSettings.xaml", UriKind.Relative));
            }
            else
            {
                this.Loading(true, "Connecting to server...");
                this.Connection.Connect();
            }
            this.loaded = true;
        }


        // Override
        // When navigated to mainpage, stop loading (progressindicator) if connection is lost
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!this.Connection.IsConnected)
                Loading(false);
        }


        // TODO Jotta ei ladata joka kerta kuvaa, mutta huomataan, jos albumi vaihtuu
        // Load current album image
        private void LoadCurrentImage()
        {
            if (Playlist.Count > 0)
            {
                Track track = Playlist[currentTrack % Playlist.Count];
                imageDownloader1.ImagePageUrl = imageSourceUrl + track.Artist.ToLower().Replace(" ", "+") + "+" + track.Album.ToLower().Replace(" ", "+");
            }
        }


        // Loading indicator
        private void Loading(bool loading, string text = "Loading...")
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.loading = loading;
                progressIndicator.Text = text;
                SystemTray.SetIsVisible(this, loading);
                progressIndicator.IsVisible = loading;
            });
        }


        // If connection fails
        private void Connection_CreateConnectionFailed(object sender, CreateConnectionAsyncArgs e)
        {
            this.Loading(false);
            MessageBox.Show("Connection failed: " + e.Message);
        }


        // If connection is success
        private void Connection_CreateConnectionCompleted(object sender, CreateConnectionAsyncArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                PlayerStatus = "Connection established to " + Connection.Server;
            });
            this.Loading(true, "Loading playlist");
            Deployment.Current.Dispatcher.BeginInvoke(() => { Playlist.Clear(); });
            getAllTracksEvent += MainPage_GetAllTracks;
            GetPlaylist();
        }


        // Get all tracks handler
        private void MainPage_GetAllTracks(object sender, EventArgs e)
        {
            this.GetAllTracks();
        }


        // Get all tracks
        private void GetAllTracks()
        {
            Loading(true, "Downloading database...");
            lines.Clear();
            Connection.MessagePass += AllTracksFetched;
            Connection.SendCommand("listallinfo");
        }

        
        // When all tracks is fetched
        private void AllTracksFetched(object sender, MessageArrayEventArgs e)
        {
            lines.AddRange(e.MessageArray);
            if (e.MessageArray.Contains("OK"))
            {
                List<Track> tracks = ParseAllTracks(lines);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    foreach (Track track in tracks)
                    {
                        AllTracks.Add(track);
                    }
                });
                Connection.MessagePass -= AllTracksFetched;
                Loading(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    listBoxSearch.ItemsSource = null;
                    listBoxSearch.ItemsSource = AllTracks;
                    DivideTracks();
                });
            }
        }


        // TODO Voisi miettiä viitteillä toteuttamista myös
        // TODO directory: -> artist/album tällöin voi lisätä myös koko kansiollisen samalla add-käskyllä
        private List<Track> ParseAllTracks(List<string> strings)
        {
            if (strings != null)
            {
                List<Track> tracks = new List<Track>();
                Track newtrack = null;
                foreach (string item in strings)
                {
                    if (item.StartsWith("directory:"))
                        continue;
                    if (item.StartsWith("file:"))
                    {
                        if (newtrack != null)
                            tracks.Add(newtrack);
                        newtrack = new Track();
                        newtrack.File = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                    }
                    else if (item.StartsWith("Artist:"))
                        newtrack.Artist = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                    else if (item.StartsWith("Title:"))
                        newtrack.Title = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                    else if (item.StartsWith("Album:"))
                        newtrack.Album = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                    else if (item.StartsWith("Track:"))
                    {
                        int number = 0;
                        Int32.TryParse(item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1)), out number);
                        newtrack.Number = number;
                    }
                    else if (item.StartsWith("Time:"))
                    {
                        int number = 0;
                        Int32.TryParse(item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1)), out number);
                        newtrack.Length = number;
                    }
                    else if (item.StartsWith("Genre:"))
                    {

                    }
                }
                return tracks;
            }
            return null;
        }


        // Divide tracks into albums and artists
        // TODO muuta parsintakohtaan
        private void DivideTracks()
        {
            Artists.Clear();
            Artist lastArtist = null; // Nopeuttamaan hakua
            foreach (Track track in AllTracks)
            {
                if (lastArtist != null && lastArtist.Name.Equals(track.Artist))
                {
                    lastArtist.AddSong(track);
                }
                else
                {
                    if (Artists.TryGetValue(track.Artist, out lastArtist))
                    {
                        lastArtist.AddSong(track);
                    }
                    else
                    {
                        lastArtist = new Artist() { Name = track.Artist };
                        lastArtist.AddSong(track);
                        Artists.Add(track.Artist, lastArtist);
                    }
                }
            }



            //    bool artistFound = false;




            //    foreach (Artist artist in Artists)
            //    {
            //        if (artist.Name.Equals(track.Artist))
            //        {
            //            bool albumFound = false;
            //            foreach (Album album in artist.Albums)
            //            {
            //                if (album.Title.Equals(track.Album))
            //                {
            //                    album.Tracks.Add(track);
            //                    albumFound = true;
            //                    break;
            //                }
            //                if (!albumFound)
            //                {
            //                    Album newalbum = new Album() { Title = track.Album };
            //                    newalbum.Tracks.Add(track);
            //                    artist.Albums.Add(newalbum);
            //                }
            //            }
            //            artistFound = true;
            //            break;
            //        }
            //    }
            //    if (!artistFound)
            //    {
            //        Artist newartist = new Artist() { Name = track.Artist };
            //        Artists.Add(newartist);
            //        Album album = new Album() { Title = track.Album };
            //        newartist.Albums.Add(album);
            //        album.Tracks.Add(track);
            //    }
            //}
            //listBoxBrowse.ItemsSource = null;
            //listBoxBrowse.ItemsSource = Artists;
        }


        // Possible sort method for all tracks
        private void SortTracks()
        {
            // Sort
        }

        /// <summary>
        /// Get playlist
        /// </summary>
        private void GetPlaylist()
        {
            Connection.MessagePass += PlaylistFetched;
            playlistLines.Clear();
            Connection.SendCommand("playlistinfo");
        }
                

        // After playlist is fetched
        // TODO Entä jos on tyhjä? Tällöin palautetaan pelkkä OK. Mutta jos palautetaankin jonkun muun komenonn tulokset välissä.
        // TODO Null check
        private void PlaylistFetched(object sender, MessageArrayEventArgs e)
        {
            playlistLines.AddRange(e.MessageArray);
            if (e.MessageArray.Contains("OK"))
            {                
                List<Track> tracks = ParseTracks(e.MessageArray);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Playlist.Clear();
                    foreach (Track track in tracks)
                    {
                        Playlist.Add(track);
                    }
                });
                Connection.MessagePass -= PlaylistFetched;
                Loading(false);
                if (getAllTracksEvent != null)
                    getAllTracksEvent(this, new EventArgs());
            }
        }


        // TODO Voisi miettiä viitteillä toteuttamista myös
        // Parse tracks from string array
        private List<Track> ParseTracks(string[] strings)
        {
            if (strings != null)
            {
                List<Track> tracks = new List<Track>();
                Track newtrack = new Track();
                foreach (string item in strings)
                {
                    if (item.StartsWith("file:"))
                        newtrack.File = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                    else if (item.StartsWith("Artist:"))
                        newtrack.Artist = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                    else if (item.StartsWith("Title:"))
                        newtrack.Title = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                    else if (item.StartsWith("Album:"))
                        newtrack.Album = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                    else if (item.StartsWith("Id:"))
                    {
                        newtrack.ID = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                        tracks.Add(newtrack);
                        newtrack = new Track();
                    }
                    else if (item.StartsWith("Time:"))
                    {
                        int number = 0;
                        Int32.TryParse(item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1)), out number);
                        newtrack.Length = number;
                    }
                }
                return tracks;
            }
            return null;
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
                // TODO Tarkistus, vaihtuuko albumi
                LoadCurrentImage();
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

        // If page is changed, check if it is playlist and if it is, fetch playlist
        private void mainControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (mainControl.SelectedItem == pivotItemBrowse && !artistsDivided)
            //    DivideTracks();
        }


        // Refresh playlist
        private void appbar_buttonRefreshPlaylist_Click(object sender, EventArgs e)
        {
            Loading(true, "Loading playlist...");
            GetPlaylist();
        }


        // Selection changed in playlist
        // Play selected song
        private void listBoxPlaylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxPlaylist.SelectedIndex == -1)
                return;
            Track track = (Track)listBoxPlaylist.SelectedItem;
            if (track == null)
                return;
            currentTrack = listBoxPlaylist.SelectedIndex;
            Connection.SendCommand("playid", track.ID);
        }


        // Test messages received
        private void TestMessagesReceived(object sender, MessageArrayEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (listBoxTestOutput.Items.Count > 500)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        listBoxTestOutput.Items.RemoveAt(0);
                    }
                }
                foreach (string item in e.MessageArray)
                {
                    listBoxTestOutput.Items.Add(item);
                }
            });
        }


        // Download database
        private void appbar_buttonUpdateDatabase_Click(object sender, EventArgs e)
        {
            AllTracks.Clear();
            GetAllTracks();
        }


        // Context menu, clear playlist
        private void ContextMenuItem_ClickClearPlaylist(object sender, RoutedEventArgs e)
        {
            Connection.SendCommand("clear");
            Playlist.Clear();
        }


        // Conext menu, remove object
        private void ContextMenuItem_ClickRemove(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null)
                return;
            FrameworkElement fe = VisualTreeHelper.GetParent(menuItem) as FrameworkElement;
            if (fe == null)
                return;
            if (fe.DataContext is Track)
            {
                Connection.SendCommand("delete", (fe.DataContext as Track).ID);
                // TODO Nyt poistaa ensimmäisen olion, joka vastaa tätä. Voisi poistaa juuri sen oikean, jos esim. on useampia esiintymiä.
                Playlist.Remove((fe.DataContext as Track));
            }
        }
    }
}