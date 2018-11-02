using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Windows.Threading;
using VirtualizationListView.SortAndFilterDTO;
using VirtualizationListView.SortAndFilterDTO.Filtering;
using VirtualizationListView.SortAndFilterDTO.Sorting;
using VirtualizationListViewControl.Interfaces;
using VirtualizationListViewControl.ServerListChangesCallBack;

namespace VirtualizationListViewControl.Collection
{
    /// <summary>
    /// Special implementation of IList for data virtualization. Collection split by pages which dynamic provided by 
    /// ItemsProvider corrective maintenance. Oldest pages delete on discharge target time.
    /// </summary>
    /// <typeparam name="T">Data item object</typeparam>
    public class VirtualizingCollection<T> : IList<DataWrapper<T>>, IList, INotifyCollectionChanged, INotifyPropertyChanged, ISelectionManager, ISortableCollection, IFilterableCollection, IDisposable
    {
        #region Constants

        /// <summary>
        /// Default page size
        /// </summary>
        private const int DefaultPageSize = 25;

        /// <summary>
        /// Default page timeout for delete page
        /// </summary>
        private const long DefaultPageTimeout = 30000;

        #endregion

        #region Stopwatch

        protected Stopwatch Stopwatch;

        private readonly Dispatcher _uiThreadDispatcher;

        public event Action<string, long> OperationFinished;

        protected virtual void OnOperationFinished(String operationName)
        {
            Action<string, long> handler = OperationFinished;
            if (handler != null)
            {
                object[] args = { operationName, Stopwatch.ElapsedMilliseconds };
                _uiThreadDispatcher.Invoke(handler, args);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor with items provider, page size and her timeout
        /// </summary>
        /// <param name="itemsProvider">Data items provider</param>
        /// <param name="pageSize">Page size for loading items</param>
        /// <param name="pageTimeout">Page timeout for delete page</param>
        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize, long pageTimeout)
            : this(itemsProvider, pageSize)
        {
            _pageTimeout = pageTimeout;
        }

        /// <summary>
        /// Constructor with items provider and page size
        /// </summary>
        /// <param name="itemsProvider">Data items provider</param>
        /// <param name="pageSize">Page size for loading items</param>
        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize)
            : this(itemsProvider)
        {
            _pageSize = pageSize;
        }

        /// <summary>
        /// Constructor with items provider
        /// </summary>
        /// <param name="itemsProvider">Data items provider</param>
        public VirtualizingCollection(IItemsProvider<T> itemsProvider)
        {
            _itemsProvider = itemsProvider;

            SelectedItem = default(T);

            Timer.Interval = new TimeSpan(0, 0, 30);
            Timer.Tick += CleanUpPagesOnTimerTick;
            Timer.Start();

            Stopwatch = new Stopwatch();

            _uiThreadDispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(_uiThreadDispatcher));

            ListEventsHandlerActionBlock = new ActionBlock<List<ServerListChanging<T>>>((Action<List<ServerListChanging<T>>>)ListChangesHandler);

            SubscribeToItemsProviderEvents();
        }

        #endregion

        #region ItemsProvider

        private readonly IItemsProvider<T> _itemsProvider;

        /// <summary>
        /// Data items provider
        /// </summary>
        public IItemsProvider<T> ItemsProvider
        {
            get { return _itemsProvider; }
        }

        #endregion

        #region Paging
        
        /// <summary>
        /// Data items pages which view now
        /// </summary>
        private readonly ConcurrentDictionary<int, DataPage<T>> _pages = new ConcurrentDictionary<int, DataPage<T>>();
        
        #region PageRemoving

        /// <summary>
        /// Timer periodic delete unused pages
        /// </summary>
        protected DispatcherTimer Timer = new DispatcherTimer();

        /// <summary>
        /// Delete unused pages
        /// </summary>
        public void CleanUpPages()
        {
            Timer.Stop();
            Trace.WriteLine("CleanUpPages::");

            List<int> keys = new List<int>(_pages.Keys);
            foreach (int key in keys)
            {
                // ItemsControl rapid refer to zero-page, so it not delete
                if (key != 0 && (DateTime.Now - _pages[key].TouchTime).TotalMilliseconds > PageTimeout)
                {
                    bool removePage = true;
                    DataPage<T> page;
                    if (_pages.TryGetValue(key, out page))
                        removePage = !page.IsInUse;
                    if (removePage)
                    {
                        DataPage<T> removedPage;
                        var isRemove = _pages.TryRemove(key, out removedPage);
                        if (isRemove)
                            Trace.WriteLine($"Removed Page: {key}");
                        else
                            Trace.WriteLine($"Cant removed Page: {key}");
                    }
                }
            }

            Trace.WriteLine("::CleanUpPages");
            Timer.Start();
        }

