using System;
using System.Globalization;
using System.Windows.Data;

namespace SampleWpfApplication.Views.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        private DateTime _lastDate;
        private TimeSpan _lastTime;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _lastDate = DateTime.Now;
            if (value != null)
            {
                _lastDate = value is DateTime ? (DateTime)value : new DateTime();
                _lastTime = _lastDate.TimeOfDay;
            }
            _lastDate = new DateTime(_lastDate.Year, _lastDate.Month, _lastDate.Day, _lastTime.Hours, _lastTime.Minutes, 0);
            if ((string)parameter == "date")
            {
                return _lastDate.Date;
            }
            if ((string)parameter == "time")
            {
                return _lastDate.TimeOfDay.ToString(@"hh\:mm");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            if ((string)parameter == "date")
            {
                DateTime cDateTime = _lastDate;
                DateTime.TryParse(value.ToString(), out cDateTime);
                _lastDate = cDateTime;
            }
            if ((string)parameter == "time")
            {
                int hours = Int32.Parse(value.ToString().Split(':')[0]);
                int minutes = Int32.Parse(value.ToString().Split(':')[1]);
                _lastTime = new TimeSpan(hours, minutes, 0);
            }

            DateTime dateTime = new DateTime(_lastDate.Year, _lastDate.Month, _lastDate.Day, _lastTime.Hours, _lastTime.Minutes, 0);
            return dateTime;
        }
    }
}
