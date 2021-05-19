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
