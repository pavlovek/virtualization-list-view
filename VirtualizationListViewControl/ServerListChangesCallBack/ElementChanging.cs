using System.ComponentModel;

namespace VirtualizationListViewControl.ServerListChangesCallBack
{
    /// <summary>
    /// Element changing at collection
    /// </summary>
    public class ElementChanging<T> : ServerListChanging<T>
    {
        /// <summary>
        /// Element position at collection
        /// </summary>
        public int Position;

        /// <summary>
        /// Changed element
        /// </summary>
        public T ChangedElement;

        /// <summary>
        /// Change type
        /// </summary>
        public CollectionChangeAction ChangeAction;


        /// <summary>
        /// Constructor for all parameters
        /// </summary>
        /// <param name="position">Element position at collection</param>
        /// <param name="changedElement">Changed element</param>
        /// <param name="changeAction">Change type</param>
        public ElementChanging(int position, T changedElement, CollectionChangeAction changeAction)
        {
            Position = position;
            ChangedElement = changedElement;
            ChangeAction = changeAction;
        }
    }
}
