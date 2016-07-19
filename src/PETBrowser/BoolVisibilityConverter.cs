using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PETBrowser
{
    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool) value;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (Visibility) value;

            switch (visibilityValue)
            {
                case Visibility.Visible:
                    return true;
                case Visibility.Hidden:
                case Visibility.Collapsed:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class NotBoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;

            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (Visibility)value;

            switch (visibilityValue)
            {
                case Visibility.Visible:
                    return false;
                case Visibility.Hidden:
                case Visibility.Collapsed:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}