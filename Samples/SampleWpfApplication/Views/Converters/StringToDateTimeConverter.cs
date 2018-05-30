using System;
using System.Globalization;
using System.Windows.Data;

namespace SampleWpfApplication.Views.Converters
{
    public class StringToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
                return String.Empty;
            
            return ((DateTime)value).ToString(culture.DateTimeFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            DateTime date;
            if (!DateTime.TryParse(value.ToString(), out date))
                return null;

            return date;
        }
    }
}
