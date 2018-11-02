using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using VirtualizationListView.SortAndFilterDTO;
using VirtualizationListView.SortAndFilterDTO.Filtering;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;
using VirtualizationListView.SortAndFilterDTO.Sorting;
using VirtualizationListViewControl.Converters;
using VirtualizationListViewControl.Helpers;
using VirtualizationListViewControl.Interfaces;
using VirtualizationListViewControl.Localization;
using VirtualizationListViewControl.SlaveTypes;

namespace VirtualizationListViewControl.Controls
{
    public class VirtualizationListView : Control
    {
        #region Fields

        private ListView _virtualizationListView;
        private TextBlock _countTextBlock;

        private readonly Style _sortingColumnHeaderStyle;

        private Window _filterConfiguratorDialog;

        #endregion

        #region Properties

        /// <summary>
        /// Indicate when ListView initialized
        /// </summary>
        public bool IsListViewInitialized
        {
            get { return _virtualizationListView != null; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Selection item changed event
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null) 
                SelectionChanged.Invoke(sender, e);
        }

        #endregion

        #region Constructors & Initializes

        public VirtualizationListView()
        {
            DefaultStyleKey = typeof(VirtualizationListView);

            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("/VirtualizationListViewControl;component/Themes/VirtualizationListView.xaml", UriKind.RelativeOrAbsolute);

            _sortingColumnHeaderStyle = resourceDictionary["SortingColumnHeaderStyle"] as Style;
            _sortingColumnHeaderStyle.Setters.Add(new EventSetter(ButtonBase.ClickEvent, new RoutedEventHandler(SortByColumn)));
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call System.Windows.FrameworkElement.ApplyTemplate().
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _virtualizationListView = GetTemplateChild("VirtualizationListView") as ListView;
            if (_virtualizationListView != null)
            {
                _virtualizationListView.SelectionChanged += VirtualizationListView_OnSelectionChanged;
                _virtualizationListView.SelectionChanged += OnSelectionChanged;
                _virtualizationListView.KeyDown += VirtualizationListView_OnKeyDown;

                var listViewItemStyle = new Style(typeof(ListViewItem));
                listViewItemStyle.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                _virtualizationListView.ItemContainerStyle = listViewItemStyle;

                if (Columns != null)
                    SetColumns(Columns);
            }

            _countTextBlock = GetTemplateChild("CountTextBlock") as TextBlock;
        }

        #endregion

        #region DependencyProperties

        #region Localization
        /// <summary>
        /// Dependency property for anchor localization at VirtualizationListView
        /// </summary>
        public ResourceManager Localization
        {
            get { return (ResourceManager)GetValue(LocalizationProperty); }
            set { SetValue(LocalizationProperty, value); }
        }

        public static readonly DependencyProperty LocalizationProperty
            = DependencyProperty.Register("Localization",
                typeof(ResourceManager),
                typeof(VirtualizationListView),
                new FrameworkPropertyMetadata(OnLocalizationChanged));

