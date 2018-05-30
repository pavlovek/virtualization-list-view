using System.Collections.Generic;

namespace VirtualizationListViewControl.ServerListChangesCallBack
{
    /// <summary>
    /// Requested data items range
    /// </summary>
    /// <typeparam name="T">Data item object</typeparam>
    public class RequestedData<T> : ServerListChanging<T>
    {
        /// <summary>
        /// Start index of requested range
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// Requested items count
        /// </summary>
        public int RequestedCount;

        /// <summary>
        /// Reauested data items
        /// </summary>
        public IList<T> RequestedDataList;

        /// <summary>
        /// Overall items count at list
        /// </summary>
        public int OverallCount;

        /// <summary>
        /// Indicate when requested data is priority
        /// </summary>
        public bool IsPriority;


        /// <summary>
        /// Constructor for basic parameters
        /// </summary>
        /// <param name="startIndex">Start index of requested range</param>
        /// <param name="requestedCount">Requested items count</param>
        /// <param name="requestedDataList">Reauested data items</param>
        /// <param name="overallCount">Overall items count at list</param>
        public RequestedData(int startIndex, int requestedCount, IList<T> requestedDataList, int overallCount)
        {
            StartIndex = startIndex;
            RequestedCount = requestedCount;
            RequestedDataList = requestedDataList;
            OverallCount = overallCount;
        }

        /// <summary>
        /// Constructor for all parameters
        /// </summary>
        /// <param name="startIndex">Start index of requested range</param>
        /// <param name="requestedCount">Requested items count</param>
        /// <param name="requestedDataList">Reauested data items</param>
        /// <param name="overallCount">Overall items count at list</param>
        /// <param name="isPriority">Indicate when requested data is priority</param>
        public RequestedData(int startIndex, int requestedCount, IList<T> requestedDataList, int overallCount, bool isPriority)
            : this(startIndex, requestedCount, requestedDataList, overallCount)
        {
            IsPriority = isPriority;
        }
    }
}
