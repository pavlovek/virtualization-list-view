using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;
using VirtualizationListViewControl.SlaveTypes;

namespace VirtualizationListViewControl.Controls
{
    /// <summary>
    /// ContentControl substituting their ContentTemplate dependence on selected expression argument
    /// </summary>
    internal class ExpressionValueLeafContentControl : ContentControl
    {
        #region DependencyProperties

        #region AvailableFilterableProperties
        /// <summary>
        /// Dependency property for available filterable properties at ExpressionValueLeafContentControl
        /// </summary>
        public FilterablePropertyDescriptionCollection AvailableFilterableProperties
        {
            get { return (FilterablePropertyDescriptionCollection)GetValue(AvailableFilterablePropertiesProperty); }
            set { SetValue(AvailableFilterablePropertiesProperty, value); }
        }

        public static readonly DependencyProperty AvailableFilterablePropertiesProperty 
            = DependencyProperty.Register("AvailableFilterableProperties", 
                                          typeof(FilterablePropertyDescriptionCollection), 
                                          typeof(ExpressionValueLeafContentControl),
                                          new FrameworkPropertyMetadata(OnAvailableFilterablePropertiesChanged) 
                                            { BindsTwoWayByDefault = false, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnAvailableFilterablePropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExpressionValueLeafContentControl)d).SetSelectedComparisonFieldLeaf(_selectedComparisonFieldLeaf);
        }

        #endregion

        #region SelectedComparisonFieldLeaf
        /// <summary>
        /// Dependency property for selected conparison argument at ExpressionValueLeafContentControl
        /// </summary>
        public ExpressionTreeFieldLeaf SelectedComparisonFieldLeaf
        {
            get { return (ExpressionTreeFieldLeaf)GetValue(SelectedComparisonFieldLeafProperty); }
            set { SetValue(SelectedComparisonFieldLeafProperty, value); }
        }
        public static readonly DependencyProperty SelectedComparisonFieldLeafProperty 
            = DependencyProperty.Register("SelectedComparisonFieldLeaf", 
                                          typeof(ExpressionTreeFieldLeaf), 
                                          typeof(ExpressionValueLeafContentControl),
                                          new FrameworkPropertyMetadata(OnSelectedComparisonOperatorChanged) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static ExpressionTreeFieldLeaf _selectedComparisonFieldLeaf;

        private static void OnSelectedComparisonOperatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExpressionValueLeafContentControl)d).SetSelectedComparisonFieldLeaf((ExpressionTreeFieldLeaf)e.NewValue);
        }

        private void SetSelectedComparisonFieldLeaf(ExpressionTreeFieldLeaf selectedFiledLeaf)
        {
            if (selectedFiledLeaf != null)
                _selectedComparisonFieldLeaf = selectedFiledLeaf;
            
            if (AvailableFilterableProperties == null
                || selectedFiledLeaf == null)
                return;

            var filterableProp =
                AvailableFilterableProperties.FirstOrDefault(
                    fp => fp.BoundProperty.Equals(selectedFiledLeaf.PropertyDescription));

            if (filterableProp != null)
                ContentTemplate = filterableProp.ValueTemplate;
        }

        #endregion

        #endregion
    }
}
