using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;
using VirtualizationListViewControl.SlaveTypes;

namespace VirtualizationListViewControl.Controls
{
    /// <summary>
    /// ContentControl substituting their ContentTemplate dependence on value of expression argument
    /// </summary>
    internal class ValueViewContentControl : ContentControl
    {
        public ValueViewContentControl()
        {
            ContentTemplate = new DataTemplate();
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
            var title = new FrameworkElementFactory(typeof(TextBlock));
            title.SetBinding(TextBlock.TextProperty, new Binding());
            title.SetValue(TextBlock.FontSizeProperty, (double)12);
            title.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);
            stackPanelFactory.AppendChild(title);
            ContentTemplate.VisualTree = stackPanelFactory;
        }
        
        #region DependencyProperties

        #region ConvertingContent

        public object ConvertingContent
        {
            get { return GetValue(ConvertingContentProperty); }
            set { SetValue(ConvertingContentProperty, value); }
        }

        public static readonly DependencyProperty ConvertingContentProperty 
            = DependencyProperty.Register("ConvertingContent", 
                                          typeof(object), 
                                          typeof(ValueViewContentControl),
                                          new FrameworkPropertyMetadata(OnConvertingContentChanged) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private object _convertingContent;

        private static void OnConvertingContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValueViewContentControl)d).SetConvertingContent(e.NewValue);
        }

        private void SetConvertingContent(object convertingContent)
        {
            if (convertingContent != null)
                _convertingContent = convertingContent;

            if (AvailableFilterableProperties == null
                || SelectedComparisonFieldLeaf == null)
                return;

            var filterableProp =
                AvailableFilterableProperties.FirstOrDefault(
                    fp => fp.BoundProperty.Equals(SelectedComparisonFieldLeaf.PropertyDescription));

            if (filterableProp != null
                && filterableProp.ValueToStringConverter != null
                && _convertingContent != null)
            {
                Content = filterableProp.ValueToStringConverter.Convert(_convertingContent,
                                                                        _convertingContent.GetType(),
                                                                        null,
                                                                        CultureInfo.InvariantCulture);
            }
            else
            {
                if (_convertingContent is string)
                    Content = "\"" + _convertingContent + "\"";
                else if (_convertingContent is DateTime)
                    Content = "\"" + ((DateTime)_convertingContent).ToString(CultureInfo.InvariantCulture) + "\"";
                else if (_convertingContent == null)
                    Content = "(пустое)";   //TODO: Localize
                else
                    Content = _convertingContent.ToString();
            }
        }
        #endregion

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
                                          typeof(ValueViewContentControl),
                                          new FrameworkPropertyMetadata(OnAvailableFilterablePropertiesChanged) 
                                            { BindsTwoWayByDefault = false, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnAvailableFilterablePropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValueViewContentControl)d).SetConvertingContent(null);
        }

        #endregion

        #region SelectedComparisonFieldLeaf
        /// <summary>
        /// Dependency property for selected comparison argument at ExpressionValueLeafContentControl
        /// </summary>
        public ExpressionTreeFieldLeaf SelectedComparisonFieldLeaf
        {
            get { return (ExpressionTreeFieldLeaf)GetValue(SelectedComparisonFieldLeafProperty); }
            set { SetValue(SelectedComparisonFieldLeafProperty, value); }
        }

        public static readonly DependencyProperty SelectedComparisonFieldLeafProperty 
            = DependencyProperty.Register("SelectedComparisonFieldLeaf", 
                                          typeof(ExpressionTreeFieldLeaf), 
                                          typeof(ValueViewContentControl),
                                          new FrameworkPropertyMetadata(OnSelectedComparisonOperatorChanged) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnSelectedComparisonOperatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValueViewContentControl)d).SetConvertingContent(null);
        }
        #endregion

        #endregion
    }
}
