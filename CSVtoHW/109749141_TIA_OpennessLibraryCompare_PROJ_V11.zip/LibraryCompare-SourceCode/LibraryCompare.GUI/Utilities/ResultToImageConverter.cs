using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LibraryCompare.Core.Enums;

namespace LibraryCompare.GUI.Utilities
{
    public class ResultToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                switch ((CompareResult) value)
                {
                    case CompareResult.None:
                        return Application.Current.FindResource("CompareNone");
                    case CompareResult.Equal:
                        return Application.Current.FindResource("CompareEqual");
                    case CompareResult.Left:
                        return Application.Current.FindResource("CompareLeft");
                    case CompareResult.Right:
                        return Application.Current.FindResource("CompareRight");
                    case CompareResult.Diff:
                        return Application.Current.FindResource("CompareDiff");
                    default:
                        return Application.Current.FindResource("CompareNone");
                }
            return Application.Current.FindResource("CompareNone");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}