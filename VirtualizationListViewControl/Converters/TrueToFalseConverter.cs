using System;
using System.Globalization;
using System.Windows.Data;

namespace VirtualizationListViewControl.Converters
{
    public class TrueToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
                flag = (bool)value;
            else
                throw new ArgumentException("value must be Boolean", "value");

            return !flag;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
