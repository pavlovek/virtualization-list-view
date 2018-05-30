using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using VirtualizationListView.SortAndFilterDTO.Filtering;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;
using VirtualizationListViewControl.Localization;

namespace VirtualizationListViewControl.Converters
{
    /// <summary>
    /// Convert validation error content to specify ValidationResponce
    /// </summary>
    public class ValidationErrorToResponseConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var val = values[0] as ExpressionTreeValueLeaf;
            if (val == null)
                return null;
            if (values[1] == DependencyProperty.UnsetValue)
            {
                val.ErrorResponce = new ValidationResponce();
            }
            else
            {
                var errStr = values[1] as string;
                var strBuilder = new StringBuilder(LocalizationDictionary.ValidationErrorText);
                strBuilder.Append(" ");
                strBuilder.Append(parameter);
                strBuilder.Append(": ");
                strBuilder.Append(errStr);
                val.ErrorResponce = new ValidationResponce(strBuilder.ToString());
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
