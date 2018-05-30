using System.Collections.Generic;
using VirtualizationListView.SortAndFilterDTO;
using VirtualizationListView.SortAndFilterDTO.Filtering;

namespace VirtualizationListViewControl.Interfaces
{
    /// <summary>
    /// Filterable ItemsProvider interface
    /// </summary>
    public interface IFilterableItemsProvider
    {
        /// <summary>
        /// Filter list on ItemsProvider
        /// </summary>
        /// <param name="filterParams">Filtering parameters</param>
        /// <param name="pageSize">Page size for requesting</param>
        void Filter(FilterParams filterParams, int pageSize);

        /// <summary>
        /// Last selected filter
        /// </summary>
        FilterParams LastFilterParams { get; set; }

        /// <summary>
        /// Available filters
        /// </summary>
        List<FilterParams> AllFilters { get; set; }

        /// <summary>
        /// Add new filter
        /// </summary>
        /// <param name="newFilterParams">Parameteter's new filter</param>
        void AddNewFilter(FilterParams newFilterParams);

        /// <summary>
        /// Update filter parameters
        /// </summary>
        /// <param name="updatedFilterParams">Updating parameters</param>
        void UpdateFilter(FilterParams updatedFilterParams);

        /// <summary>
        /// Remove filter from list
        /// </summary>
        /// <param name="filterName">Filter name</param>
        void RemoveFilter(string filterName);

        /// <summary>
        /// Get available values for expression field
        /// </summary>
        /// <param name="field">Expression field</param>
        /// <returns>Available values</returns>
        List<object> GetExpressionAvailableValues(FieldDescription field);
    }
}
