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
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using MPDConnectLibrary;

namespace MPDCWP
{
    /// <summary>
    /// PageSettings class
    /// Inherits PhoneApplicationPage
    /// Application settings
    /// </summary>
    public partial class PageSettings : PhoneApplicationPage
    {
        // Page is loaded, Values changed
        private bool loaded = false, valuesChanged = false;

        private MPDClient connection;        

        /// <summary>
        /// Constructor
        /// </summary>
        public PageSettings()
        {
            InitializeComponent();
            this.connection = (Application.Current as App).Connection;
            // TODO voisi toteuttaa TryGetValuella?
            if (IsolatedStorageSettings.ApplicationSettings.Contains("savepassword"))
                checkBoxSavePassword.IsChecked = (bool)IsolatedStorageSettings.ApplicationSettings["savepassword"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("autoconnect"))
                checkBoxAutoConnect.IsChecked = (bool)IsolatedStorageSettings.ApplicationSettings["autoconnect"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("testmode"))
                checkBoxTestMode.IsChecked = (bool)IsolatedStorageSettings.ApplicationSettings["testmode"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("server"))
                textBoxServer.Text = (string)IsolatedStorageSettings.ApplicationSettings["server"];
            if (IsolatedStorageSettings.ApplicationSettings.Contains("port"))
                textBoxPort.Text = (int)IsolatedStorageSettings.ApplicationSettings["port"] + "";
            if (IsolatedStorageSettings.ApplicationSettings.Contains("password"))
                textBoxPassword.Text = (string)IsolatedStorageSettings.ApplicationSettings["password"];

            buttonDisconnect.IsEnabled = this.connection.IsConnected;
        }


        // Button connect clicked
        // If values are changed use new values in connection
        // Connect to the server
        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!textBoxServer.Text.Equals("") && !textBoxPort.Text.Equals(""))
            {
                if (valuesChanged)
                {
                    connection.Password = textBoxPassword.Text;
                    connection.Server = textBoxServer.Text;
                    int value;
                    if (Int32.TryParse(textBoxPort.Text, out value))
                        connection.Port = value;                    
                }

                connection.CreateConnectionCompleted += Connection_CreateConnectionCompleted;
                connection.Connect();
            }
        }


        // If connection is established we can go back to the main page
        private void Connection_CreateConnectionCompleted(object sender, MPDConnectLibrary.CreateConnectionAsyncArgs e)
        {            
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                buttonDisconnect.IsEnabled = this.connection.IsConnected;
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            });

        }


        /// <summary>
        /// Leaving the page
        /// Saving settings if changed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (loaded && valuesChanged)
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("savepassword"))
                    IsolatedStorageSettings.ApplicationSettings["savepassword"] = checkBoxSavePassword.IsChecked;
                else
                    IsolatedStorageSettings.ApplicationSettings.Add("savepassword", checkBoxSavePassword.IsChecked);

                if (IsolatedStorageSettings.ApplicationSettings.Contains("autoconnect"))
                    IsolatedStorageSettings.ApplicationSettings["autoconnect"] = checkBoxAutoConnect.IsChecked;
                else
                    IsolatedStorageSettings.ApplicationSettings.Add("autoconnect", checkBoxAutoConnect.IsChecked);

                if (IsolatedStorageSettings.ApplicationSettings.Contains("testmode"))
                    IsolatedStorageSettings.ApplicationSettings["testmode"] = checkBoxTestMode.IsChecked;
                else
                    IsolatedStorageSettings.ApplicationSettings.Add("testmode", checkBoxTestMode.IsChecked);

                if (IsolatedStorageSettings.ApplicationSettings.Contains("testmode"))
                    IsolatedStorageSettings.ApplicationSettings["testmode"] = checkBoxTestMode.IsChecked;
                else
                    IsolatedStorageSettings.ApplicationSettings.Add("testmode", checkBoxTestMode.IsChecked);

                if (IsolatedStorageSettings.ApplicationSettings.Contains("server"))
                    IsolatedStorageSettings.ApplicationSettings["server"] = textBoxServer.Text;
                else
                    IsolatedStorageSettings.ApplicationSettings.Add("server", textBoxServer.Text);

                int value;
                if (Int32.TryParse(textBoxPort.Text, out value))
                {
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("port"))
                        IsolatedStorageSettings.ApplicationSettings["port"] = value;
                    else
                        IsolatedStorageSettings.ApplicationSettings.Add("port", value);
                }

                if (IsolatedStorageSettings.ApplicationSettings.Contains("password"))
                {
                    if ((bool)checkBoxSavePassword.IsChecked)
                        IsolatedStorageSettings.ApplicationSettings["password"] = textBoxPort.Text;
                    else
                        IsolatedStorageSettings.ApplicationSettings.Remove("password");
                }
                else if ((bool)checkBoxSavePassword.IsChecked)
                    IsolatedStorageSettings.ApplicationSettings.Add("password", textBoxPort.Text);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            base.OnNavigatingFrom(e);
        }


        // If save password checkbox checked
        private void checkBoxSavePassword_Checked(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }


        // If save password checkbox unchecked
        private void checkBoxSavePassword_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }


        // If auto connect checkbox checked
        private void checkBoxAutoConnect_Checked(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }


        // If auto connect checkbox unchecked
        private void checkBoxAutoConnect_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }


        // Page loaded
        private void PhoneApplicationPage_Loaded_1(object sender, RoutedEventArgs e)
        {
            this.loaded = true;
        }


        // Server text changed
        private void textBoxServer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }


        // Port text changed
        private void textBoxPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }


        // Username text changed
        private void textBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }


        // Password text changed
        private void textBoxPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }


        // Checkbox for test mode checked
        private void checkBoxTestMode_Checked(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }


        // Checkbox for test mode unchecked
        private void checkBoxTestMode_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            this.valuesChanged = true;
        }

        private void buttonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            this.connection.Disconnect();
            buttonDisconnect.IsEnabled = this.connection.IsConnected;
        }
    }
}