        /// <summary>
        /// Rise CleanUpPages method when timer work
        /// </summary>
        private void CleanUpPagesOnTimerTick(object sender, EventArgs e)
        {
            CleanUpPages();
        }

        #endregion

        /// <summary>
        /// Inizialise page in dictionary
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="dataItems">Page data items</param>
        protected virtual void PopulatePage(int pageIndex, IList<T> dataItems)
        {
            if (_pages.ContainsKey(pageIndex))
            {
                DataPage<T> page;
                if (_pages.TryGetValue(pageIndex, out page))
                {
                    page.Populate(dataItems);
                }

                Trace.WriteLine($"Page populated: {pageIndex}");
            }
        }

        /// <summary>
        /// Request specified page. Create default items for her and update last touch time.
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        protected virtual void RequestPage(int pageIndex)
        {
            if (!_pages.ContainsKey(pageIndex))
            {
                int pageLength;
                if (PageSize < AccumulatedCount - pageIndex * PageSize + 1)
                    pageLength = PageSize;
                else
                    pageLength = AccumulatedCount - pageIndex * PageSize;

                if (pageLength <= 0)
                    return;

                var page = new DataPage<T>(pageIndex * PageSize, pageLength);

                var isAdded = _pages.TryAdd(pageIndex, page);

                if (isAdded)
                {
                    Trace.WriteLine($"Added page: {pageIndex}");

                    FetchPageCommand(pageIndex, pageLength);
                }
                else
                {
                    Trace.WriteLine($"Can't add page: {pageIndex}");
                }
            }
            else
            {
                _pages[pageIndex].TouchTime = DateTime.Now;
            }
        }

        #endregion

        #region PageSize

        private readonly int _pageSize = DefaultPageSize;

