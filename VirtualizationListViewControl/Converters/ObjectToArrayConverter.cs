using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace VirtualizationListViewControl.Converters
{
    internal class ObjectToArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new List<object>();
            return new List<object> {value};
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
