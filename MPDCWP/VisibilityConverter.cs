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
using System.Globalization;
using System.Windows.Data;

namespace MPDCWP
{
    /// <summary>
    /// Class for converting int to Visibility
    /// Inherits IValueConverter
    /// </summary>
    public class VisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts value to visibility
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="culture">Culture</param>
        /// <returns>Visibility</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int number = (int)value;
            if (number == 0)
                return Visibility.Collapsed;
            else return Visibility.Visible;
        }

        /// <summary>
        /// Converts visibility to integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="culture">Culture</param>
        /// <returns>Integer</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
