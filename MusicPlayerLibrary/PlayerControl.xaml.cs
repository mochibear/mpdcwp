﻿/*
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
using System.ComponentModel;

namespace MusicPlayerLibrary
{
    /// <summary>
    /// Musicplayer Controller Control for Windows Phone 7
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        // If playing
        private bool hold = false;


        /// <summary>
        /// Is control in Playing mode
        /// </summary>
        public bool Playing { get; private set; }


        /// <summary>
        /// Event if play is pressed
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Event of play button")]
        public event EventHandler<CancelRoutedEventArgs> Play;


        /// <summary>
        /// Event if pause is pressed
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Event of play button")]
        public event EventHandler Pause;


        /// <summary>
        /// Event if stop is pressed
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Event of stop button")]
        public event EventHandler Stop;


        /// <summary>
        /// Event if previous is pressed
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Event of previous button")]
        public event EventHandler Previous;


        /// <summary>
        /// Event if next is pressed
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Event of next button")]
        public event EventHandler Next;


        /// <summary>
        /// Event if rewind is pressed
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Event of rewind button")]
        public event EventHandler Rewind;


        /// <summary>
        /// Event if forward is pressed
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Event of forward button")]
        public event EventHandler Forward;


        /// <summary>
        /// Event if volume is changed
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Event of volume being changed")]
        public event RoutedPropertyChangedEventHandler<double> VolumeChanged;


        /// <summary>
        /// Background of play button
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Background of play button")]
        public Brush PlayBackground { set { this.buttonPlay.Background = value; } get { return this.buttonPlay.Background; } }


        /// <summary>
        /// Background of pause button
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Background of pause button")]
        public Brush PauseBackground { set { this.buttonPause.Background = value; } get { return this.buttonPause.Background; } }


        /// <summary>
        /// Background of stop button
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Background of stop button")]
        public Brush StopBackground { set { this.buttonStop.Background = value; } get { return this.buttonStop.Background; } }


        /// <summary>
        /// Background of previous button
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Background of previous button")]
        public Brush PreviousBackground { set { this.buttonPrevious.Background = value; } get { return this.buttonPrevious.Background; } }


        /// <summary>
        /// Background of next button
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Background of next button")]
        public Brush NextBackground { set { this.buttonNext.Background = value; } get { return this.buttonNext.Background; } }


        /// <summary>
        /// Is playback paused
        /// </summary>
        public bool Paused { get; private set; }

        
        /// <summary>
        /// Constructor
        /// </summary>
        public PlayerControl()
        {
            InitializeComponent();
            buttonPause.Visibility = System.Windows.Visibility.Collapsed;
        }


        // Button play clicked
        // Hide play and show pause
        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (Play != null)
            {
                CancelRoutedEventArgs args = new CancelRoutedEventArgs("Toggle play");
                Play(this, args);
                if (args.Cancel)
                    return;
            }
            Playing = true;
            this.Paused = false;
            buttonPlay.Visibility = System.Windows.Visibility.Collapsed;
            buttonPause.Visibility = System.Windows.Visibility.Visible;
        }


        // Button pause clicked
        // Hide pause and show play
        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            if (Pause != null)
                Pause(this, new EventArgs());
            buttonPlay.Visibility = System.Windows.Visibility.Visible;
            buttonPause.Visibility = System.Windows.Visibility.Collapsed;
            this.Paused = true;
        }


        // Button stop clicked
        // Hide pause and show play
        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            if (Stop != null)
                Stop(this, new EventArgs());
            Playing = false;
            buttonPlay.Visibility = System.Windows.Visibility.Visible;
            buttonPause.Visibility = System.Windows.Visibility.Collapsed;
        }


        // Button next clicked
        // If button is being hold, do nothing
        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (hold)
            {
                hold = false;
                return;
            }
            if (Next != null)
                Next(this, new EventArgs());
        }


        // Button previous clicked
        // If button is being hold, do nothing
        private void buttonPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (hold)
            {
                hold = false;
                return;
            }
            if (Previous != null)
                Previous(this, new EventArgs());
        }


        // Button next is being pressed long time
        // Perform forward, if playing a song
        private void buttonNext_Hold(object sender, GestureEventArgs e)
        {
            hold = true;
            if (Playing && Forward != null)
                Forward(this, new EventArgs());
        }


        // Button previous is being pressed long time
        // Perform rewind, if playing
        private void buttonPrevious_Hold(object sender, GestureEventArgs e)
        {
            hold = true;
            if (Playing && Rewind != null)
                Rewind(this, new EventArgs());
        }


        // Volume changed
        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeChanged != null)
                VolumeChanged(sender, e);
        }
    }
}
