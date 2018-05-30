using System.Windows.Controls;

namespace VirtualizationListViewControl.SlaveTypes
{
    /// <summary>
    /// GridView column header with filtration field
    /// </summary>
    internal class FilterableGridViewColumnHeader : GridViewColumnHeader
    {
        /// <summary>
        /// Filtration field
        /// </summary>
        public FilterRowContainer Filter { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public FilterableGridViewColumnHeader() 
            : base()
        {
            Filter = new FilterRowContainer();
        }
    }
}
