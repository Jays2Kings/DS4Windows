/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace DS4WinWPF.DS4Forms.Converters
{
    public class EscapeAccessKeysConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string temp = value.ToString();
            temp = Regex.Replace(temp, "_{1}", "__");
            return temp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string temp = value.ToString();
            temp = Regex.Replace(temp, "_{2}", "_");
            return temp;
        }
    }
}
