using System;
using System.Globalization;
using System.Windows.Data;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;

namespace VirtualizationListViewControl.Converters
{
    internal class ComparisonOperatorToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ComparisonOperators))
                return String.Empty;

            return ComparisonOperatorNode.OperatorToString((ComparisonOperators)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
