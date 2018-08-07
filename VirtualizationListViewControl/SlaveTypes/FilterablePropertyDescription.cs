using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using VirtualizationListView.SortAndFilterDTO;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;

namespace VirtualizationListViewControl.SlaveTypes
{
    /// <summary>
    /// Property description for filtering
    /// </summary>
    public class FilterablePropertyDescription
    {
        /// <summary>
        /// Property representation title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Filterable property description
        /// </summary>
        public FieldDescription BoundProperty { get; set; }

        /// <summary>
        /// Available comparision operators
        /// </summary>
        public ObservableCollection<ComparisonOperators> AvailableOperators { get; set; }

        /// <summary>
        /// DataTemplate for generation property value
        /// </summary>
        public DataTemplate ValueTemplate { get; set; }

        /// <summary>
        /// Default property value
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Indicate when property value is bound available values
        /// </summary>
        public bool HasAvailableValues { get; set; }

        /// <summary>
        /// ValueConverter for property value representation
        /// </summary>
        public IValueConverter ValueToStringConverter { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public FilterablePropertyDescription()
        {
            AvailableOperators = new ObservableCollection<ComparisonOperators>();
            HasAvailableValues = false;
        }
    }
}
