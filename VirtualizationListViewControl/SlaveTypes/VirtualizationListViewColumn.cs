using System.Windows;
using VirtualizationListView.SortAndFilterDTO;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;

namespace VirtualizationListViewControl.SlaveTypes
{
    /// <summary>
    /// Representation column at VirtualizationListView
    /// </summary>
    public class VirtualizationListViewColumn
    {
        /// <summary>
        /// Column's header title
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Column's header ToolTip
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        /// Column's width
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// DataTemplate for column's cell at VirtualizationListView
        /// </summary>
        public DataTemplate CellTemplate { get; set; }

        /// <summary>
        /// Indicate when sorting is disable
        /// </summary>
        public bool DisabledSorting { get; set; }

        /// <summary>
        /// Column's cell element property description
        /// </summary>
        public FieldDescription BoundProperty { get; set; }

        /// <summary>
        /// DataTemplate for fast filter cell
        /// </summary>
        public DataTemplate FilterValueTemplate { get; set; }

        /// <summary>
        /// Fast filter cell's property description
        /// </summary>
        public FieldDescription FilterBoundProperty { get; set; }

        /// <summary>
        /// Fast filter comparison operator
        /// </summary>
        public ComparisonOperators FilterOperator { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public VirtualizationListViewColumn()
        {
            Width = double.NaN;
            DisabledSorting = false;
            FilterOperator = ComparisonOperators.Equal;
        }
    }
}