        private static void OnLocalizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VirtualizationListView)d).SetLocalization(e.NewValue as ResourceManager);
        }

        private void SetLocalization(ResourceManager localization)
        {
            Localization = localization;

            LocalizationManager.SetLocalization(localization);
        }
        #endregion

        #region ItemsSource
        /// <summary>
        /// Dependency property for anchor items source to VirtualizationListView
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty 
            = DependencyProperty.Register("ItemsSource", 
                                          typeof(IEnumerable), 
                                          typeof(VirtualizationListView),
                                          new FrameworkPropertyMetadata(OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VirtualizationListView)d).SetItemsSource(e.NewValue as IEnumerable);
        }
        private void SetItemsSource(IEnumerable itemsSource)
        {
            if (_virtualizationListView == null)
                return;
            if (_virtualizationListView.ItemsSource is IDisposable)
                ((IDisposable)(_virtualizationListView.ItemsSource)).Dispose();
            _virtualizationListView.ItemsSource = itemsSource;
            if (_virtualizationListView.ItemsSource != null)
            {
                if (itemsSource is ISelectionManager)
                {
                    ((ISelectionManager) itemsSource).ClearSelection += ClearSelection;
                    ((ISelectionManager) itemsSource).ResetSelection += ResetSelection;
                }
                if (itemsSource is INotifyCollectionChanged)
                {
                    ((INotifyCollectionChanged)itemsSource).CollectionChanged += CollectionChanged;
                }
                if (itemsSource is IFilterableCollection)
                {
                    //Set filter
                    if (Filters == null)
                    {
                        Filters = new ObservableCollection<FilterParams>(((IFilterableCollection)itemsSource).AllFilters);
                    }
                    else if (!Filters.SequenceEqual(((IFilterableCollection)itemsSource).AllFilters))
                    {
                        Filters.Clear();
                        foreach (FilterParams filter in ((IFilterableCollection)itemsSource).AllFilters)
                            Filters.Add(filter);
                    }

                    SelectedFilter = ((IFilterableCollection)itemsSource).CurrentFilterParams;

                    ((IFilterableCollection)itemsSource).FilterChanged += OnFiltersChanged;
                }
            }

            if (SelectedFilter == null)
                IsDefaultFilter = true;

            //Set sorting
            SortParams currentSortParams = null;
            if (itemsSource is ISortableCollection)
                currentSortParams = ((ISortableCollection)itemsSource).CurrentSortParams;
            if (currentSortParams != null)
            {
                GridView gridView = _virtualizationListView.View as GridView;
                for (int i = 0; i < Columns.Count; i++)
                {
                    if (currentSortParams.PropertyDescr.Equals(Columns[i].BoundProperty))
                    {
                        if (gridView.Columns.Count > i
                            && gridView.Columns[i].Header is GridViewColumnHeader)
                            ((GridViewColumnHeader)gridView.Columns[i].Header).Tag = currentSortParams.IsAsc ? 0 : 1;
                    }
                }
            }
        }
        #endregion

        #region Columns
        /// <summary>
        /// Dependency property for anchor columns to VirtualizationListView
        /// </summary>
        public VirtualizationListViewColumnCollection Columns
        {
            get { return (VirtualizationListViewColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty 
            = DependencyProperty.Register("Columns", 
                                          typeof(VirtualizationListViewColumnCollection), 
                                          typeof(VirtualizationListView),
                                          new FrameworkPropertyMetadata(OnColumnsChanged));

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VirtualizationListView)d).SetColumns(e.NewValue as VirtualizationListViewColumnCollection);
        }

        private void SetColumns(VirtualizationListViewColumnCollection columns)
        {
            Columns = columns;

            if (_virtualizationListView == null)
                return;

            var gridView = _virtualizationListView.View as GridView;
            gridView.Columns.Clear();

            var starWidthBinding = new Binding
                {
                    RelativeSource = new RelativeSource { Mode = RelativeSourceMode.FindAncestor, 
                                                          AncestorType = typeof(ListView) },
                    Converter = new StarWidthConverter()
                };

            for (int i = 0; i < Columns.Count; i++)
            {
                var columnHeader = new FilterableGridViewColumnHeader { Content = Columns[i].Header, 
                                                                        ToolTip = Columns[i].ToolTip,
                                                                        Width = Columns[i].Width };
                if (!Columns[i].DisabledSorting)
                    columnHeader.Style = _sortingColumnHeaderStyle;

                //Add fast filter row
                if (!Columns[i].FilterBoundProperty.IsNull)
                {
                    columnHeader.Filter = new FilterRowContainer(Columns[i].FilterBoundProperty,
                        Columns[i].FilterValueTemplate,
                        null, 
                        Columns[i].FilterOperator);
                    columnHeader.Filter.FilterByValue = FilterByValue;
                    ClearFastFilterRow += columnHeader.Filter.ClearValue;
                }

                var gridViewColumn = new GridViewColumn
                    {
                        Header = columnHeader, 
                        Width = Columns[i].Width,
                        CellTemplate = Columns[i].CellTemplate
                    };
                if (Double.IsNaN(gridViewColumn.Width))
                    BindingOperations.SetBinding(gridViewColumn, 
                                                 GridViewColumn.WidthProperty, 
                                                 starWidthBinding);

                gridView.Columns.Add(gridViewColumn);
            }
        }
        #endregion

        #region SelectedItem
        /// <summary>
        /// Dependency property for anchor selected item at VirtualizationListView
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty 
            = DependencyProperty.Register("SelectedItem", 
                                          typeof(object), 
                                          typeof(VirtualizationListView),
                                          new FrameworkPropertyMetadata() 
                                            { BindsTwoWayByDefault = false, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private bool _isUpdateSelection;

        private void VirtualizationListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdateSelection)
            {
                e.Handled = true;
                return;
            }

            if (e.AddedItems.Count > 0)
            {
                if (ItemsSource != null && ItemsSource is ISelectionManager)
                {
                    ((ISelectionManager)ItemsSource).SelectedItem = _virtualizationListView.SelectedItem;
                    ((ISelectionManager)ItemsSource).SelectedIndex = _virtualizationListView.SelectedIndex;

                    Trace.WriteLine($"Select item: {_virtualizationListView.SelectedIndex}");

                    SelectedItem = ((ISelectionManager)ItemsSource).SelectedItem;
                }
            }
            else
                SelectedItem = null;
        }
        #endregion

        #region HasFilter
        /// <summary>
        /// Dependency property for indicating availability filtration at VirtualizationListView (readonly)
        /// </summary>
        public bool HasFilter
        {
            get { return (bool)GetValue(HasFilterProperty); }
            protected set { SetValue(HasFilterPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey HasFilterPropertyKey 
            = DependencyProperty.RegisterReadOnly("HasFilter", 
                                                  typeof(bool), 
                                                  typeof(VirtualizationListView),
                                                  new FrameworkPropertyMetadata(false, 
                                                                                FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty HasFilterProperty = HasFilterPropertyKey.DependencyProperty;

        #endregion

        #region AvailableFilterableProperties
        /// <summary>
        /// Dependency property for anchor available filterable properties at VirtualizationListView
        /// </summary>
        public FilterablePropertyDescriptionCollection AvailableFilterableProperties
        {
            get { return (FilterablePropertyDescriptionCollection)GetValue(AvailableFilterablePropertiesProperty); }
            set { SetValue(AvailableFilterablePropertiesProperty, value); }
        }

        public static readonly DependencyProperty AvailableFilterablePropertiesProperty 
            = DependencyProperty.Register("AvailableFilterableProperties", 
                                          typeof(FilterablePropertyDescriptionCollection), 
                                          typeof(VirtualizationListView),
                                          new FrameworkPropertyMetadata(OnAvailableFilterablePropertiesChanged));

        private static void OnAvailableFilterablePropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VirtualizationListView)d).SetAvailableFilterableProperties(e.NewValue as FilterablePropertyDescriptionCollection);
        }

        private void SetAvailableFilterableProperties(FilterablePropertyDescriptionCollection availableFilterableProperties)
        {
            AvailableFilterableProperties = availableFilterableProperties;

            if (availableFilterableProperties != null
                && availableFilterableProperties.Count > 0)
                HasFilter = true;
        }
        #endregion

        #region Filters
        /// <summary>
        /// Dependency property for filters list at VirtualizationListView
        /// </summary>
        public ObservableCollection<FilterParams> Filters
        {
            get { return (ObservableCollection<FilterParams>)GetValue(FiltersProperty); }
            protected set { SetValue(FiltersPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey FiltersPropertyKey 
            = DependencyProperty.RegisterReadOnly("Filters", 
                                                  typeof(ObservableCollection<FilterParams>), 
                                                  typeof(VirtualizationListView),
                                                  new FrameworkPropertyMetadata(null, 
                                                                                FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty FiltersProperty = FiltersPropertyKey.DependencyProperty;

        private void OnFiltersChanged()
        {
            var filterableItemsSource = ItemsSource as IFilterableCollection;
            if (filterableItemsSource != null)
            {
                var selectedFilter = filterableItemsSource.CurrentFilterParams;
                Filters.Clear();
                foreach (FilterParams filter in filterableItemsSource.AllFilters)
                    Filters.Add(filter);
                SelectedFilter = selectedFilter;
            }
        }

        #endregion

        #region SelectedFilter
        /// <summary>
        /// Dependency property for anchor selected filter at VirtualizationListView
        /// </summary>
        public FilterParams SelectedFilter
        {
            get { return (FilterParams)GetValue(SelectedFilterProperty); }
            set { SetValue(SelectedFilterProperty, value); }
        }
        public static readonly DependencyProperty SelectedFilterProperty 
            = DependencyProperty.Register("SelectedFilter", 
                                          typeof(FilterParams), 
                                          typeof(VirtualizationListView),
                                          new FrameworkPropertyMetadata(OnSelectedFilterChanged) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnSelectedFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VirtualizationListView)d).SetSelectedFilter(e.NewValue as FilterParams, 
                                                          ((VirtualizationListView)d).IsSetFilterByValue);
        }

        private void SetSelectedFilter(FilterParams selectedFilter, bool isSetFilterByValue)
        {
            if (selectedFilter != null)
            {
                if (isSetFilterByValue
                    && selectedFilter.Conditions.Root is ComparisonOperatorNode)
                    OnClearFastFilterRow(
                        ((ExpressionTreeFieldLeaf) ((ComparisonOperatorNode) selectedFilter.Conditions.Root).Left)
                            .PropertyDescription);
                else
                    OnClearFastFilterRow(null);
            }
            else if (!isSetFilterByValue)
            {
                OnClearFastFilterRow(null);
            }

            _virtualizationListView.SelectedItem = null;
            IsDefaultFilter = selectedFilter == null;

            var filterableItemsSource = ItemsSource as IFilterableCollection;
            if (filterableItemsSource != null)
            {
                filterableItemsSource.Filter(selectedFilter);
            }
        }
        #endregion

        #region IsDefaultFilter
        /// <summary>
        /// Dependency property for indicating selected default filter at VirtualizationListView
        /// </summary>
        internal bool IsDefaultFilter
        {
            get { return (bool)GetValue(IsDefaultFilterProperty); }
            set { SetValue(IsDefaultFilterProperty, value); }
        }
        internal static readonly DependencyProperty IsDefaultFilterProperty 
            = DependencyProperty.Register("IsDefaultFilter", 
                                          typeof(bool), 
                                          typeof(VirtualizationListView),
                                          new FrameworkPropertyMetadata(false, OnIsDefaultFilterChanged) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnIsDefaultFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VirtualizationListView)d).SetIsDefaultFilterFilter((bool)e.NewValue);
        }

        private void SetIsDefaultFilterFilter(bool isDefaultFilter)
        {
            if (isDefaultFilter)
                SelectedFilter = null;
        }
        #endregion

        #endregion

        #region Commands

        private DelegateCommand _editQueriesCommand;
        /// <summary>
        /// Show filter configurator command
        /// </summary>
        public ICommand EditQueriesCommand
        {
            get
            {
                return _editQueriesCommand ??
                       (_editQueriesCommand = new DelegateCommand(EditQueries));
            }
        }

        private void EditQueries()
        {
            if (_filterConfiguratorDialog != null)
                _filterConfiguratorDialog.Close();

            var resourceDictionary = new ResourceDictionary
                {
                    Source =
                        new Uri("/VirtualizationListViewControl;component/Themes/FilterConfigurator.xaml",
                            UriKind.RelativeOrAbsolute)
                };

            _filterConfiguratorDialog = new FilterConfigurator(ItemsSource as IFilterableCollection, 
                                                               AvailableFilterableProperties, 
                                                               SelectedFilter)
            {
                Resources = resourceDictionary
            };

            _filterConfiguratorDialog.Show();
        }

        #endregion

        #region Methods

        #region Selection

        /// <summary>
        /// Key down actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyEventArgs"></param>
        private void VirtualizationListView_OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            switch (keyEventArgs.Key)
            {
                //Unselect item
                case Key.Escape:
                {
                    _isUpdateSelection = false;
                    _virtualizationListView.SelectedItem = null;

                    if (ItemsSource != null && ItemsSource is ISelectionManager)
                    {
                        ((ISelectionManager)ItemsSource).SelectedItem = null;
                        ((ISelectionManager)ItemsSource).SelectedIndex = -1;

                        SelectedItem = null;
                    }

                    keyEventArgs.Handled = true;
                    break;
                }
                //Next selection
                case Key.Down:
                {
                    if (ItemsSource != null
                        && ItemsSource is ISelectionManager)
                    {
                        ((ISelectionManager)ItemsSource).NextSelected();

                        keyEventArgs.Handled = true;
                    }
                    break;
                }
                //Previous selection
                case Key.Up:
                {
                    if (ItemsSource != null
                        && ItemsSource is ISelectionManager)
                    {
                        ((ISelectionManager)ItemsSource).PreviousSelected();

                        keyEventArgs.Handled = true;
                    }
                    break;
                }
            }
        }

        private void ClearSelection(bool isFullClear)
        {
            if (isFullClear)
            {
                _isUpdateSelection = false;
                _virtualizationListView.SelectedItem = null;
                return;
            }
            
            if (_isUpdateSelection)
                return;

            _isUpdateSelection = true;
            if (ItemsSource != null 
                && ItemsSource is ISelectionManager)
            {
                ((ISelectionManager)ItemsSource).SelectedItem = _virtualizationListView.SelectedItem;
                ((ISelectionManager)ItemsSource).SelectedIndex = _virtualizationListView.SelectedIndex;
            }
            _virtualizationListView.SelectedItem = null;
        }

        private void ResetSelection()
        {
            if (ItemsSource != null && ItemsSource is ISelectionManager)
                _virtualizationListView.SelectedIndex = ((ISelectionManager)ItemsSource).SelectedIndex;
            if (_virtualizationListView.SelectedIndex > -1)
                _virtualizationListView.ScrollIntoView(_virtualizationListView.Items[_virtualizationListView.SelectedIndex]);

            if (_virtualizationListView.SelectedIndex > -1
                || (ItemsSource != null && ItemsSource is ISelectionManager && ((ISelectionManager)ItemsSource).SelectedIndex == -1))
                _isUpdateSelection = false;
        }

        #endregion

        #region Sorting

        public void SortByColumn(object sender, RoutedEventArgs routedEventArgs)
        {
            var columnHeader = sender as GridViewColumnHeader;
            if (columnHeader == null)
                return;

            bool isAsc = true;
            if (columnHeader.Tag == null)
            {
                columnHeader.Tag = 0;
            }
            else
            {
                columnHeader.Tag = (int)columnHeader.Tag == 1 ? 0 : 1;
                isAsc = (int)columnHeader.Tag == 1 ? false : true;
            }

            var gridViewColumns = columnHeader.Parent as GridViewHeaderRowPresenter;
            if (gridViewColumns == null)
                return;

            for (int i = 0; i < gridViewColumns.Columns.Count; i++)
            {
                var header = gridViewColumns.Columns[i].Header as GridViewColumnHeader;
                if (header == null)
                    continue;

                if (header.Content != columnHeader.Content)
                {
                    header.Tag = null;
                }
                else
                {
                    if (Columns != null
                        && Columns.Count > i
                        && Columns[i] != null)
                    {
                        var sortParam = new SortParams(Columns[i].BoundProperty, isAsc);

                        if (ItemsSource is ISortableCollection)
                            ((ISortableCollection)ItemsSource).Sort(sortParam);
                    }
                }
            }
        }

        #endregion

        #region Filtering

        /// <summary>
        /// Clear fast filter row event
        /// </summary>
        private event Action<FieldDescription?> ClearFastFilterRow;

        /// <summary>
        /// Rise clear fast filter row handler
        /// </summary>
        /// <param name="notClearProperty">Property which not clear fast filter row</param>
        protected virtual void OnClearFastFilterRow(FieldDescription? notClearProperty)
        {
            var handler = ClearFastFilterRow;
            if (handler != null) 
                handler(notClearProperty);
        }

        /// <summary>
        /// Cancel fast filter operation
        /// </summary>
        private CancellationTokenSource _filterByValueTokenSource;

        /// <summary>
        /// Filter by value from fast filter row
        /// </summary>
        /// <param name="property">Filter property</param>
        /// <param name="value">Filter field value</param>
        /// <param name="filterOperator">Filter operator</param>
        private void FilterByValue(FieldDescription property, object value, ComparisonOperators filterOperator)
        {
            if (_filterByValueTokenSource != null)
                _filterByValueTokenSource.Cancel();

            _filterByValueTokenSource = new CancellationTokenSource();

            Task.Run(() => FilterByValueAsync(_filterByValueTokenSource.Token, 
                                              property, 
                                              value, 
                                              filterOperator),
                     _filterByValueTokenSource.Token);
        }

        /// <summary>
        /// Indicate when set filter by value
        /// </summary>
        internal bool IsSetFilterByValue;

        /// <summary>
        /// Async filter by value from fast filter row
        /// </summary>
        /// <param name="cancellationToken">Cancel fast filter operation</param>
        /// <param name="property">Filter property</param>
        /// <param name="value">Filter field value</param>
        /// <param name="filterOperator">Filter operator</param>
        private void FilterByValueAsync(CancellationToken cancellationToken, FieldDescription property, object value, ComparisonOperators filterOperator)
        {
            //Ждем перед тем как фильтровать, чтобы пользователь мог ввести оставшееся значение фильтра
            Thread.Sleep(1000);

            try
            {
                //Завершаем поток если пришли еще данные от пользователя
                cancellationToken.ThrowIfCancellationRequested();

                Trace.WriteLine("Filter by value from fast filter row");

                FilterParams filter = null;
                if (value != null
                    && !String.IsNullOrEmpty(value.ToString()))
                {
                    var condition = new ExpressionTree(filterOperator,
                                                       new ExpressionTreeFieldLeaf(null, property),
                                                       new ExpressionTreeValueLeaf(null, value));
                    filter = new FilterParams(condition);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsSetFilterByValue = true;
                    IsDefaultFilter = true;
                    SelectedFilter = filter;
                    IsSetFilterByValue = false;
                });
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("New filter from user. Cancel current filter.");
            }    
        }

        #endregion

        #region CollectionChanged

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_countTextBlock == null)
                return;

            var list = sender as IList;
            if (list == null)
            {
                _countTextBlock.Text = "0";
            }
            else
            {
                _countTextBlock.Text = list.Count.ToString();
                Trace.WriteLine("Read IList.Count");
            }
        }

        #endregion

        #endregion
    }
}
