using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace LibraryCompare.GUI.Utilities
{
    internal class LanguageCommentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return "";

            var dict = values[0] as Dictionary<string, string>;
            var key = values[1] as string;

            if (dict == null || key == null) return "";

            string ret;
            return dict.TryGetValue(key, out ret) ? ret : "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
