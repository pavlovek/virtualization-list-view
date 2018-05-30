using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SamplesBasicDto;
using SampleWpfApplication.Models;
using VirtualizationListViewControl.Collection;
using VirtualizationListViewControl.Helpers;

namespace SampleWpfApplication.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private readonly SimpleHttpResponcesDataSource _simpleHttpResponcesDataSource;
        private readonly FilteringAndSortingHttpResponcesDataSource _filteringAndSortingHttpResponcesDataSource;
        private readonly ItemsChangingHttpResponceDataSource _itemsChangingHttpResponceDataSource;
        private const int ListPageSize = 100;
        private const int ListPageTimeout = 30000;

        private readonly Timer _updateUsedMemoryTimer;
        private readonly Timer _updateResponcesList;

        private readonly TimeSpan _updatingResponceTimeInterval = new TimeSpan(0, 0, 0, 0, 350);

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            _simpleHttpResponcesDataSource = new SimpleHttpResponcesDataSource();
            _filteringAndSortingHttpResponcesDataSource = new FilteringAndSortingHttpResponcesDataSource();
            _itemsChangingHttpResponceDataSource = new ItemsChangingHttpResponceDataSource();

            _updateUsedMemoryTimer = new Timer(state => UpdateMemoryUsed(), 
                                               null, 
                                               TimeSpan.Zero,
                                               new TimeSpan(0, 0, 0, 1));
            _updateResponcesList = new Timer(state => UpdateResponcesList(),
                                             null,
                                             Timeout.Infinite,
                                             Timeout.Infinite);
        }

        #endregion

        #region Properties

        /// <summary>
        /// HttpResponces virtualizing list
        /// </summary>
        public VirtualizingCollection<HttpResponce> HttpResponcesList { get; set; }

        /// <summary>
        /// Selected item from HttpResponcesList
        /// </summary>
        public HttpResponce SelectedHttpResponce
        {
            get { return _selectedHttpResponce; }
            set { _selectedHttpResponce = value; OnPropertyChanged("SelectedHttpResponce"); }
        }
        private HttpResponce _selectedHttpResponce = null;

        /// <summary>
        /// How many memory use application (in Mb)
        /// </summary>
        public double UsingMemory
        {
            get { return _usingMemory; }
            set { _usingMemory = value; OnPropertyChanged("UsingMemory"); }
        }
        private double _usingMemory;

        /// <summary>
        /// Log list with descriptions about changes
        /// </summary>
        public ObservableCollection<string> ChangingListLog
        {
            get { return _changingListLog; }
            set { _changingListLog = value; OnPropertyChanged("ChangingListLog"); }
        }
        private ObservableCollection<string> _changingListLog = new ObservableCollection<string>();

        #endregion

        #region Commands

        private DelegateCommand _startUpadatingResponcesListCommand;
        /// <summary>
        /// Start changing Http responces list command
        /// </summary>
        public ICommand StartUpadatingResponcesListCommand
        {
            get
            {
                return _startUpadatingResponcesListCommand ??
                       (_startUpadatingResponcesListCommand = new DelegateCommand(StartUpadatingResponcesList));
            }
        }

        private void StartUpadatingResponcesList()
        {
            ChangingListLog.Insert(0, "START");
            _updateResponcesList.Change(TimeSpan.Zero,
                                        _updatingResponceTimeInterval);
        }

        private DelegateCommand _stopUpadatingResponcesListCommand;
        /// <summary>
        /// Stop changing Http responces list command
        /// </summary>
        public ICommand StopUpadatingResponcesListCommand
        {
            get
            {
                return _stopUpadatingResponcesListCommand ??
                       (_stopUpadatingResponcesListCommand = new DelegateCommand(StopUpadatingResponcesList));
            }
        }

        private void StopUpadatingResponcesList()
        {
            ChangingListLog.Insert(0, "STOP");
            _updateResponcesList.Change(Timeout.Infinite,
                                        Timeout.Infinite);
        }

        private DelegateCommand _clearResponcesListCommand;
        /// <summary>
        /// Clear Http responces list command
        /// </summary>
        public ICommand ClearResponcesListCommand
        {
            get
            {
                return _clearResponcesListCommand ??
                       (_clearResponcesListCommand = new DelegateCommand(ClearResponcesList));
            }
        }

        private void ClearResponcesList()
        {
            ChangingListLog.Insert(0, "CLEAR LIST");
            _itemsChangingHttpResponceDataSource.ClearList();
        }

        #endregion

        #region Methods

        public void InicializedSimpleDataSource()
        {
            HttpResponcesList =
                    new VirtualizingCollection<HttpResponce>(
                        _simpleHttpResponcesDataSource,
                        ListPageSize,
                        ListPageTimeout);
            _simpleHttpResponcesDataSource.FetchRangeCommand(0, ListPageSize);
            OnPropertyChanged("HttpResponcesList");
        }

        public void InicializedFilteringAndSortingDataSource()
        {
            HttpResponcesList =
                    new VirtualizingCollection<HttpResponce>(
                        _filteringAndSortingHttpResponcesDataSource,
                        ListPageSize,
                        ListPageTimeout);
            _filteringAndSortingHttpResponcesDataSource.FetchRangeCommand(0, ListPageSize);
            OnPropertyChanged("HttpResponcesList");
        }

        public void InicializedItemsChangingDataSource()
        {
            HttpResponcesList =
                new VirtualizingCollection<HttpResponce>(
                    _itemsChangingHttpResponceDataSource,
                    ListPageSize,
                    ListPageTimeout);
            _itemsChangingHttpResponceDataSource.FetchRangeCommand(0, ListPageSize);
            OnPropertyChanged("HttpResponcesList");
        }

        private void UpdateMemoryUsed()
        {
            var memSize = GC.GetTotalMemory(true);
            UsingMemory = Math.Round(memSize / 1048576.0, 2);
        }

        private void UpdateResponcesList()
        {
            var logStr = _itemsChangingHttpResponceDataSource.RandomUpdateHttpResponce();
            Application.Current.Dispatcher.Invoke(() => ChangingListLog.Insert(0, logStr));
        }

        #endregion
    }
}
