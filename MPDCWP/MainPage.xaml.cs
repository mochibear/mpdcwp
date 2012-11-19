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
 * TODO Tarkistus, ollaanko online vai offline, siitä ilmoitus ja sen mukaan toimintojen esto
 * TODO Pollaus kappaleen vaihtoa varten yms. "currentsong"
 * TODO Soittolista ei päivity, kun lisätään search-listilta kappale
 * TODO Hyödynnä Connecting, jotta ei tule päällekkäisiä kutsuja
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
using System.Windows.Threading;

namespace MPDCWP
{
    /// <summary>
    /// MainPage class
    /// Inherits PhoneApplicationPage
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // Page loaded, something is being loaded, Is test mode enabled
        private bool loaded, loading;


        // Temporary list for downloading playlist lines
        private List<string> playlistLines = new List<string>();


        // Temporary list for downloading all tracks
        private List<string> lines = new List<string>();


        // Get all tracks event handler
        private EventHandler getAllTracksEvent;


        // Progressbar
        private ProgressIndicator progressIndicator;


        // Timer for polling current track
        private DispatcherTimer timer = new DispatcherTimer();


        /// <summary>
        /// If app is in test mode
        /// </summary>
        public bool TestMode { get; private set; }


        /// <summary>
        /// Current track
        /// </summary>
        public Track CurrentTrack
        {
            get { return (Track)GetValue(currentTrackProperty); }
            set {
                SetValue(currentTrackProperty, value);
                // TODO Bindaus
                textBlockAlbum.Text = value.Album;
                textBlockArtist.Text = value.Artist;
                textBlockSong.Text = value.Title;
            }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for currentTrack.
        /// </summary>
        public static readonly DependencyProperty currentTrackProperty =
            DependencyProperty.Register("currentTrack", typeof(Track), typeof(MainPage), new PropertyMetadata(null));



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
                this.TestMode = (bool)IsolatedStorageSettings.ApplicationSettings["testmode"];
            if (TestMode)
            {
                if (!mainControl.Items.Contains(controlTestMode))
                    mainControl.Items.Add(controlTestMode);
                if (Connection != null && Connection.TestMessagesReceived == null)
                    Connection.TestMessagesReceived += TestMessagesReceived;
                controlTestMode.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                mainControl.Items.Remove(controlTestMode);
                if (Connection != null)
                    Connection.TestMessagesReceived = null;
                controlTestMode.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (loaded)
                return;

            // TODO Voisi miettiä TryGetValue

            if (IsolatedStorageSettings.ApplicationSettings.Contains("password"))
                this.Connection.Password = (string)IsolatedStorageSettings.ApplicationSettings["password"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("server"))
                this.Connection.Server = (string)IsolatedStorageSettings.ApplicationSettings["server"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("port"))
                this.Connection.Port = (int)IsolatedStorageSettings.ApplicationSettings["port"];
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


        /// <summary>
        /// When navigated to mainpage, stop loading (progressindicator) if connection is lost
        /// Overridden
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!this.Connection.IsConnected)
                Loading(false);
            if (Playlist.Changed)
            {
                GetPlaylist();
                Playlist.Changed = false;
            }
        }


        // TODO Jotta ei ladata joka kerta kuvaa, mutta huomataan, jos albumi vaihtuu
        // Load current album image
        private void LoadCurrentImage()
        {
            try
            {
                imageDownloader1.ImagePageUrl = imageSourceUrl + CurrentTrack.Artist.ToLower().Replace(" ", "+") + "+" + CurrentTrack.Album.ToLower().Replace(" ", "+");
            }
            catch (NullReferenceException)
            {
            }
        }


