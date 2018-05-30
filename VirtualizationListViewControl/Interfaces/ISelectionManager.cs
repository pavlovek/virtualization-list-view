using System;

namespace VirtualizationListViewControl.Interfaces
{
    /// <summary>
    /// Selection items at collection interface
    /// </summary>
    interface ISelectionManager
    {
        /// <summary>
        /// Clear selection event. 
        /// Parameter indicate if full clear (non-reset selection)
        /// </summary>
        event Action<bool> ClearSelection;

        /// <summary>
        /// Recovery selection event
        /// </summary>
        event Action ResetSelection;

        /// <summary>
        /// Selected item
        /// </summary>
        object SelectedItem { get; set; }

        /// <summary>
        /// Index of selected item
        /// </summary>
        int SelectedIndex { get; set; }

        /// <summary>
        /// Select previous item
        /// </summary>
        void PreviousSelected();

        /// <summary>
        /// Select next item
        /// </summary>
        void NextSelected();
    }
}
