using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Build.Framework.XamlTypes;

namespace PETBrowser
{
    /**
     * Converter that takes an integer and multiple boolean values; returns true if integer is
     * non-zero and all boolean values are true
     */
    public class CountAndBoolMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("Target type must be a bool");
            }

            if ((int)values[0] != 0)
            {
                for (int i = 1; i < values.Length; i++)
                {
                    if ((bool) values[i] == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}