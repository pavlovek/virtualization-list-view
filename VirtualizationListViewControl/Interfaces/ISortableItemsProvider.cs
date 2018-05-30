using VirtualizationListView.SortAndFilterDTO.Sorting;

namespace VirtualizationListViewControl.Interfaces
{
    /// <summary>
    /// Sortable ItemsProvider interface
    /// </summary>
    public interface ISortableItemsProvider
    {
        /// <summary>
        /// Sort list on ItemsProvider
        /// </summary>
        /// <param name="sorterParams">Sorting parameters</param>
        /// <param name="pageSize">Page size for requesting</param>
        void Sort(SortParams sorterParams, int pageSize);

        /// <summary>
        /// Last selected sorting
        /// </summary>
        SortParams LastSortParams { get; set; }
    }
}
