using System;
using System.ComponentModel;
using System.Windows;
using VirtualizationListView.SortAndFilterDTO;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;

namespace VirtualizationListViewControl.SlaveTypes
{
    /// <summary>
    /// Container for filterable cell at fast filter row
    /// </summary>
    internal class FilterRowContainer : INotifyPropertyChanged
    {
        /// <summary>
        /// Filterable property description
        /// </summary>
        public FieldDescription BoundProperty { get; set; }
        
        /// <summary>
        /// DataTemplate for generation property value
        /// </summary>
        public DataTemplate ValueTemplate { get; set; }

        /// <summary>
        /// Comparision filtration operator
        /// </summary>
        public ComparisonOperators FilterOperator { get; set; }

        /// <summary>
        /// Property value
        /// </summary>
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");

                if (FilterByValue != null
                    && !_isClearing)
                    FilterByValue(BoundProperty, _value, FilterOperator);
                else
                    _isClearing = false;
            }
        }
        private object _value;
        private bool _isClearing;


        /// <summary>
        /// Default constructor
        /// </summary>
        public FilterRowContainer()
            : this(new FieldDescription(), null, null, ComparisonOperators.Equal)
        { }

        /// <summary>
        /// Constructor for all parameters
        /// </summary>
        /// <param name="boundProperty">Filterable property description</param>
        /// <param name="valueTemplate">DataTemplate for generation property value</param>
        /// <param name="value">Property value</param>
        /// <param name="filterOperator">Comparision filtration operator</param>
        public FilterRowContainer(FieldDescription boundProperty, DataTemplate valueTemplate, object value, ComparisonOperators filterOperator)
        {
            BoundProperty = boundProperty;
            ValueTemplate = valueTemplate;
            _value = value;
            FilterOperator = filterOperator;

            _isClearing = false;
        }

        
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clear property value
        /// </summary>
        /// <param name="notClearProperty">Property which not clear fast filter row</param>
        public void ClearValue(FieldDescription? notClearProperty)
        {
            if (!notClearProperty.HasValue
                || !BoundProperty.Equals(notClearProperty))
            {
                _isClearing = true;
                Value = null;
            }
        }

        /// <summary>
        /// Filter by value delegate
        /// </summary>
        public Action<FieldDescription, object, ComparisonOperators> FilterByValue;

        #endregion
    }
}
