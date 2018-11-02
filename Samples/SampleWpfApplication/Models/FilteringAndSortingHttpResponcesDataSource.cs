using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic;
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
    public class FilteringAndSortingHttpResponcesDataSource : IItemsProvider<HttpResponce>
    {
        private readonly List<HttpResponce> _httpResponces;

        private readonly Type _httpResponceType;
        private readonly Dictionary<Type, Type[]> _httpResponcesKnownTypes;


        public FilteringAndSortingHttpResponcesDataSource()
        {
            _httpResponces = new List<HttpResponce>();

            _httpResponceType = typeof (HttpResponce);
            _httpResponcesKnownTypes = new Dictionary<Type, Type[]>
            {
                {_httpResponceType, new[] { typeof (HttpTextResponce), typeof (HttpImageResponce), typeof (HttpVideoResponce) }}
            };

            LastSortParams = null;

            LastFilterParams = null;
            AllFilters = new List<FilterParams>();

            //Generate HTTP Responces
            _httpResponces = GenerateHttpResponces(10000);
        }

        private List<HttpResponce> GenerateHttpResponces(int count)
        {
            var result = new List<HttpResponce>();
            var rand = new Random();
            for (int i = 0; i < count; i++)
            {
                HttpResponce newResponce = null;
                switch (rand.Next(3))
                {
                    case 0:
                        {
                            newResponce = new HttpTextResponce(DateTime.Now, 
                                                               (MimeTypes) rand.Next(2), 
                                                               rand.Next(100), 
                                                               "UTF8");
                            break;
                        }
                    case 1:
                        {
                            newResponce = new HttpImageResponce(DateTime.Now, 
                                                                (MimeTypes) rand.Next(2, 4),
                                                                rand.Next(100, 1000),
                                                                new Size(480, 360),
                                                                256);
                            break;
                        }
                    case 2:
                        {
                            newResponce = new HttpVideoResponce(DateTime.Now, 
                                                                MimeTypes.Video, 
                                                                rand.Next(1000, 10000),
                                                                new Size(640, 780), 
                                                                new TimeSpan(0, rand.Next(1, 60), rand.Next(0, 60)),
                                                                "KMP");
                            break;
                        }
                }

                if (newResponce != null)
                {
                    newResponce.Id = i;
                    result.Add(newResponce);
                }
            }

            return result;
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

        protected void FetchRangeCommand(int startIndex, int pageCount, bool isPriority)
        {
            Task.Run(() =>
            {
                if (_httpResponces == null)
                    return;

                //Filter items
                List<HttpResponce> filteredList;
                if (LastFilterParams != null
                    && LastFilterParams.Conditions.Root != null)
                {
                    var filterExpression = FilterConditionConverter.GetLinqFilteringConditions(LastFilterParams.Conditions.Root,
                                                                                               _httpResponceType,
                                                                                               _httpResponcesKnownTypes);
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
                                                                                 String.Empty, 
                                                                                 out isFindProperty);
                    isAsc = LastSortParams.IsAsc;
                    if (isFindProperty)
                        sortString = sortProperty;
                }
                List<HttpResponce> sortedList;
                if (isAsc)
                    sortedList = filteredList.AsQueryable().OrderBy(x => ReflectionHelper.GetPropertyValue(x, sortString)).ToList();
                else
                    sortedList = filteredList.AsQueryable().OrderByDescending(x => ReflectionHelper.GetPropertyValue(x, sortString)).ToList();

                int realPageCount;
                if (sortedList.Count > 0
                    && sortedList.Count > startIndex + pageCount)
                {
                    realPageCount = pageCount;
                }
                else
                {
                    realPageCount = sortedList.Count - startIndex;
                }
                var array = new HttpResponce[realPageCount];
                sortedList.CopyTo(startIndex, array, 0, realPageCount);
                var requestedList = new List<HttpResponce>(array);

                var result = new RequestedData<HttpResponce>(startIndex,
                                                             realPageCount,
                                                             requestedList,
                                                             sortedList.Count,
                                                             isPriority);

                if (ListUpdates != null)
                    ListUpdates(new List<ServerListChanging<HttpResponce>> { result });
            });
        }

        #endregion

        #region IServerListCallBackBehavior

        /// <summary>
        /// List changed event
        /// </summary>
        public event Action<List<ServerListChanging<HttpResponce>>> ListUpdates;

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
