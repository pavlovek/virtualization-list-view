using System;
using System.Collections.Generic;
using System.Diagnostics;
using SamplesBasicDto;
using SamplesSpecificDto;
using VirtualizationListView.SortAndFilterDTO;
using VirtualizationListView.SortAndFilterDTO.Filtering;
using VirtualizationListView.SortAndFilterDTO.Sorting;
using VirtualizationListViewControl.Interfaces;
using VirtualizationListViewControl.ServerListChangesCallBack;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using SampleWpfApplication.Helpers;

namespace SampleWpfApplication.Models
{
    public class SimpleHttpResponcesDataSource : IItemsProvider<HttpResponce>
    {
        public SimpleHttpResponcesDataSource()
        {
#if GENERATE_DATA
            //Generate HTTP Responces
            GenerateHttpResponces(10000);
#endif
        }

        private void GenerateHttpResponces(int count)
        {
            try
            {
                var rand = new Random();
                using (var db = new HttpResponcesContext())
                {
                    if (db.HttpResponces.Any())
                    {
#if !RESET_DATA
                        return;
#endif
                    }
                    
                    //Clear old data
                    db.HttpResponces.Clear();
                    db.SaveChanges();

                    for (int i = 0; i < count; i++)
                    {
                        HttpResponce newResponce = null;
                        switch (rand.Next(3))
                        {
                            case 0:
                            {
                                newResponce = new HttpTextResponce(DateTime.Now,
                                    (MimeTypes)rand.Next(2),
                                    rand.Next(100),
                                    "UTF8");
                                break;
                            }
                            case 1:
                            {
                                newResponce = new HttpImageResponce(DateTime.Now,
                                    (MimeTypes)rand.Next(2, 4),
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
                            db.HttpResponces.Add(newResponce);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
            }
        }

        
        #region IItemsProvider

        /// <summary>
        /// Data items rage request
        /// </summary>
        /// <param name="startIndex">Start range index</param>
        /// <param name="pageCount">Items count</param>
        public void FetchRangeCommand(int startIndex, int pageCount)
        {
            Task.Run(() =>
            {
                RequestedData<HttpResponce> result;
                using (var db = new HttpResponcesContext())
                {
                    int realPageCount;
                    var httpResponcesCount = db.HttpResponces.Count();
                    if (httpResponcesCount < startIndex + pageCount)
                    {
                        realPageCount = pageCount;
                    }
                    else
                    {
                        realPageCount = httpResponcesCount - startIndex;
                    }

                    var selectedResponces = db.HttpResponces.OrderBy(k => k.Id).Skip(startIndex).Take(pageCount);
                    var requestedList = new List<HttpResponce>(selectedResponces);

                    result = new RequestedData<HttpResponce>(startIndex,
                        realPageCount,
                        requestedList,
                        httpResponcesCount);
                }

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Last selected sorting
        /// </summary>
        public SortParams LastSortParams
        {
            get { return null; }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region IFilterableItemsProvider

        /// <summary>
        /// Filter list on ItemsProvider
        /// </summary>
        /// <param name="filterParams">Filtering parameters</param>
        /// <param name="pageSize">Page size for requesting</param>
        public void Filter(FilterParams filterParams, int pageSize)
        { }

        /// <summary>
        /// Last selected filter
        /// </summary>
        public FilterParams LastFilterParams
        {
            get { return null; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Available filters
        /// </summary>
        public List<FilterParams> AllFilters
        {
            get { return new List<FilterParams>(); }
            set { throw new NotImplementedException(); }
        }
        
        /// <summary>
        /// Add new filter
        /// </summary>
        /// <param name="newFilterParams">Parameteter's new filter</param>
        public void AddNewFilter(FilterParams newFilterParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update filter parameters
        /// </summary>
        /// <param name="updatedFilterParams">Updating parameters</param>
        public void UpdateFilter(FilterParams updatedFilterParams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove filter from list
        /// </summary>
        /// <param name="filterName">Filter name</param>
        public void RemoveFilter(string filterName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get available values for expression field
        /// </summary>
        /// <param name="field">Expression field</param>
        /// <returns>Available values</returns>
        public List<object> GetExpressionAvailableValues(FieldDescription field)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        { }

        #endregion
    }
}
