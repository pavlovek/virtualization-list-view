using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtualizationListViewControl.Collection
{
    /// <summary>
    /// Definition page with data items
    /// </summary>
    /// <typeparam name="T">Data item object</typeparam>
    public class DataPage<T>
    {
        /// <summary>
        /// Page's data items
        /// </summary>
        public IList<DataWrapper<T>> Items { get; set; }

        /// <summary>
        /// Time when page touched
        /// </summary>
        public DateTime TouchTime { get; set; }

        /// <summary>
        /// Indicate when page now in view state
        /// </summary>
        public bool IsInUse
        {
            get { return Items.Any(wrapper => wrapper.IsInUse); }
        }


        /// <summary>
        /// Constructor with inizialise new items
        /// </summary>
        /// <param name="firstIndex">Index of first item in page</param>
        /// <param name="pageLength">Count items in page</param>
        public DataPage(int firstIndex, int pageLength)
        {
            Items = new List<DataWrapper<T>>(pageLength);
            for (int i = 0; i < pageLength; i++)
            {
                Items.Add(new DataWrapper<T>(firstIndex + i));
            }
            TouchTime = DateTime.Now;
        }

        /// <summary>
        /// Inizialise page items
        /// </summary>
        /// <param name="newItems">Items with data</param>
        public void Populate(IList<T> newItems)
        {
            if (newItems == null)
                return;
            
            int i;
            int index = 0;
            for (i = 0; i < newItems.Count && i < Items.Count; i++)
            {
                Items[i].Data = newItems[i];
                index = Items[i].Index;
            }
            while (i < newItems.Count)
            {
                index++;
                Items.Add(new DataWrapper<T>(index) {Data = newItems[i]});
                i++;
            }
            while (i < Items.Count)
            {
                Items.RemoveAt(Items.Count-1);
            }
        }
    }
}
