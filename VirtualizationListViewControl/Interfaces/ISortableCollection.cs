using VirtualizationListView.SortAndFilterDTO.Sorting;

namespace VirtualizationListViewControl.Interfaces
{
    /// <summary>
    /// Sorting for collection interface
    /// </summary>
    public interface ISortableCollection
    {
        /// <summary>
        /// Sort list
        /// </summary>
        /// <param name="sorterParams">Sorting parameters</param>
        void Sort(SortParams sorterParams);

        /// <summary>
        /// Current sorting parameters
        /// </summary>
        SortParams CurrentSortParams { get; }
    }
}
