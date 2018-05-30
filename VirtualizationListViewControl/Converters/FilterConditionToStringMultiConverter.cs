using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;
using VirtualizationListViewControl.Helpers;
using VirtualizationListViewControl.Localization;
using VirtualizationListViewControl.SlaveTypes;

namespace VirtualizationListViewControl.Converters
{
    public class FilterConditionToStringMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null
                || values.Length != 2)
                return null;

            var fieldLeaf = values[0] as ExpressionTree;
            if (fieldLeaf == null)
                return null;

            var availableFilterableProperties = values[1] as FilterablePropertyDescriptionCollection;
            if (availableFilterableProperties == null)
                return null;

            return GetStringFilteringConditions(fieldLeaf.Root, availableFilterableProperties);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Получить строковое выражение фильтрации
        /// </summary>
        /// <param name="filterElement">Параметры фильтра</param>
        /// <param name="filterablePropertyDescription">Описание свойств фильтрации</param>
        /// <returns>Строковое выражение фильтрации</returns>
        private string GetStringFilteringConditions(ExpressionTreeElement filterElement, FilterablePropertyDescriptionCollection filterablePropertyDescription)
        {
            var fieldLeaf = filterElement as ExpressionTreeFieldLeaf;
            if (fieldLeaf != null)
            {
                var foundFieldDescr =
                    filterablePropertyDescription.FirstOrDefault(
                        prop => prop.BoundProperty.Equals(fieldLeaf.PropertyDescription));
                if (foundFieldDescr != null)
                    return foundFieldDescr.Title;
            }

            var valueLeaf = filterElement as ExpressionTreeValueLeaf;
            if (valueLeaf != null)
            {
                var fieldForValue = filterElement.Parent.Left as ExpressionTreeFieldLeaf;
                if (fieldForValue != null)
                {
                    var foundFieldDescr =
                        filterablePropertyDescription.FirstOrDefault(
                            prop => prop.BoundProperty.Equals(fieldForValue.PropertyDescription));
                    if (foundFieldDescr != null
                        && foundFieldDescr.ValueToStringConverter != null)
                        return foundFieldDescr.ValueToStringConverter.Convert(valueLeaf.FieldValue,
                            valueLeaf.FieldValue.GetType(), null, CultureInfo.CurrentCulture).ToString();
                }
                if (valueLeaf.FieldValue is string)
                    return "\"" + valueLeaf.FieldValue + "\"";
                if (valueLeaf.FieldValue is DateTime)
                    return "\"" + ((DateTime)valueLeaf.FieldValue).ToString(CultureInfo.CurrentCulture) + "\"";
                if (valueLeaf.FieldValue == null)
                    return LocalizationDictionary.Empty;
                else
                    return valueLeaf.FieldValue.ToString();
            }

            var comparisonNode = filterElement as ComparisonOperatorNode;
            if (comparisonNode != null)
            {
                if (comparisonNode.Right is ExpressionTreeValueLeaf)
                {
                    if (((ExpressionTreeValueLeaf)comparisonNode.Right).FieldValue == null
                       || (((ExpressionTreeValueLeaf)comparisonNode.Right).FieldValue is String
                       && String.IsNullOrEmpty(((ExpressionTreeValueLeaf)comparisonNode.Right).FieldValue as String)))
                        return GetStringFilteringConditions(comparisonNode.Left, filterablePropertyDescription) 
                               + " " + ComparisonOperatorNode.OperatorToString(comparisonNode.Operator) + " "
                               + LocalizationDictionary.Empty;
                    return GetStringFilteringConditions(comparisonNode.Left, filterablePropertyDescription) 
                           + " " + ComparisonOperatorNode.OperatorToString(comparisonNode.Operator) + " "
                           + GetStringFilteringConditions(comparisonNode.Right, filterablePropertyDescription);
                }
            }

            var booleanNode = filterElement as BooleanOperatorNode;
            if (booleanNode != null)
            {
                var boolOperatorItemsSource = new LocalizableEnumItemsSource {Type = typeof (BooleanOperators)};
                if (booleanNode.Operator == BooleanOperators.Not)
                    return boolOperatorItemsSource.Convert(booleanNode.Operator, null, null, null) + " "
                        + (booleanNode.Left is BooleanOperatorNode ? "( " : String.Empty) 
                        + GetStringFilteringConditions(booleanNode.Left, filterablePropertyDescription) 
                        + (booleanNode.Left is BooleanOperatorNode ? " )" : String.Empty);
                return GetStringFilteringConditions(booleanNode.Left, filterablePropertyDescription)
                       + " " + boolOperatorItemsSource.Convert(booleanNode.Operator, null, null, null) + " "
                       + GetStringFilteringConditions(booleanNode.Right, filterablePropertyDescription);
            }

            return String.Empty;
        }
    }
}
