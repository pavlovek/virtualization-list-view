using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using SamplesBasicDto;
using SamplesSpecificDto;
using VirtualizationListView.SortAndFilterDTO;
using VirtualizationListView.SortAndFilterDTO.Filtering;
using VirtualizationListView.SortAndFilterDTO.Helpers;
using VirtualizationListView.SortAndFilterDTO.Sorting;
using VirtualizationListViewControl.Interfaces;
using VirtualizationListViewControl.ServerListChangesCallBack;

namespace SampleWpfApplication.Models
{
    public class ItemsChangingHttpResponceDataSource : IItemsProvider<HttpResponce>
    {
        private readonly List<KeyValuePair<long, HttpResponce>> _httpResponces;
        private int _increment = 0;

        private readonly Type _httpResponceType;
        private readonly Dictionary<Type, Type[]> _httpResponcesKnownTypes;
        private long _idCount = 0;


        public ItemsChangingHttpResponceDataSource()
        {
            _httpResponces = new List<KeyValuePair<long, HttpResponce>>();

            _httpResponceType = typeof(HttpResponce);
            _httpResponcesKnownTypes = new Dictionary<Type, Type[]>
            {
                {_httpResponceType, new[] { typeof (HttpTextResponce), typeof (HttpImageResponce), typeof (HttpVideoResponce) }}
            };

            LastSortParams = null;

            LastFilterParams = null;
            AllFilters = new List<FilterParams>();
        }
        
        #region IItemsProvider

        /// <summary>
        /// Data items rage request
        /// </summary>
        /// <param name="startIndex">Start range index</param>
        /// <param name="pageCount">Items count</param>
        public void FetchRangeCommand(int startIndex, int pageCount)
        {
            FetchRangeCommand(startIndex, pageCount, false);
        }

        /// <summary>
        /// Clear items
        /// </summary>
        public void ClearList()
        {
            _httpResponces.Clear();
            _idCount = 0;
            _increment = 0;

            FetchRangeCommand(0, 0, true);
        }

        protected List<KeyValuePair<long, HttpResponce>> SortAndFilterHttpResponcesList()
        {
            if (_httpResponces == null)
                return null;

            const string preambul = "Value";

            //Filter items
            List<KeyValuePair<long, HttpResponce>> filteredList;
            if (LastFilterParams != null
                && LastFilterParams.Conditions.Root != null)
            {
                var filterExpression = FilterConditionConverter.GetLinqFilteringConditions(LastFilterParams.Conditions.Root,
                                                                                           _httpResponceType,
                                                                                           _httpResponcesKnownTypes,
                                                                                           preambul);
                //Dynamic LINQ has constraints at filter by property only into inheritance objects
                //If use filter via SQL you have more capabilities
                filteredList = _httpResponces.AsQueryable().Where(filterExpression).ToList();
            }
            else
            {
                filteredList = _httpResponces;
            }

            //Sort items
            //Default sort by time
            var sortString = "DetectTime";
            var isAsc = false;
            if (LastSortParams != null)
            {
                bool isFindProperty;
                var sortProperty = ReflectionHelper.GetPropertyPathFromClass(LastSortParams.PropertyDescr.FieldName,
                                                                             _httpResponceType,
                                                                             _httpResponcesKnownTypes,
                                                                             preambul,
                                                                             out isFindProperty);
                isAsc = LastSortParams.IsAsc;
                if (isFindProperty)
                    sortString = sortProperty;
            }
            List<KeyValuePair<long, HttpResponce>> sortedList;
            if (isAsc)
                sortedList = filteredList.AsQueryable().OrderBy(x => ReflectionHelper.GetPropertyValue(x, sortString)).ToList();
            else
                sortedList = filteredList.AsQueryable().OrderByDescending(x => ReflectionHelper.GetPropertyValue(x, sortString)).ToList();

            return sortedList;
        }

        protected void FetchRangeCommand(int startIndex, int pageCount, bool isPriority)
        {
            Task.Run(() =>
            {
                var sortedAndFilteredList = SortAndFilterHttpResponcesList();

                int realPageCount;
                if (sortedAndFilteredList.Count > 0
                    && sortedAndFilteredList.Count > startIndex + pageCount)
                {
                    realPageCount = pageCount;
                }
                else
                {
                    realPageCount = sortedAndFilteredList.Count - startIndex;
                }
                var array = new KeyValuePair<long, HttpResponce>[realPageCount];
                sortedAndFilteredList.CopyTo(startIndex, array, 0, realPageCount);
                var requestedList = array.Select(l => l.Value).ToList();

                var result = new RequestedData<HttpResponce>(startIndex,
                                                             realPageCount,
                                                             requestedList,
                                                             sortedAndFilteredList.Count,
                                                             isPriority);

                if (ListUpdates != null)
                    ListUpdates(new List<ServerListChanging<HttpResponce>> { result });
            });
        }

