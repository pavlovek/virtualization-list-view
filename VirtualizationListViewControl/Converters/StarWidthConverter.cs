using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace VirtualizationListViewControl.Converters
{
    public class StarWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var listView = value as ListView;
            if (listView == null)
                return Double.NaN;
            var gv = listView.View as GridView;
            if (gv == null)
                return Double.NaN;

            int columnNumber;
            if (parameter != null
                && Int32.TryParse(parameter.ToString(), out columnNumber)
                && !Double.IsNaN(gv.Columns[columnNumber].Width))
                return gv.Columns[columnNumber].Width - 27;

            double width = listView.ActualWidth;

            for (int i = 0; i < gv.Columns.Count; i++)
                if (!Double.IsNaN(gv.Columns[i].Width))
                    width -= gv.Columns[i].Width;

            return width - 27; //this is to take care of margin/padding
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
