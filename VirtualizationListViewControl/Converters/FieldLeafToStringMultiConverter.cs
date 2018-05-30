using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;
using VirtualizationListViewControl.SlaveTypes;

namespace VirtualizationListViewControl.Converters
{
    internal class FieldLeafToStringMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null
                || values.Length != 2)
                return null;

            var fieldLeaf = values[0] as ExpressionTreeFieldLeaf;
            if (fieldLeaf == null)
                return null;
            
            var availableFilterableProperties = values[1] as FilterablePropertyDescriptionCollection;
            if (availableFilterableProperties == null)
                return null;

            var foundFieldDescr =
                availableFilterableProperties.FirstOrDefault(
                    prop => prop.BoundProperty.Equals(fieldLeaf.PropertyDescription));
            if (foundFieldDescr == null)
                return null;

            return foundFieldDescr.Title;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
