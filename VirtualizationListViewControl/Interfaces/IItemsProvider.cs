namespace VirtualizationListViewControl.Interfaces
{
    /// <summary>
    /// Items provider interface
    /// </summary>
    /// <typeparam name="T">Data item object</typeparam>
    public interface IItemsProvider<T> : IServerListCallBackBehavior<T>, ISortableItemsProvider, IFilterableItemsProvider
    {        
        /// <summary>
        /// Data items rage request
        /// </summary>
        /// <param name="startIndex">Start range index</param>
        /// <param name="pageCount">Items count</param>
        void FetchRangeCommand(int startIndex, int pageCount);
    }
}
