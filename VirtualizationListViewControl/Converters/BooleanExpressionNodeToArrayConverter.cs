using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;

namespace VirtualizationListViewControl.Converters
{
    internal class BooleanExpressionNodeToArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = new List<object>();
            
            var boolNode = value as BooleanOperatorNode;
            if (boolNode == null)
                return result;

            result = GetChildJfNode(boolNode);

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private List<object> GetChildJfNode(BooleanOperatorNode boolNode)
        {
            var result = new List<object>();

            if (boolNode.Left is BooleanOperatorNode
                && boolNode.Operator == ((BooleanOperatorNode)boolNode.Left).Operator)
                result.AddRange(GetChildJfNode(boolNode.Left as BooleanOperatorNode));
            else if (boolNode.Left != null)
                result.Add(boolNode.Left);

            if (boolNode.Right is BooleanOperatorNode
                && boolNode.Operator == ((BooleanOperatorNode)boolNode.Right).Operator)
                result.AddRange(GetChildJfNode(boolNode.Right as BooleanOperatorNode));
            else if (boolNode.Right != null)
                result.Add(boolNode.Right);

            return result;
        }
    }
}