        // Set playcontrol page's info texts
        private void SetInfo(Track track)
        {
            textBlockSong.Text = track.Title;
            textBlockArtist.Text = track.Artist;
            textBlockAlbum.Text = track.Album;
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
            getAllTracksEvent -= MainPage_GetAllTracks;
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
                    listBoxBrowse.ItemsSource = Artists.Values;
                });
            }
        }


        // Parses all tracks from strings to a list of Tracks
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
                        newtrack.File = item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2));
                    }
                    else if (item.StartsWith("Artist:"))
                        newtrack.Artist = item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2));
                    else if (item.StartsWith("Title:"))
                        newtrack.Title = item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2));
                    else if (item.StartsWith("Album:"))
                        newtrack.Album = item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2));
                    else if (item.StartsWith("Track:"))
                    {
                        int number = 0;
                        Int32.TryParse(item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2)), out number);
                        newtrack.Number = number;
                    }
                    else if (item.StartsWith("Time:"))
                    {
                        int number = 0;
                        Int32.TryParse(item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2)), out number);
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
        }


        /// <summary>
        /// Get playlist
        /// </summary>
        private void GetPlaylist()
        {
            Loading(true, "Loading playlist...");
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
                        newtrack.File = item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2));
                    else if (item.StartsWith("Artist:"))
                        newtrack.Artist = item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2));
                    else if (item.StartsWith("Title:"))
                        newtrack.Title = item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2));
                    else if (item.StartsWith("Album:"))
                        newtrack.Album = item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2));
                    else if (item.StartsWith("Id:"))
                    {
                        newtrack.ID = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 2));
                        tracks.Add(newtrack);
                        newtrack = new Track();
                    }
                    else if (item.StartsWith("Time:"))
                    {
                        int number = 0;
                        Int32.TryParse(item.Substring(item.IndexOf(":") + 2, item.Length - (item.IndexOf(":") + 2)), out number);
                        newtrack.Length = number;
                    }
                }
                return tracks;
            }
            return null;
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
                {
                    if (listBoxPlaylist.SelectedIndex == -1)
                        CurrentTrack = (Track)listBoxPlaylist.Items[0];
                    else
                        CurrentTrack = (Track)listBoxPlaylist.SelectedItem;

                    Connection.SendCommand(MPDClient.PLAY);
                    PlayerStatus = "Playing";
                    // TODO Tarkistus, vaihtuuko albumi
                    LoadCurrentImage();
                }
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
            // Connection.SendCommand(MPDClient.NEXT);

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
            // TODO Tee viive, ennen kuin käynnistetään haku tai sitten haun keskeytys tai mahdollisesti background workerilla, mille cancel mahdollisuus

            List<Track> result = this.Find(textBoxSearch.Text);
            Deployment.Current.Dispatcher.BeginInvoke(() => { listBoxSearch.ItemsSource = null; listBoxSearch.ItemsSource = result; });
        }


        /// <summary>
        /// Finds tracks containing given term and return results as a IEnumerableList
        /// </summary>
        /// <param name="term">Search term</param>
        /// <returns>Search results</returns>
        public List<Track> Find(string term)
        {
            term = term.ToLower();
            List<Track> tracksFound = new List<Track>();
            foreach (Track track in (Application.Current as App).AllTracks)
            {
                if (track.Find(term))
                    tracksFound.Add(track);
            }
            return tracksFound;
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
        // TODO Directory-lisääminen
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
                Connection.SendCommand("add", ((Track)fe.DataContext).File);
            }
            else if (fe.DataContext is Album)
            {
                foreach (Track track in ((Album)fe.DataContext).Tracks)
                {
                    Connection.SendCommand("add", track.File);
                }
            }
            else if (fe.DataContext is Artist)
            {
                foreach (Track track in ((Artist)fe.DataContext).GetAllTracksAsList())
                {
                    Connection.SendCommand("add", track.File);
                }
            }
            GetPlaylist();
        }


        // If not connected, but needs to play
        // After successfull connection this method is being called
        private void mpdclient_nextEventToPorform(object sender, NextEventArgs e)
        {
            Connection.SendCommand(MPDClient.PLAY);
        }


        // If page is changed, check if it is playlist and if it is, fetch playlist
        private void mainControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (mainControl.SelectedItem == pivotItemPlaylist)
            //    GetPlaylist();
        }


        // Refresh playlist
        private void appbar_buttonRefreshPlaylist_Click(object sender, EventArgs e)
        {            
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
            CurrentTrack = track;
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