        /// <summary>
        /// One page size for loading items
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
        }

        #endregion

        #region PageTimeout

        private readonly long _pageTimeout = DefaultPageTimeout;

        /// <summary>
        /// Page timeout for delete page if her long ago not touched
        /// </summary>
        public long PageTimeout
        {
            get { return _pageTimeout; }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Dispose using resources
        /// </summary>
        public virtual void Dispose()
        {
            ClearSelection = null;
            ResetSelection = null;
            Timer.Tick -= CleanUpPagesOnTimerTick;
            UnsubscribeFromItemsProviderEvents();
        }

        #endregion

        #region IList<T>, IList

        #region Count

        /// <summary>
        /// Get total items count in list. 
        /// In first time request count in ItemsProvider.
        /// </summary>
        public virtual int Count
        {
            get { return _actualCount; }
            protected set
            {
                Trace.WriteLine($"New actual List Count = {value}");
                _actualCount = value;

                FirePropertyChanged("Count");
            }
        }
        private int _actualCount;

        /// <summary>
        /// Lock object for AccumulatedCount property
        /// </summary>
        private readonly ReaderWriterLock _accumulatedCountLock = new ReaderWriterLock();

        /// <summary>
        /// Lock timeout for AccumulatedCount property
        /// </summary>
        private readonly TimeSpan _accumulatedCountLockSlimTimeOut = new TimeSpan(0, 0, 3);

        /// <summary>
        /// Accumulated items count in list (without indexer)
        /// </summary>
        protected int AccumulatedCount
        {
            get
            {
                int accumulatedCount = 0;
                try
                {
                    _accumulatedCountLock.AcquireReaderLock(_accumulatedCountLockSlimTimeOut);
                    try
                    {
                        if (_accumulatedCount < 0)
                        {
                            Trace.WriteLine($"Uncorrect accumulated Count = {_accumulatedCount}");

                            try
                            {
                                var lc = _accumulatedCountLock.UpgradeToWriterLock(_accumulatedCountLockSlimTimeOut);
                                try { _accumulatedCount = 0; }
                                finally { _accumulatedCountLock.DowngradeFromWriterLock(ref lc); }
                            }
                            catch (ApplicationException)
                            {
                                Trace.WriteLine("Timeout UpgradeToWriterLock AccumulatedCount properties");
                            }
                        }
                        accumulatedCount = _accumulatedCount;
                    }
                    finally
                    {
                        _accumulatedCountLock.ReleaseReaderLock();
                    }
                }
                catch (ApplicationException)
                {
                    Trace.WriteLine("Timeout ReaderLock AccumulatedCount properties");
                }

                return accumulatedCount;
            }
            set
            {
                Trace.WriteLine($"New accumulated List Count = {value}");

                _accumulatedCountLock.AcquireWriterLock(_accumulatedCountLockSlimTimeOut);
                try { _accumulatedCount = value; }
                finally { _accumulatedCountLock.ReleaseWriterLock(); }
            }
        }
        private int _accumulatedCount;

        #endregion

        #region Indexer

        /// <summary>
        /// Indexer. Get specify item by index. 
        /// If necessary request page in ItemsProvider.
        /// </summary>
        /// <param name="index">Specify item index</param>
        /// <returns>Specify item by index</returns>
        public DataWrapper<T> this[int index]
        {
            get
            {
                // Define page index and offset inside her
                int pageIndex = index / PageSize;
                int pageOffset = index % PageSize;

                // Request page
                RequestPage(pageIndex);

                // Defensive check if async loading
                if (!_pages.ContainsKey(pageIndex)
                    || _pages[pageIndex] == null)
                {
                    Trace.WriteLine($"No page {pageIndex}. Get default DataWrapper");
                    return default(DataWrapper<T>);
                }

                if (_pages[pageIndex].Items.Count <= pageOffset)
                {
                    for (int i = _pages[pageIndex].Items.Count; i <= pageOffset; i++)
                    {
                        _pages[pageIndex].Items.Add(new DataWrapper<T>(pageIndex * PageSize + i));
                        Trace.WriteLine($"No item {i} on page {pageIndex}. Add default DataWrapper");
                    }
                }

                return _pages[pageIndex].Items[pageOffset];
            }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Indexer. Get specify item by index. 
        /// If necessary request page in ItemsProvider.
        /// </summary>
        /// <param name="index">Specify item index</param>
        /// <returns>Specify item by index</returns>
        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region IEnumerator<DataWrapper<T>>, IEnumerator

        /// <summary>
        /// Get specify enumerator for only accumulated collection items
        /// </summary>
        /// <returns>Enumerator for collection items</returns>
        public IEnumerator<DataWrapper<T>> GetEnumerator()
        {
            for (int i = 0; i < AccumulatedCount; i++)
                yield return this[i];
        }

        /// <summary>
        /// Get enumerator for collection items
        /// </summary>
        /// <returns>Enumerator for collection items</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Add

        public void Add(DataWrapper<T> item)
        {
            throw new NotSupportedException();
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Contains

        /// <summary>
        /// Contains item in list
        /// </summary>
        /// <param name="value">Finding intem</param>
        /// <returns>true - contains item in list, otherwise - false</returns>
        bool IList.Contains(object value)
        {
            return Contains((DataWrapper<T>)value);
        }

        /// <summary>
        /// Contains item in only acumulated pages
        /// </summary>
        /// <param name="item">Finding intem</param>
        /// <returns>true - contains item in list, otherwise - false</returns>
        public bool Contains(DataWrapper<T> item)
        {
            return _pages.Values.Any(page => page.Items.Contains(item));
        }

        #endregion

        #region Clear

        public void Clear()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IndexOf

        /// <summary>
        /// Get item index
        /// </summary>
        /// <param name="value">Item for which finding index</param>
        /// <returns>Item index</returns>
        int IList.IndexOf(object value)
        {
            return IndexOf((DataWrapper<T>)value);
        }

        /// <summary>
        /// Get item index in accumulated pages
        /// </summary>
        /// <param name="item">Item for which finding index</param>
        /// <returns>Item index</returns>
        public int IndexOf(DataWrapper<T> item)
        {
            if (item == null) return -1;
            foreach (var keyValuePair in _pages)
            {
                int indexWithinPage = keyValuePair.Value.Items.IndexOf(item);
                if (indexWithinPage != -1)
                    return PageSize * keyValuePair.Key + indexWithinPage;
            }
            return -1;
        }

        #endregion

        #region Insert

        public void Insert(int index, DataWrapper<T> item)
        {
            throw new NotSupportedException();
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (DataWrapper<T>)value);
        }

        #endregion

        #region Remove

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        public bool Remove(DataWrapper<T> item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region CopyTo

        public void CopyTo(DataWrapper<T>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Misc

        public object SyncRoot
        {
            get { return this; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        #endregion

        #endregion

        #region INotifyCollectionChanged

        /// <summary>
        /// Collection changed event
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Rise collection changed handler
        /// </summary>
        /// <param name="e">Collection changed information</param>
        protected virtual void OnCollectionChanged(List<NotifyCollectionChangedEventArgs> e)
        {
            Trace.WriteLine("OnCollectionChanged");

            OnClearSelection();
            
            //Update list count
            Count = AccumulatedCount;
            var h = CollectionChanged;
            if (h != null)
                foreach (var arg in e)
                    if (arg.NewStartingIndex < _actualCount
                        && arg.OldStartingIndex < _actualCount)
                        _uiThreadDispatcher.BeginInvoke(h, this, arg);

            OnResetSelection();
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Rise property changed handler
        /// </summary>
        /// <param name="e">Property changed information</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        /// <summary>
        /// Rise async property changed handler
        /// </summary>
        /// <param name="propertyName">Property changed information</param>
        protected void FirePropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(e);
        }

        #endregion

        #region Selection

        #region ISelectionManager

        /// <summary>
        /// Clear selection event. 
        /// Parameter indicate if full clear (non-reset selection)
        /// </summary>
        public event Action<bool> ClearSelection;

        /// <summary>
        /// Rise clear selection handler
        /// </summary>
        /// <param name="isFullClear">Indicate if full clear (non-reset selection)</param>
        protected void OnClearSelection(bool isFullClear = false)
        {
            if (ClearSelection != null)
                _uiThreadDispatcher.BeginInvoke(ClearSelection, isFullClear);
        }

        /// <summary>
        /// Recovery selection event
        /// </summary>
        public event Action ResetSelection;

        /// <summary>
        /// Rise recovery selection handler
        /// </summary>
        protected void OnResetSelection()
        {
            if (ResetSelection != null)
                _uiThreadDispatcher.BeginInvoke(ResetSelection);
        }

        /// <summary>
        /// Lock object for SelectedItem and SelectedIndex properties
        /// </summary>
        private readonly ReaderWriterLock _selectedItemLock = new ReaderWriterLock();

        /// <summary>
        /// Lock timeout for SelectedItem and SelectedIndex properties
        /// </summary>
        private readonly TimeSpan _selectedItemLockTimeOut = new TimeSpan(0, 0, 3);

        /// <summary>
        /// Selected item
        /// </summary>
        public object SelectedItem
        {
            get
            {
                try
                {
                    _selectedItemLock.AcquireReaderLock(_selectedItemLockTimeOut);
                    try
                    {
                        return _selectedItem;
                    }
                    finally
                    {
                        _selectedItemLock.ReleaseReaderLock();
                    }
                }
                catch (ApplicationException)
                {
                    Trace.WriteLine("Timeout ReaderLock SelectedItem properties");
                }

                return default(T);
            }
            set
            {
                if (value != null
                    && ((DataWrapper<T>)value).Data != null)
                {
                    var clonableData = ((DataWrapper<T>) value).Data as ICloneable;
                    if (clonableData != null)
                    {
                        var cloneData = (T)clonableData.Clone();
                        try
                        {
                            _selectedItemLock.AcquireWriterLock(_selectedItemLockTimeOut);
                            try
                            {
                                _selectedItem = cloneData;
                            }
                            finally
                            {
                                _selectedItemLock.ReleaseWriterLock();
                            }
                        }
                        catch (ApplicationException)
                        {
                            Trace.WriteLine("Timeout WriterLock SelectedItem properties");
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"Selected item data ({((DataWrapper<T>) value).Data.GetType()}) must be clonable (ICloneable), because it is necessary when DataWrapper changing selected item shoud not changed too");
                    }
                }
                else
                {
                    try
                    {
                        _selectedItemLock.AcquireWriterLock(_selectedItemLockTimeOut);
                        try
                        {
                            _selectedItem = default(T);
                        }
                        finally
                        {
                            _selectedItemLock.ReleaseWriterLock();
                        }
                    }
                    catch (ApplicationException)
                    {
                        Trace.WriteLine("Timeout WriterLock SelectedItem properties");
                    }
                }
            }
        }
        private T _selectedItem;

        /// <summary>
        /// Index of selected item
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                try
                {
                    _selectedItemLock.AcquireReaderLock(_selectedItemLockTimeOut);
                    try
                    {
                        return _selectedIndex;
                    }
                    finally
                    {
                        _selectedItemLock.ReleaseReaderLock();
                    }
                }
                catch (ApplicationException)
                {
                    Trace.WriteLine("Timeout ReaderLock SelectedIndex properties");
                }

                return -1;
            }
            set
            {
                try
                {
                    _selectedItemLock.AcquireWriterLock(_selectedItemLockTimeOut);
                    try
                    {
                        if (value < 0
                            && _selectedIndex >= 0)
                            Trace.WriteLine($"Unselect item: {_selectedIndex}");

                        _selectedIndex = value;
                    }
                    finally
                    {
                        _selectedItemLock.ReleaseWriterLock();
                    }
                }
                catch (ApplicationException)
                {
                    Trace.WriteLine("Timeout WriterLock SelectedIndex properties");
                }
            }
        }

        /// <summary>
        /// Select previous item
        /// </summary>
        public void PreviousSelected()
        {
            if (SelectedItem != null
                && SelectedIndex > 0)
            {
                SelectedIndex -= 1;
                OnResetSelection();
            }
        }

        /// <summary>
        /// Select next item
        /// </summary>
        public void NextSelected()
        {
            if (SelectedItem != null
                && SelectedIndex > -1
                && AccumulatedCount > SelectedIndex + 1)
            {
                SelectedIndex += 1;
                OnResetSelection();
            }
        }

        private int _selectedIndex = -1;

        #endregion

        /// <summary>
        /// Indicate when selected item retrieve in view area
        /// </summary>
        public bool IsSelectedItemVisible
        {
            get
            {
                bool isInUse = false;
                if (_selectedIndex > -1)
                {
                    if (_selectedIndex == 0)
                        //If selected item is first, check second element is view
                        isInUse = this[_selectedIndex].IsInUse && this[_selectedIndex + 1].IsInUse;
                    else if (_selectedIndex == AccumulatedCount - 1)
                        //If selected item is last, check previous element is view
                        isInUse = this[_selectedIndex].IsInUse && this[_selectedIndex - 1].IsInUse;
                    else
                        //Otherwise check previous and next elements is view
                        isInUse = this[_selectedIndex].IsInUse &&
                                  (this[_selectedIndex - 1].IsInUse || this[_selectedIndex + 1].IsInUse);
                }
                return isInUse;
            }
        }
        
        #endregion

        #region List Events Handling

        /// <summary>
        /// Subscribe to items provider changes events
        /// </summary>
        protected void SubscribeToItemsProviderEvents()
        {
            _itemsProvider.ListUpdates += OnListUpdates;
        }

        /// <summary>
        /// Unsubscribe to items provider changes events
        /// </summary>
        protected void UnsubscribeFromItemsProviderEvents()
        {
            _itemsProvider.ListUpdates -= OnListUpdates;
        }

        /// <summary>
        /// Adding item in list handler
        /// </summary>
        /// <param name="index">Index of adding item</param>
        /// <param name="newItem">New item</param>
        /// <returns>Adding information for notify collection changed event</returns>
        protected virtual NotifyCollectionChangedEventArgs AddItem(int index, T newItem)
        {
            // Calculating position for adding item
            int pageIndex = index / PageSize;
            int pageOffset = index % PageSize;

            //Delete all unused pages (which long ago not touched)
            CleanUpPages();

            try
            {
                //Create object for save shift element
                T movedItem = default(T);

                foreach (int key in _pages.Keys)
                {
                    if (_pages[key].IsInUse || key == pageIndex)
                    {
                        if (_pages[key].Items.Count < PageSize
                            && _pages[key].Items.Count <= AccumulatedCount)
                        {
                            _pages[key].Items.Add(new DataWrapper<T>(key * PageSize + _pages[key].Items.Count));

                            Trace.WriteLine($"Item Added: {index}");
                        }
                        if (key == pageIndex)
                            //If new element add to current page, save his to movedItem
                            movedItem = newItem;
                        if (key >= pageIndex)
                        {
                            //Start index for shift elements
                            int startIndex = 0;

                            //If element add to current item, shift start with adding element index in page
                            if (pageIndex == key) startIndex = pageOffset;

                            //Shift page elements
                            for (int i = startIndex; i < _pages[key].Items.Count; i++)
                            {
                                var tempItem = _pages[key].Items[i].Data;
                                _pages[key].Items[i].Data = movedItem;
                                movedItem = tempItem;
                            }
                        }
                    }
                }

                if (_pages.Count == 0)
                {
                    DataPage<T> page = new DataPage<T>(pageIndex * PageSize, 1);

                    var isAdded = _pages.TryAdd(pageIndex, page);

                    if (isAdded)
                    {
                        Trace.WriteLine($"Added page: {pageIndex}");

                        PopulatePage(pageIndex, new List<T> { newItem });
                    }
                    else
                    {
                        Trace.WriteLine($"Can't add page: {pageIndex}");
                    }
                }

                //Augment accumulated items count in collection
                AccumulatedCount++;
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine($"AddItem throw {exception}");
            }
            
            //Shift scroll box
            if (index <= SelectedIndex)
            {
                //If selected item retrieved below adding item, scroll box shift down to one element, and selected item index inc
                SelectedIndex++;

                //Check conteins previous and new selected item
                if (_pages.ContainsKey(SelectedIndex / PageSize))
                {
                    if (_pages[SelectedIndex / PageSize].Items.Count > (SelectedIndex % PageSize)
                        && (_pages[SelectedIndex / PageSize].Items[SelectedIndex % PageSize].Data == null
                            || !(_pages[SelectedIndex / PageSize].Items[SelectedIndex % PageSize].Data).Equals(SelectedItem)))
                    {
                        Trace.WriteLine("Selected Item is wrong (AddItem)");

                        //SelectedIndex = -1;
                        //SelectedItem = null;
                    }
                }
            }

            return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 
                                                        new DataWrapper<T>(index) {Data = newItem}, 
                                                        index);
        }

        /// <summary>
        /// Changing item in list handler
        /// </summary>
        /// <param name="index">Index of chenging item</param>
        /// <param name="newState">Changed item</param>
        /// <returns>Changing information for notify collection changed event</returns>
        protected virtual NotifyCollectionChangedEventArgs ChangeItem(int index, T newState)
        {
            NotifyCollectionChangedEventArgs resultChanging = null;
            
            int pageIndex = index / PageSize;
            if (_pages.ContainsKey(pageIndex)
                && (pageIndex * PageSize + _pages[pageIndex].Items.Count) > index)
            {
                int pageOffset = index % PageSize;
                if (_pages[pageIndex].Items[pageOffset].Data == null
                    || newState.Equals(_pages[pageIndex].Items[pageOffset].Data))
                {
                    var oldItem = _pages[pageIndex].Items[pageOffset].Clone() as DataWrapper<T>;
                    _pages[pageIndex].Items[pageOffset].Data = newState;

                    resultChanging = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, 
                                                                          _pages[pageIndex].Items[pageOffset], 
                                                                          oldItem, 
                                                                          index);
                    
                    Trace.WriteLine($"Item Changed: {index}");
                }
                else
                {
                    Trace.WriteLine($"Item Changed is wrong: {index}");
                    FetchPageCommand(pageIndex, PageSize);
                }
            }

            return resultChanging;
        }

        /// <summary>
        /// Removing item from list handler
        /// </summary>
        /// <param name="index">Index of removing item</param>
        /// <param name="removedItem">Removed item</param>
        /// <returns>Removing information for notify collection changed event</returns>
        protected virtual NotifyCollectionChangedEventArgs RemoveItem(int index, T removedItem)
        {
            int pageIndex = index / PageSize;
            int pageOffset = index % PageSize;

            CleanUpPages();

            try
            {
                foreach (int key in _pages.Keys)
                {
                    if (_pages[key].IsInUse && key > pageIndex || key == pageIndex)
                    {
                        int i = 0;
                        if (key == pageIndex)
                            i = pageOffset;

                        while (i < _pages[key].Items.Count - 1)
                        {
                            _pages[key].Items[i].Data = _pages[key].Items[i + 1].Data;
                            i++;
                        }

                        int movedItemIndex = key*PageSize + i;
                        if (movedItemIndex < AccumulatedCount - 1)
                        {
                            //If at removing exists elemets on next page, that add first element of next page to end this page
                            if (i < _pages[key].Items.Count)
                            {
                                _pages[key].Items[i].Data = default(T);
                            }
                            else
                                Trace.WriteLine($"FetchItem is wrong: {movedItemIndex}");
                        }
                    }
                    if (key == AccumulatedCount / PageSize
                        && _pages[key].Items.Count > 0)
                    {
                        _pages[key].Items.RemoveAt(_pages[key].Items.Count - 1);

                        Trace.WriteLine($"Item Removed: {index}");
                    }
                }

                //Reduce accumulated items count
                AccumulatedCount--;
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine($"RemoveItem throw {exception}");
            }
            
            if (SelectedIndex == index)
            {
                SelectedIndex = -1;
                SelectedItem = null;

                Trace.WriteLine($"Remove selected item: {index}");
            }
            if (index < SelectedIndex)
            {
                SelectedIndex--;
            }

            return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 
                                                        new DataWrapper<T>(index) { Data = removedItem }, 
                                                        index);
        }

        /// <summary>
        /// List changes handler block
        /// </summary>
        protected readonly ActionBlock<List<ServerListChanging<T>>> ListEventsHandlerActionBlock;

        /// <summary>
        /// Add list changes to handler block
        /// </summary>
        /// <param name="serverListChangings">List changes</param>
        protected virtual void OnListUpdates(List<ServerListChanging<T>> serverListChangings)
        {
            if (serverListChangings == null)
                return;

            if (ListEventsHandlerActionBlock.InputCount > 10)
                Trace.WriteLine($"Count changes handler > {ListEventsHandlerActionBlock.InputCount}");

            ListEventsHandlerActionBlock.Post(serverListChangings);
        }

        /// <summary>
        /// List changes handler
        /// </summary>
        /// <param name="serverListChangings">List changes</param>
        private void ListChangesHandler(List<ServerListChanging<T>> serverListChangings)
        {
            bool hasCountChanging = serverListChangings.Any(changing => changing is RequestedData<T>) 
                || serverListChangings.Where(changing => changing is ElementChanging<T>).Cast<ElementChanging<T>>()
                .Any(changing => changing.ChangeAction == CollectionChangeAction.Add
                                 || changing.ChangeAction == CollectionChangeAction.Remove);

            //Rise clear selection handler
            if (hasCountChanging)
                OnClearSelection();
            
            var aplliedChangesList = new List<NotifyCollectionChangedEventArgs>();
            for (int i = 0; i < serverListChangings.Count; i++)
            {
                if (serverListChangings[i] is RequestedData<T>)
                {
                    var requestedData = serverListChangings[i] as RequestedData<T>;

                    //If priority changes, clear list and apply it
                    if (requestedData.IsPriority)
                        ClearList();

                    int pageIndex = requestedData.StartIndex / PageSize;
                    PopulatePage(pageIndex, requestedData.RequestedDataList);
                    AccumulatedCount = requestedData.OverallCount;

                    Trace.WriteLine($"Request Data on page {pageIndex} overall count = {requestedData.OverallCount}");

                    //Undate selected item
                    if (requestedData.StartIndex <= SelectedIndex)
                    {
                        bool findUpdated = false;
                        foreach (var key in _pages.Keys)
                        {
                            for (int j = 0; j < _pages[key].Items.Count; j++)
                            {
                                if (_pages[key].Items[j].Data != null
                                    && _pages[key].Items[j].Data.Equals(SelectedItem))
                                {
                                    SelectedIndex = _pages[key].Items[j].Index;
                                    findUpdated = true;
                                    break;
                                }
                            }
                            if (findUpdated)
                                break;
                        }

                        if (!findUpdated)
                        {
                            Trace.WriteLine("Selected Item is wrong (ListChangesHandler)");
                        }
                    }

                    aplliedChangesList.Add(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
                else if (serverListChangings[i] is ElementChanging<T>)
                {
                    var elementChanging = serverListChangings[i] as ElementChanging<T>;
                    NotifyCollectionChangedEventArgs appliedChanged = null;
                    switch (elementChanging.ChangeAction)
                    {
                        case CollectionChangeAction.Add:
                            {
                                appliedChanged = AddItem(elementChanging.Position, elementChanging.ChangedElement);
                                break;
                            }
                        case CollectionChangeAction.Refresh:
                            {
                                ElementChanging<T> refreshChange = null;
                                while (refreshChange == null)
                                {
                                    if (serverListChangings.Count > i + 1
                                        && serverListChangings[i + 1] is ElementChanging<T>
                                        && ((ElementChanging<T>)serverListChangings[i + 1]).ChangeAction == CollectionChangeAction.Refresh
                                        && elementChanging.Position == ((ElementChanging<T>)serverListChangings[i + 1]).Position)
                                    {
                                        serverListChangings.RemoveAt(i);
                                    }
                                    else
                                    {
                                        refreshChange = serverListChangings[i] as ElementChanging<T>;
                                    }
                                }

                                bool isRemovedItemLate = false;
                                //Remove change if go after remove change
                                for (int j = i + 1; j < serverListChangings.Count; j++)
                                {
                                    if (serverListChangings[j] is ElementChanging<T>
                                        && ((ElementChanging<T>)serverListChangings[j]).ChangeAction == CollectionChangeAction.Remove
                                        && ((ElementChanging<T>)serverListChangings[j]).Position == refreshChange.Position)
                                    {
                                        isRemovedItemLate = true;

                                        Trace.WriteLine("Remove Change because Remove Item late");
                                        break;
                                    }
                                }

                                if (!isRemovedItemLate)
                                    appliedChanged = ChangeItem(refreshChange.Position, refreshChange.ChangedElement);

                                break;
                            }
                        case CollectionChangeAction.Remove:
                            {
                                appliedChanged = RemoveItem(elementChanging.Position, elementChanging.ChangedElement);
                                break;
                            }
                    }

                    if (appliedChanged != null)
                        aplliedChangesList.Add(appliedChanged);
                }
            }

            VerifyUnloadedPages();

            //Notify collection changed
            if (aplliedChangesList.Count > 0)
            {
                Trace.WriteLine("FireCollectionChanged - RESET");
                OnCollectionChanged(new List<NotifyCollectionChangedEventArgs>//FireCollectionChanged
                    {
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
                    });
            }

            //if (hasCountChanging)
            //    OnResetSelection();
        }

        /// <summary>
        /// Check unloaded data items at pages and load theirs
        /// </summary>
        protected void VerifyUnloadedPages()
        {
            bool mustReload = false;
            int pageIndex = -1; 
            int pageLength = 0;

            if (_pages.Count > 0)
            {
                var usedPage = _pages.Where(page => page.Value.IsInUse).FirstOrDefault(page => page.Value.Items.Any(item => Equals(item.Data, default(T))));
                if (usedPage.Value != null)
                {
                    mustReload = true;
                    pageIndex = usedPage.Key;
                    if (AccumulatedCount < PageSize && usedPage.Value.Items.Count < AccumulatedCount)
                        pageLength = AccumulatedCount;
                    else
                        pageLength = usedPage.Value.Items.Count;
                }
            }

            if (mustReload)
            {
                Trace.WriteLine("Must reload pages");
                FetchPageCommand(pageIndex, pageLength);
            }
        }

        /// <summary>
        /// Reload all pages in collection
        /// </summary>
        public virtual void ClearList()
        {
            Trace.WriteLine("Clear List");

            _pages.Clear();

            OnClearSelection(true);

            AccumulatedCount = 0;

            OnCollectionChanged(new List<NotifyCollectionChangedEventArgs> { new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset) });//FireCollectionChanged
        }

        #endregion

        #region Load methods

        /// <summary>
        /// Indicate when page loading
        /// </summary>
        protected bool IsPageLoading
        {
            get
            {
                try
                {
                    _pageLoadingLock.AcquireReaderLock(_pageLoadingLockTimeOut);
                    try { return _isPageLoading; }
                    finally { _pageLoadingLock.ReleaseReaderLock(); }
                }
                catch (ApplicationException exception)
                {
                    Trace.WriteLine(exception.ToString());
                    return false;
                }
            }
            set
            {
                try
                {
                    _pageLoadingLock.AcquireWriterLock(_pageLoadingLockTimeOut);
                    try { _isPageLoading = value; }
                    finally { _pageLoadingLock.ReleaseWriterLock(); }
                }
                catch (ApplicationException exception)
                {
                    Trace.WriteLine(exception.ToString());
                }
            }
        }
        private bool _isPageLoading;

        /// <summary>
        /// Lock object for page loading indicator
        /// </summary>
        private readonly ReaderWriterLock _pageLoadingLock = new ReaderWriterLock();

        /// <summary>
        /// Lock timeout for page loading indicator
        /// </summary>
        private readonly TimeSpan _pageLoadingLockTimeOut = new TimeSpan(0, 0, 3);

        #endregion

        #region Fetch methods

        /// <summary>
        /// Request page with data items from ItemsProvider
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageLength">Items count in page</param>
        protected void FetchPageCommand(int pageIndex, int pageLength)
        {
            ItemsProvider.FetchRangeCommand(pageIndex * PageSize, pageLength);
        }

        #endregion

        #region Sorting

        /// <summary>
        /// Current sorting parameters
        /// </summary>
        public SortParams CurrentSortParams
        {
            get { return ItemsProvider.LastSortParams; }
        }

        /// <summary>
        /// Sort list
        /// </summary>
        /// <param name="sorterParams">Sorting parameters</param>
        public void Sort(SortParams sorterParams)
        {
            ItemsProvider.Sort(sorterParams, PageSize);
        }

        #endregion

        #region Filtering

        /// <summary>
        /// Filter changed event
        /// </summary>
        public event Action FilterChanged;

        /// <summary>
        /// Rise filter changed handler
        /// </summary>
        private void OnFilterChanged()
        {
            if (FilterChanged != null)
                _uiThreadDispatcher.BeginInvoke(FilterChanged);
        }

        /// <summary>
        /// Filter list
        /// </summary>
        /// <param name="filterParams">Filtering parameters</param>
        public void Filter(FilterParams filterParams)
        {
            ItemsProvider.Filter(filterParams, PageSize);
        }

        /// <summary>
        /// Current filtering parameters
        /// </summary>
        public FilterParams CurrentFilterParams
        {
            get { return ItemsProvider.LastFilterParams; }
        }

        /// <summary>
        /// Available filters list
        /// </summary>
        public List<FilterParams> AllFilters
        {
            get { return ItemsProvider.AllFilters; }
        }

        /// <summary>
        /// Add new filter
        /// </summary>
        /// <param name="newFilterParams">New filter parameters</param>
        public void AddNewFilter(FilterParams newFilterParams)
        {
            ItemsProvider.AddNewFilter(newFilterParams);
            OnFilterChanged();
        }

        /// <summary>
        /// Update filter parameters
        /// </summary>
        /// <param name="updatedFilterParams">Updated filter parameters</param>
        public void UpdateFilter(FilterParams updatedFilterParams)
        {
            ItemsProvider.UpdateFilter(updatedFilterParams);
        }

        /// <summary>
        /// Update some filters parameters
        /// </summary>
        /// <param name="updatedFiltersParams">Updated filters parameters</param>
        public void UpdateFilters(List<FilterParams> updatedFiltersParams)
        {
            foreach (var filterParam in updatedFiltersParams)
                UpdateFilter(filterParam);
            OnFilterChanged();
        }

        /// <summary>
        /// Remove filter
        /// </summary>
        /// <param name="filterName">FIlter name</param>
        public void RemoveFilter(string filterName)
        {
            ItemsProvider.RemoveFilter(filterName);
            OnFilterChanged();
        }

        /// <summary>
        /// Get available values for expression field
        /// </summary>
        /// <param name="field">Expression field</param>
        /// <returns>Available values</returns>
        public List<object> GetExpressionAvailableValues(FieldDescription field)
        {
            return ItemsProvider.GetExpressionAvailableValues(field);
        }

        #endregion
    }
}
