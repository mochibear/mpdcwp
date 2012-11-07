/*
 * ImageDownloader
 * (c) Matti Ahinko 2012
 * matti.m.ahinko@student.jyu.fi
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace ImageDownloaderLibrary
{
    /// <summary>
    /// ImageDownloader is a control which downloads and shows an image.
    /// You may use integrated parser, set image url manually or set image from BitmapImage.
    /// </summary>
    public partial class ImageDownloader : UserControl
    {
        private WebClient webClientGetImage, webClientPageToParse;
        private BitmapImage imageSource;
        private bool autoDownload = true, autoLoad = true, autoParse = true;
        private string imageUrl, keyWord, imagePageUrl;


        /// <summary>
        /// Keyword for image search.
        /// Finds rows with this keyword in it and parses img in src.
        /// </summary>
        [Browsable(true),
        DescriptionAttribute("Keyword for image search")]
        public string KeyWord
        {
            get { return keyWord; }
            set { keyWord = value; }
        }
        
        
        /// <summary>
        /// Image which is shown
        /// </summary>
        public BitmapImage ImageSource
        {
            get { return imageSource; }
            set { imageSource = value; }
        }
        
        
        /// <summary>
        /// If image will be downloaded automatically
        /// </summary>
        [Browsable(true)]
        public bool AutoDownload
        {
            get { return autoDownload; }
            set { autoDownload = value; }
        }
        

        /// <summary>
        /// If image will be loaded automatically
        /// </summary>
        [Browsable(true)]
        public bool AutoLoad
        {
            get { return autoLoad; }
            set { autoLoad = value; }
        }

        
        /// <summary>
        /// If page containing the image will be parsed automatically
        /// </summary>
        public bool AutoParse
        {
            get { return autoParse; }
            set { autoParse = value; }
        }
        
        
        /// <summary>
        /// Url of the image which will be downloaded and shown
        /// </summary>
        [Browsable(true)]
        public string ImageUrl
        {
            get { return imageUrl; }
            set { ChangeImageUrl(value); }
        }

        
        /// <summary>
        /// Url of the page containing the image
        /// </summary>
        public string ImagePageUrl
        {
            get { return imagePageUrl; }
            set { ChangeImagePageUrl(value); }
        }
        
        
        /// <summary>
        /// Default image, if no image is available
        /// </summary>
        [Browsable(true)]
        public ImageSource DefaultImage { get { return this.image.Source; } set { this.image.Source = value; } }


        /// <summary>
        /// Event if the image is opened
        /// </summary>
        [Browsable(true)]
        public event EventHandler<RoutedEventArgs> ImageOpened { add { this.image.ImageOpened += value; } remove { this.image.ImageOpened -= value; } }


        /// <summary>
        /// Event if opening of the image failed
        /// </summary>
        [Browsable(true)]
        public event EventHandler<ExceptionRoutedEventArgs> ImageFailed { add { this.image.ImageFailed += value; } remove { this.image.ImageFailed -= value; } }


        /// <summary>
        /// Event if download of the image succeeded
        /// </summary>
        [Browsable(true)]
        public event EventHandler<RoutedEventArgs> ImageDownloadSucceeded;


        /// <summary>
        /// Event if url of the image is found
        /// </summary>
        [Browsable(true)]
        public event EventHandler ImageUrlFound;


        /// <summary>
        /// Event if url of the image is not found
        /// </summary>
        [Browsable(true)]
        public event EventHandler ImageUrlNotFound;


        /// <summary>
        /// Constructor of ImageDownloader
        /// </summary>
        public ImageDownloader()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Change url of the image. If AutoDownload is true, image will be downloaded automatically
        /// </summary>
        /// <param name="url">Url of the image</param>
        public void ChangeImageUrl(string url)
        {
            this.imageUrl = url;
            if (autoDownload)
            {
                GetImage(url);
            }
        }


        /// <summary>
        /// Changes url of the image page
        /// </summary>
        /// <param name="url"></param>
        public void ChangeImagePageUrl(string url)
        {
            this.imagePageUrl = url;
            if (autoParse)
                GetImageUrl(url);
        }


        /// <summary>
        /// Parses url of the image
        /// </summary>
        /// <param name="urlToParse">Url to parse</param>
        /// <exception cref="NullReferenceException">Keyword is null</exception>
        public void GetImageUrl(string urlToParse)
        {
            if (keyWord == null)
                throw new NullReferenceException("Keyword is null");
            if (webClientPageToParse != null)
                webClientPageToParse.CancelAsync();
            webClientPageToParse = new WebClient();
            webClientPageToParse.OpenReadAsync(new Uri(urlToParse));
            webClientPageToParse.OpenReadCompleted += new OpenReadCompletedEventHandler(pageToParse_OpenReadCompleted);
        }

        
        private void pageToParse_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            bool found = false;
            if (e.Error == null && !e.Cancelled)
            {
                var reader = new System.IO.StreamReader(e.Result);
                string line;
                while (!reader.EndOfStream && !found)
                {
                    line = reader.ReadLine();
                    if (line.Contains(this.keyWord) && line.ToLower().Contains("src="))
                    {
                        string tmp = line.Substring(line.IndexOf("src=") + 5);
                        this.imageUrl = tmp.Substring(0, tmp.IndexOf("\""));
                        found = true;
                    }
                }
            }
            if (found)
            {
                if (ImageUrlFound != null)
                    ImageUrlFound(this, new EventArgs());
                if (this.AutoDownload)
                    this.GetImage(this.imageUrl);
            }
            if (!found && ImageUrlNotFound != null)
                ImageUrlNotFound(this, new EventArgs());
        }

        /// <summary>
        /// Downloads and changes image of the control
        /// </summary>
        /// <exception cref="BadImageFormatException">If image is in bad format, exception will be thrown</exception>
        /// <param name="imageUrl">Url of the image</param>
        public void GetImage(string imageUrl)
        {
            if (webClientGetImage != null)
                webClientGetImage.CancelAsync();
            webClientGetImage = new WebClient();
            webClientGetImage.OpenReadAsync(new Uri(imageUrl));
            webClientGetImage.OpenReadCompleted += new OpenReadCompletedEventHandler(downloadImage_OpenReadCompleted);            
        }


        private void downloadImage_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                try
                {
                    imageSource = new BitmapImage();
                    imageSource.SetSource(e.Result);
                    if (ImageDownloadSucceeded != null)
                        ImageDownloadSucceeded(this, new RoutedEventArgs());
                    if (autoLoad)
                        image.Source = imageSource;
                }
                catch (BadImageFormatException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("Unspecified error"))
                        throw ex;
                    else
                        throw new BadImageFormatException("Downloaded image is in bad format. Check url.");
                }
            }
        }
    }
}