        protected int GetIndexAfterSortAndFilter(long elementIdx)
        {
            var sortedAndFilteringHttpResponces = SortAndFilterHttpResponcesList();

            if (sortedAndFilteringHttpResponces.Any()
                && sortedAndFilteringHttpResponces.Exists(responce => responce.Key == elementIdx))
            {
                return sortedAndFilteringHttpResponces.FindIndex(responce => responce.Key == elementIdx);
            }

            return -1;
        }

        #endregion

        #region IServerListCallBackBehavior

        /// <summary>
        /// List changed event
        /// </summary>
        public event Action<List<ServerListChanging<HttpResponce>>> ListUpdates;

        /// <summary>
        /// One Http responces changes (random from add, remove or change)
        /// </summary>
        /// <returns>Change description</returns>
        public string RandomUpdateHttpResponce()
        {
            var updateLog = new StringBuilder(DateTime.Now.ToShortTimeString());
            var rand = new Random();

            var httpResponceChangingList = new List<ServerListChanging<HttpResponce>>();
            var position = rand.Next(_httpResponces.Count);

            switch (rand.Next(4))
            {
                case 0: // Add new Http responce
                    {
                        var currIdx = _idCount;
                        _idCount++;
                        var newHttpResponce = GetHttpResponce(_increment);
                        _httpResponces.Insert(position, new KeyValuePair<long, HttpResponce>(currIdx, newHttpResponce));
                        _increment++;

                        updateLog.Append(" ADD (pos: ");
                        updateLog.Append(position);

                        var realPosition = GetIndexAfterSortAndFilter(currIdx);

                        updateLog.Append("; r_pos: ");
                        updateLog.Append(realPosition);
                        updateLog.Append(") - ");
                        updateLog.Append(newHttpResponce.GetType().ToString());

                        if (realPosition >= 0)
                        {
                            var httpResponceChanging = new ElementChanging<HttpResponce>(realPosition,
                                                                                         newHttpResponce,
                                                                                         CollectionChangeAction.Add);
                            httpResponceChangingList.Add(httpResponceChanging);    
                        }
                        else
                        {
                            updateLog.Append(" (NOT IN VIEW) ");
                        }

                        break;
                    }
                case 1: // Add collection Http responce
                    {
                        const int count = 5;

                        updateLog.Append(" ADD LIST (pos: ");
                        updateLog.Append(position);
                        updateLog.Append(" count: ");
                        updateLog.Append(count);
                        updateLog.Append(") - ");

                        for (int i = 0; i < count; i++)
                        {
                            var currIdx = _idCount;
                            _idCount++;
                            var newHttpResponce = GetHttpResponce(_increment);
                            _httpResponces.Insert(position, new KeyValuePair<long, HttpResponce>(currIdx, newHttpResponce));
                            _increment++;

                            var realPosition = GetIndexAfterSortAndFilter(currIdx);
                            if (realPosition >= 0)
                            {
                                var httpResponceChanging = new ElementChanging<HttpResponce>(realPosition,
                                                                                             newHttpResponce,
                                                                                             CollectionChangeAction.Add);
                                httpResponceChangingList.Add(httpResponceChanging);
                            }
                            else
                            {
                                updateLog.Append(" (NOT IN VIEW) ");
                            }

                            updateLog.Append(newHttpResponce.GetType().ToString());
                            if (i != count - 1)
                                updateLog.Append(", ");
                        }

                        break;
                    }
                case 2: // Remove Http responce
                    {
                        if (_httpResponces.Count == 0)
                            break;

                        updateLog.Append(" REMOVE (");
                        updateLog.Append(position);
                        updateLog.Append(")");

                        var currIdx = _httpResponces[position].Key;
                        var realPosition = GetIndexAfterSortAndFilter(currIdx);

                        updateLog.Append("; r_pos: ");
                        updateLog.Append(realPosition);

                        if (realPosition >= 0)
                        {
                            var httpResponceChanging = new ElementChanging<HttpResponce>(realPosition,
                                                                                         null,
                                                                                         CollectionChangeAction.Remove);
                            httpResponceChangingList.Add(httpResponceChanging);
                        }
                        else
                        {
                            updateLog.Append(" (NOT IN VIEW) ");
                        }
                        
                        _httpResponces.RemoveAt(position);

                        break;
                    }
                case 3: // Change Http responce
                    {
                        if (_httpResponces.Count == 0)
                            break;

                        var newHttpResponce = GetHttpResponce(_httpResponces[position].Value.Id);
                        newHttpResponce.DetectTime = _httpResponces[position].Value.DetectTime;

                        updateLog.Append(" CHANGE (");
                        updateLog.Append(position);

                        var currIdx = _httpResponces[position].Key;
                        var realPosition = GetIndexAfterSortAndFilter(currIdx);

                        updateLog.Append("; r_pos: ");
                        updateLog.Append(realPosition);

                        if (realPosition >= 0)
                        {
                            var httpResponceChanging = new ElementChanging<HttpResponce>(realPosition,
                                                                                         newHttpResponce,
                                                                                         CollectionChangeAction.Refresh);
                            httpResponceChangingList.Add(httpResponceChanging);
                        }
                        else
                        {
                            updateLog.Append(" (NOT IN VIEW) ");
                        }

                        _httpResponces[position] = new KeyValuePair<long, HttpResponce>(_httpResponces[position].Key, newHttpResponce);
                        
                        updateLog.Append(") - ");
                        updateLog.Append(newHttpResponce.GetType().ToString());

                        break;
                    }
            }

            if (ListUpdates != null)
                ListUpdates(httpResponceChangingList);

            return updateLog.ToString();
        }

