using System;
using System.Collections.Generic;
using VirtualizationListView.SortAndFilterDTO;
using VirtualizationListView.SortAndFilterDTO.Filtering;

namespace VirtualizationListViewControl.Interfaces
{
    /// <summary>
    /// Filter for collection interface
    /// </summary>
    public interface IFilterableCollection
    {
        /// <summary>
        /// Filter changed event
        /// </summary>
        event Action FilterChanged;
        
        /// <summary>
        /// Filter list
        /// </summary>
        /// <param name="filterParams">Filtering parameters</param>
        void Filter(FilterParams filterParams);

        /// <summary>
        /// Current filtering parameters
        /// </summary>
        FilterParams CurrentFilterParams { get; }

        /// <summary>
        /// Available filters list
        /// </summary>
        List<FilterParams> AllFilters { get; }

        /// <summary>
        /// Add new filter
        /// </summary>
        /// <param name="newFilterParams">New filter parameters</param>
        void AddNewFilter(FilterParams newFilterParams);

        /// <summary>
        /// Update filter parameters
        /// </summary>
        /// <param name="updatedFilterParams">Updated filter parameters</param>
        void UpdateFilter(FilterParams updatedFilterParams);

        /// <summary>
        /// Update some filters parameters
        /// </summary>
        /// <param name="updatedFiltersParams">Updated filters parameters</param>
        void UpdateFilters(List<FilterParams> updatedFiltersParams);

        /// <summary>
        /// Remove filter
        /// </summary>
        /// <param name="filterName">FIlter name</param>
        void RemoveFilter(string filterName);

        /// <summary>
        /// Get available values for expression field
        /// </summary>
        /// <param name="field">Expression field</param>
        /// <returns>Available values</returns>
        List<object> GetExpressionAvailableValues(FieldDescription field);
    }
}
