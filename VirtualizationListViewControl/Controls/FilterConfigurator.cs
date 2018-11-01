using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using VirtualizationListView.SortAndFilterDTO.Filtering;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions;
using VirtualizationListView.SortAndFilterDTO.Filtering.FilterExpressions.Operators;
using VirtualizationListViewControl.Helpers;
using VirtualizationListViewControl.Interfaces;
using VirtualizationListViewControl.SlaveTypes;

namespace VirtualizationListViewControl.Controls
{
    /// <summary>
    /// Filter configurator window
    /// </summary>
    internal class FilterConfigurator : Window
    {
        #region Fields

        private readonly IFilterableCollection _filterableCollection;
        private FilterParams _beginningSelectedFilter;

        #endregion

        #region Constructors & Initializes

        public FilterConfigurator(IFilterableCollection filterableCollection, FilterablePropertyDescriptionCollection availableFilterableProperties, FilterParams selectedFilter)
        {
            DefaultStyleKey = typeof(FilterConfigurator);

            _filterableCollection = filterableCollection;
            _beginningSelectedFilter = selectedFilter;

            Filters.Clear();
            if (_filterableCollection != null
                && _filterableCollection.AllFilters != null)
                foreach (var filterParams in filterableCollection.AllFilters)
                    Filters.Add(filterParams);

            FilterableArguments = availableFilterableProperties;
            SelectedFilterConfiguration = selectedFilter;

            if (selectedFilter != null)
                SelectedExpression = selectedFilter.Conditions.Root;

            //Set the default owner to the app main window (if possible)
            if (Application.Current != null 
                && Application.Current.MainWindow != this)
                Owner = Application.Current.MainWindow;
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call System.Windows.FrameworkElement.ApplyTemplate().
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        #endregion

        #region DependencyProperties

        #region Filters
        /// <summary>
        /// Dependency property for filters list at FilterConfigurator
        /// </summary>
        public ObservableCollection<FilterParams> Filters
        {
            get { return (ObservableCollection<FilterParams>)GetValue(FiltersProperty); }
            protected set { SetValue(FiltersPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey FiltersPropertyKey 
            = DependencyProperty.RegisterReadOnly("Filters", 
                                                  typeof(ObservableCollection<FilterParams>), 
                                                  typeof(FilterConfigurator),
                                                  new FrameworkPropertyMetadata(new ObservableCollection<FilterParams>(), 
                                                                                FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty FiltersProperty = FiltersPropertyKey.DependencyProperty;

        #endregion

        #region SelectedFilterConfiguration
        /// <summary>
        /// Dependency property for associating selected filter to FilterConfigurator
        /// </summary>
        public FilterParams SelectedFilterConfiguration
        {
            get { return (FilterParams)GetValue(SelectedFilterConfigurationProperty); }
            set { SetValue(SelectedFilterConfigurationProperty, value); }
        }
        public static readonly DependencyProperty SelectedFilterConfigurationProperty 
            = DependencyProperty.Register("SelectedFilterConfiguration", 
                                          typeof(FilterParams), 
                                          typeof(FilterConfigurator),
                                          new FrameworkPropertyMetadata(OnSelectedFilterConfigurationChanged) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnSelectedFilterConfigurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FilterConfigurator)d).SetSelectedFilterConfiguration((FilterParams)e.NewValue);
        }

        private void SetSelectedFilterConfiguration(FilterParams filterParams)
        {
            SelectedExpression = filterParams != null ? filterParams.Conditions.Root : null;
        }

        #endregion

        #region FilterableArguments
        /// <summary>
        /// Dependency property for filterable arguments at FilterConfigurator
        /// </summary>
        public FilterablePropertyDescriptionCollection FilterableArguments
        {
            get { return (FilterablePropertyDescriptionCollection)GetValue(FilterableArgumentsProperty); }
            set { SetValue(FilterableArgumentsProperty, value); }
        }

        public static readonly DependencyProperty FilterableArgumentsProperty 
            = DependencyProperty.Register("FilterableArguments", 
                                          typeof(FilterablePropertyDescriptionCollection), 
                                          typeof(FilterConfigurator),
                                          new FrameworkPropertyMetadata() 
                                            { BindsTwoWayByDefault = false, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        #endregion

        #region SelectedFilterableArgument
        /// <summary>
        /// Dependency property for selected filterable argument at FilterConfigurator
        /// </summary>
        public FilterablePropertyDescription SelectedFilterableArgument
        {
            get { return (FilterablePropertyDescription)GetValue(SelectedFilterableArgumentProperty); }
            set { SetValue(SelectedFilterableArgumentProperty, value); }
        }
        public static readonly DependencyProperty SelectedFilterableArgumentProperty 
            = DependencyProperty.Register("SelectedFilterableArgument", 
                                          typeof(FilterablePropertyDescription), 
                                          typeof(FilterConfigurator),
                                          new FrameworkPropertyMetadata() 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        #endregion

        #region IsAddingNewFilter
        /// <summary>
        /// Dependency property for indicating adding new filter at FilterConfigurator
        /// </summary>
        internal bool IsAddingNewFilter
        {
            get { return (bool)GetValue(IsAddingNewFilterProperty); }
            set { SetValue(IsAddingNewFilterProperty, value); }
        }
        internal static readonly DependencyProperty IsAddingNewFilterProperty 
            = DependencyProperty.Register("IsAddingNewFilter", 
                                          typeof(bool), 
                                          typeof(FilterConfigurator),
                                          new FrameworkPropertyMetadata(false) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        #endregion

        #region NewFilterName
        /// <summary>
        /// Dependency property for new filter name at FilterConfigurator
        /// </summary>
        internal string NewFilterName
        {
            get { return (string)GetValue(NewFilterNameProperty); }
            set { SetValue(NewFilterNameProperty, value); }
        }
        internal static readonly DependencyProperty NewFilterNameProperty 
            = DependencyProperty.Register("NewFilterName", 
                                          typeof(string), 
                                          typeof(FilterConfigurator),
                                          new FrameworkPropertyMetadata(String.Empty) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        #endregion

        #region SelectedExpression
        /// <summary>
        /// Dependency property for selected filter expression at FilterConfigurator
        /// </summary>
        public ExpressionTreeNode SelectedExpression
        {
            get { return (ExpressionTreeNode)GetValue(SelectedExpressionProperty); }
            set { SetValue(SelectedExpressionProperty, value); }
        }
        public static readonly DependencyProperty SelectedExpressionProperty 
            = DependencyProperty.Register("SelectedExpression", 
                                          typeof(ExpressionTreeNode), 
                                          typeof(FilterConfigurator),
                                          new FrameworkPropertyMetadata(OnSelectedExpressionChanged) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnSelectedExpressionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FilterConfigurator)d).SetSelectedExpression((ExpressionTreeNode)e.NewValue);
        }

        private void SetSelectedExpression(ExpressionTreeNode selectedExpression)
        {
            var comparisonNode = selectedExpression as ComparisonOperatorNode;
            if (comparisonNode != null)
            {
                var fieldLeaf = comparisonNode.Left as ExpressionTreeFieldLeaf;
                if (fieldLeaf != null)
                {
                    var foundFieldDescr =
                        FilterableArguments.FirstOrDefault(
                            prop => prop.BoundProperty.Equals(fieldLeaf.PropertyDescription));
                    if (foundFieldDescr != null)
                    {
                        var newAvailableComparisonOperators = new LocalizableEnumItemsSource();
                        newAvailableComparisonOperators.Initialize(typeof(ComparisonOperators), foundFieldDescr.AvailableOperators.Cast<object>());
                        AvailableComparisonOperators = newAvailableComparisonOperators;

                        //Set available values for expression
                        if (foundFieldDescr.HasAvailableValues)
                        {
                            var valueLeaf = comparisonNode.Right as ExpressionTreeValueLeaf;
                            if (valueLeaf != null)
                            {
                                valueLeaf.AvailableValues =
                                    _filterableCollection.GetExpressionAvailableValues(foundFieldDescr.BoundProperty);
                            }
                        }
                    }
                }

                SelectedComparisonOperator = comparisonNode.Operator;
            }
        }

        #endregion

        #region AvailableComparisonOperators
        /// <summary>
        /// Dependency property for  available comparison operators at FilterConfigurator
        /// </summary>
        public LocalizableEnumItemsSource AvailableComparisonOperators
        {
            get { return (LocalizableEnumItemsSource)GetValue(AvailableComparisonOperatorsProperty); }
            set { SetValue(AvailableComparisonOperatorsProperty, value); }
        }
        public static readonly DependencyProperty AvailableComparisonOperatorsProperty 
            = DependencyProperty.Register("AvailableComparisonOperators", 
                                          typeof(LocalizableEnumItemsSource), 
                                          typeof(FilterConfigurator),
                                          new FrameworkPropertyMetadata(new LocalizableEnumItemsSource {Type = typeof(ComparisonOperators) }) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        #endregion

        #region SelectedComparisonOperator
        /// <summary>
        /// Dependency property for selected comparison operator at FilterConfigurator
        /// </summary>
        public ComparisonOperators SelectedComparisonOperator
        {
            get { return (ComparisonOperators)GetValue(SelectedComparisonOperatorProperty); }
            set { SetValue(SelectedComparisonOperatorProperty, value); }
        }
        public static readonly DependencyProperty SelectedComparisonOperatorProperty 
            = DependencyProperty.Register("SelectedComparisonOperator", 
                                          typeof(ComparisonOperators), 
                                          typeof(FilterConfigurator),
                                          new FrameworkPropertyMetadata(OnSelectedComparisonOperatorChanged) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnSelectedComparisonOperatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FilterConfigurator) d).SetSelectedComparisonOperator((ComparisonOperators) e.NewValue);
        }

        private void SetSelectedComparisonOperator(ComparisonOperators selectedOperator)
        {
            var comparisonNode = SelectedExpression as ComparisonOperatorNode;
            if (comparisonNode != null)
            {
                comparisonNode.Operator = selectedOperator;
            }
        }

        #endregion

        #region FilterValidationError
        /// <summary>
        /// Dependency property for filter validation error text at FilterConfigurator
        /// </summary>
        internal string FilterValidationError
        {
            get { return (string)GetValue(FilterValidationErrorProperty); }
            set { SetValue(FilterValidationErrorProperty, value); }
        }
        internal static readonly DependencyProperty FilterValidationErrorProperty 
            = DependencyProperty.Register("FilterValidationError", 
                                          typeof(string), 
                                          typeof(FilterConfigurator),
                                          new FrameworkPropertyMetadata(String.Empty) 
                                            { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        #endregion

        #endregion

        #region Commands

        #region FinishFiltersConfigurateCommand
        private DelegateCommand _finishFiltersConfigurateCommand;
        /// <summary>
        /// Finish filters configurate command. 
        /// Save configurate filters and close filter configurator.
        /// </summary>
        public ICommand FinishFiltersConfigurateCommand
        {
            get
            {
                return _finishFiltersConfigurateCommand ??
                       (_finishFiltersConfigurateCommand = new DelegateCommand(FinishFiltersConfigurate));
            }
        }

        private void FinishFiltersConfigurate()
        {
            var canClose = true;
            if (_filterableCollection != null
                && Filters != null)
            {
                //Add new filters and update exists
                var existsFilters = new List<FilterParams>();
                foreach (var filter in Filters)
                {
                    var verifyFilter = filter.ValidateConditions();
                    if (verifyFilter.ValidationResult)
                    {
                        if (_filterableCollection.AllFilters != null
                            && _filterableCollection.AllFilters.Exists(f => f.Equals(filter)))
                        {
                            existsFilters.Add(filter);
                        }
                        else
                        {
                            _filterableCollection.AddNewFilter(filter);
                        }
                    }
                    else
                    {
                        FilterValidationError = verifyFilter.ValidationErrorMessage;

                        canClose = false;
                        break;
                    }
                }
                if (existsFilters.Any())
                    _filterableCollection.UpdateFilters(existsFilters);
            }

            if (canClose)
            {
                if (_filterableCollection != null
                    && _beginningSelectedFilter != null)
                    _filterableCollection.Filter(_beginningSelectedFilter);
                
                Close();
            }
        }
        #endregion

        #region AddNewFilterCommand
        private DelegateCommand<string> _addNewFilterCommand;
        /// <summary>
        /// Add new filter command
        /// </summary>
        public ICommand AddNewFilterCommand
        {
            get
            {
                return _addNewFilterCommand ??
                       (_addNewFilterCommand = new DelegateCommand<string>(AddNewFilter));
            }
        }

        private void AddNewFilter(string filterName)
        {
            if (String.IsNullOrWhiteSpace(filterName)
                && !IsAddingNewFilter)
            {
                NewFilterName = String.Empty;
                IsAddingNewFilter = true;
            }
            else
            {
                var newFilter = new FilterParams(filterName, new ExpressionTree());
                Filters.Add(newFilter);
                SelectedFilterConfiguration = newFilter;
                IsAddingNewFilter = false;
            }
        }
        #endregion

        #region CancelAddingNewFilterCommand
        private DelegateCommand _cancelAddingNewFilterCommand;
        /// <summary>
        /// Cancel adding new filter command
        /// </summary>
        public ICommand CancelAddingNewFilterCommand
        {
            get
            {
                return _cancelAddingNewFilterCommand ??
                       (_cancelAddingNewFilterCommand = new DelegateCommand(CancelAddingNewFilter));
            }
        }

        private void CancelAddingNewFilter()
        {
            IsAddingNewFilter = false;
        }
        #endregion

        #region RemoveFilterCommand
        private DelegateCommand<FilterParams> _removeFilterCommand;
        /// <summary>
        /// Remove filter command
        /// </summary>
        public ICommand RemoveFilterCommand
        {
            get
            {
                return _removeFilterCommand ??
                       (_removeFilterCommand = new DelegateCommand<FilterParams>(RemoveFilter));
            }
        }

        private void RemoveFilter(FilterParams removedFilter)
        {
            Filters.Remove(removedFilter);
            SelectedExpression = null;

            if (_beginningSelectedFilter != null
                && _beginningSelectedFilter.Name == removedFilter.Name)
                _beginningSelectedFilter = null;

            if (_filterableCollection != null)
                _filterableCollection.RemoveFilter(removedFilter.Name);
        }
        #endregion

        #region AddOperandCommand
        private DelegateCommand _addOperandCommand;
        /// <summary>
        /// Add operand to expression command
        /// </summary>
        public ICommand AddOperandCommand
        {
            get
            {
                return _addOperandCommand ??
                       (_addOperandCommand = new DelegateCommand(AddOperand, AddOperandCanExecute));
            }
        }

        private bool AddOperandCanExecute()
        {
            if (SelectedFilterConfiguration != null
                && SelectedFilterableArgument != null)
                return true;
            return false;
        }

        private void AddOperand()
        {
            if (SelectedFilterConfiguration == null
                || SelectedFilterableArgument == null)
                return;

            var fieldLeaf = new ExpressionTreeFieldLeaf(null, SelectedFilterableArgument.BoundProperty);

            object value;
            if (SelectedFilterableArgument.DefaultValue != null)
                value = SelectedFilterableArgument.DefaultValue;
            else if (fieldLeaf.Property.PropertyType == typeof(DateTime))
                value = DateTime.Now;
            else if (fieldLeaf.Property.PropertyType == typeof(String))
                value = String.Empty;
            else if (fieldLeaf.Property.PropertyType.IsAbstract)
                value = null;
            else
                value = Activator.CreateInstance(fieldLeaf.Property.PropertyType);

            var valueLeaf = new ExpressionTreeValueLeaf(null, value);

            var compariosonNode = new ComparisonOperatorNode(null,
                                                             fieldLeaf,
                                                             valueLeaf,
                                                             SelectedFilterableArgument.AvailableOperators.FirstOrDefault());

            //First node always add as comparision node
            if (SelectedFilterConfiguration.Conditions.Root == null)
            {
                SelectedFilterConfiguration.Conditions.Root = compariosonNode;
            }
            else
            {
                //Default add join node as AND operator
                if (SelectedExpression == null)
                {
                    //If adding to root
                    var rootBooleanNode = new BooleanOperatorNode(null,
                                                                  compariosonNode,
                                                                  SelectedFilterConfiguration.Conditions.Root,
                                                                  BooleanOperators.And);
                    SelectedFilterConfiguration.Conditions.Root = rootBooleanNode;
                }
                else if (SelectedExpression is BooleanOperatorNode
                         && ((BooleanOperatorNode) SelectedExpression).Operator != BooleanOperators.Not
                         && SelectedExpression.Left == null)
                {
                    //Adding to existing node
                    SelectedExpression.Left = compariosonNode;

                    SelectedFilterConfiguration.Conditions.Root = SelectedFilterConfiguration.Conditions.Root.Clone() as ExpressionTreeNode;
                }
                else if (SelectedExpression.Parent is BooleanOperatorNode
                         && ((BooleanOperatorNode) SelectedExpression.Parent).Operator != BooleanOperators.Not
                         && SelectedExpression.Parent.Left == null)
                {
                    //Adding to existing node
                    SelectedExpression.Parent.Left = compariosonNode;

                    SelectedFilterConfiguration.Conditions.Root = SelectedFilterConfiguration.Conditions.Root.Clone() as ExpressionTreeNode;
                }
                else
                {
                    //Adding to arbitrary node
                    bool isParentNull = SelectedExpression.Parent == null;
                    bool isSelectedBoolNode = SelectedExpression is BooleanOperatorNode;

                    ExpressionTreeElement leftExpession;
                    ExpressionTreeElement rightExpression;

                    if (isSelectedBoolNode)
                        leftExpession = SelectedExpression.Right;
                    else if (isParentNull)
                        leftExpession = SelectedExpression;
                    else
                        leftExpession = SelectedExpression.Parent.Right;

                    if (leftExpession is BooleanOperatorNode)
                    {
                        rightExpression = leftExpession;
                        leftExpession = compariosonNode;
                    }
                    else
                    {
                        rightExpression = compariosonNode;
                    }

                    var newBoolNode =
                            new BooleanOperatorNode(
                                SelectedExpression.Parent,
                                leftExpession != null ? (leftExpession.Clone() as ExpressionTreeElement) : null,
                                rightExpression.Clone() as ExpressionTreeElement,
                                BooleanOperators.And);

                    if (isParentNull
                        && !isSelectedBoolNode)
                    {
                        SelectedFilterConfiguration.Conditions.Root = newBoolNode;
                    }
                    else
                    {
                        Trace.WriteLine($"Before adding to SelectedExpression.Parent\r\n{SelectedExpression.Parent}");

                        if (isSelectedBoolNode)
                            SelectedExpression.Right = newBoolNode;
                        else
                            SelectedExpression.Parent.Right = newBoolNode;

                        Trace.WriteLine($"After adding to SelectedExpression.Parent\r\n{SelectedExpression.Parent}");

                        SelectedFilterConfiguration.Conditions.Root = SelectedFilterConfiguration.Conditions.Root.Clone() as ExpressionTreeNode;
                    }

                    Trace.WriteLine($"Final expression after adding\r\n{SelectedFilterConfiguration.Conditions.Root}");
                }
            }

            if (SelectedFilterConfiguration.Conditions.Root != null)
            {
                Task.Run(() =>
                {
                    Thread.Sleep(100);
                    Application.Current.Dispatcher.Invoke(() => 
                        SelectedExpression = SelectedFilterConfiguration.Conditions.Root.Find(compariosonNode) as ExpressionTreeNode);
                });
            }
            else
            {
                SelectedExpression = null;
            }
        }
        #endregion

        #region AddNodeCommand
        private DelegateCommand<BooleanOperators> _addNodeCommand;
        /// <summary>
        /// Add logical operator node to expression command
        /// </summary>
        public ICommand AddNodeCommand
        {
            get
            {
                return _addNodeCommand ??
                       (_addNodeCommand = new DelegateCommand<BooleanOperators>(AddNode, AddNodeCanExecute));
            }
        }

        private bool AddNodeCanExecute(BooleanOperators type)
        {
            if (SelectedFilterConfiguration != null)
                return true;
            return false;
        }

        private void AddNode(BooleanOperators type)
        {
            if (SelectedFilterConfiguration == null
                || SelectedExpression == null)
                return;

            if (SelectedExpression is BooleanOperatorNode
                && ((BooleanOperatorNode)SelectedExpression).Operator == type)
                return;

            ExpressionTreeElement left;
            ExpressionTreeElement right;
            if (type == BooleanOperators.Not)
            {
                left = SelectedExpression.Clone() as ExpressionTreeElement;
                right = null;
            }
            else
            {
                left = null;
                right = SelectedExpression.Clone() as ExpressionTreeElement;
            }

            var newNode = new BooleanOperatorNode(SelectedExpression.Parent,
                                                  left,
                                                  right, 
                                                  type);

            var replaceResult = SelectedFilterConfiguration.Conditions.FindAndReplace(SelectedExpression, newNode);
            if (!replaceResult)
                Trace.WriteLine("Cant replace SelectedExpression");

            if (SelectedFilterConfiguration.Conditions.Root != null)
                SelectedFilterConfiguration.Conditions.Root = SelectedFilterConfiguration.Conditions.Root.Clone() as ExpressionTreeNode;
        }
        #endregion

        #region RemoveOperandCommand
        private DelegateCommand _removeOperandCommand;
        /// <summary>
        /// Remove operand from expression command
        /// </summary>
        public ICommand RemoveOperandCommand
        {
            get
            {
                return _removeOperandCommand ??
                       (_removeOperandCommand = new DelegateCommand(RemoveOperand, RemoveOperandCanExecute));
            }
        }

        private bool RemoveOperandCanExecute()
        {
            if (SelectedFilterConfiguration != null
                && SelectedExpression != null)
                return true;
            return false;
        }

        private void RemoveOperand()
        {
            if (SelectedFilterConfiguration == null
                || SelectedExpression == null)
                return;

            if (SelectedExpression.Parent == null)
            {
                SelectedFilterConfiguration.Conditions.Root = null;
            }
            else
            {
                var parent = SelectedExpression.Parent as BooleanOperatorNode;
                if (parent != null)
                {
                    ExpressionTreeElement expression;
                    if (SelectedExpression.Equals(parent.Right))
                        expression = parent.Left;
                    else
                        expression = parent.Right;
                    if (expression != null)
                        expression.Parent = parent.Parent;

                    var isReplace = SelectedFilterConfiguration.Conditions.FindAndReplace(SelectedExpression.Parent, expression);
                    if (!isReplace)
                        Trace.WriteLine("Cant replace SelectedExpression.Parent");
                }
                else
                    Trace.WriteLine("SelectedExpression.Parent [is not BooleanOperatorNode] OR [== null]");
            }

            if (SelectedFilterConfiguration.Conditions.Root != null)
                SelectedFilterConfiguration.Conditions.Root = SelectedFilterConfiguration.Conditions.Root.Clone() as ExpressionTreeNode;

            SelectedExpression = null;
        }
        #endregion

        #region ClearExpressionCommand
        private DelegateCommand _clearExpressionCommand;
        /// <summary>
        /// Clear expression command
        /// </summary>
        public ICommand ClearExpressionCommand
        {
            get
            {
                return _clearExpressionCommand ??
                       (_clearExpressionCommand = new DelegateCommand(ClearExpression, ClearExpressionCanExecute));
            }
        }

        private bool ClearExpressionCanExecute()
        {
            if (SelectedFilterConfiguration != null)
                return true;
            return false;
        }

        private void ClearExpression()
        {
            if (SelectedFilterConfiguration == null
                || SelectedFilterConfiguration.Conditions.Root == null)
                return;

            SelectedExpression = null;
            SelectedFilterConfiguration.Conditions.Root = null;
        }
        #endregion

        #endregion
    }
}