        /// <summary>
        /// Get new Http responce
        /// </summary>
        /// <param name="id">Increment Id</param>
        /// <returns>New Http responce</returns>
        private HttpResponce GetHttpResponce(int id)
        {
            var rand = new Random();
            HttpResponce newResponce = null;
            int size = rand.Next(50, 300);
            switch (rand.Next(3))
            {
                case 0: // Text Responce
                {
                    newResponce = new HttpTextResponce(DateTime.Now,
                        (MimeTypes)rand.Next(2),
                        size,
                        "Unicode");
                    break;
                }
                case 1: // Image Responce
                {
                    newResponce = new HttpImageResponce(DateTime.Now,
                        (MimeTypes)rand.Next(2, 4),
                        size,
                        new Size(rand.Next(30, 150), rand.Next(30, 150)),
                        rand.Next(1, 100));
                    break;
                }
                case 2: // Video Rasponce
                {
                    newResponce = new HttpVideoResponce(DateTime.Now,
                        MimeTypes.Video,
                        size,
                        new Size(rand.Next(30, 150), rand.Next(30, 150)),
                        new TimeSpan(rand.Next(1000, 10000)),
                        "MPEG");
                    break;
                }
            }
            if (newResponce != null)
                newResponce.Id = id;

            return newResponce;
        }

        #endregion

        #region ISortableItemsProvider

        /// <summary>
        /// Sort list on ItemsProvider
        /// </summary>
        /// <param name="sorterParams">Sorting parameters</param>
        /// <param name="pageSize">Page size for requesting</param>
        public void Sort(SortParams sorterParams, int pageSize)
        {
            LastSortParams = sorterParams;

            FetchRangeCommand(0, pageSize, true);
        }

        /// <summary>
        /// Last selected sorting
        /// </summary>
        public SortParams LastSortParams { get; set; }

        #endregion

        #region IFilterableItemsProvider

        /// <summary>
        /// Filter list on ItemsProvider
        /// </summary>
        /// <param name="filterParams">Filtering parameters</param>
        /// <param name="pageSize">Page size for requesting</param>
        public void Filter(FilterParams filterParams, int pageSize)
        {
            LastFilterParams = filterParams;

            FetchRangeCommand(0, pageSize, true);
        }

        /// <summary>
        /// Last selected filter
        /// </summary>
        public FilterParams LastFilterParams { get; set; }

        /// <summary>
        /// Available filters
        /// </summary>
        public List<FilterParams> AllFilters { get; set; }

        /// <summary>
        /// Add new filter
        /// </summary>
        /// <param name="newFilterParams">Parameteter's new filter</param>
        public void AddNewFilter(FilterParams newFilterParams)
        {
            AllFilters.Add(newFilterParams);
        }

        /// <summary>
        /// Update filter parameters
        /// </summary>
        /// <param name="updatedFilterParams">Updating parameters</param>
        public void UpdateFilter(FilterParams updatedFilterParams)
        {
            var updatingFilter = AllFilters.FirstOrDefault(f => f.Name == updatedFilterParams.Name);
            if (updatingFilter != null)
                updatingFilter.Conditions = updatedFilterParams.Conditions;
        }

        /// <summary>
        /// Remove filter from list
        /// </summary>
        /// <param name="filterName">Filter name</param>
        public void RemoveFilter(string filterName)
        {
            AllFilters.RemoveAll(f => f.Name == filterName);
        }

        /// <summary>
        /// Get available values for expression field
        /// </summary>
        /// <param name="field">Expression field</param>
        /// <returns>Available values</returns>
        public List<object> GetExpressionAvailableValues(FieldDescription field)
        {
            var fieldValues = FilterConditionConverter.GetFieldValuesLinq(field,
                                                                          _httpResponceType,
                                                                          _httpResponces.AsQueryable()).ToList();
            return fieldValues;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        { }

        #endregion
    }
}
