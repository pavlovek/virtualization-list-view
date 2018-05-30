using System;
using System.Globalization;
using System.Windows.Data;

namespace VirtualizationListViewControl.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterValue = parameter as string;
            if (parameterValue == "inverse")
            {
                if (value is string)
                {
                    if (String.IsNullOrWhiteSpace(value.ToString()))
                        return false;
                    return true;
                }
                
                if (value == null)
                    return false;
                return true;
            }

            if (value is string)
            {
                if (String.IsNullOrWhiteSpace(value.ToString()))
                    return true;
                return false;
            }

            if (value == null)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
