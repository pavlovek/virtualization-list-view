using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace VirtualizationListViewControl.Controls
{
    internal class FocusableListView : ListView
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is FocusableListViewItem);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new FocusableListViewItem();
        }
    }

    internal class FocusableListViewItem : ListViewItem
    {
        private ListView ParentListView
        {
            get { return (ItemsControl.ItemsControlFromItemContainer(this) as ListView); }
        }

        
        public FocusableListViewItem()
        {
            GotFocus += FocusableListViewItem_GotFocus;
        }


        private void FocusableListViewItem_GotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Content != BindingOperations.DisconnectedSource)
            {
                var obj = ParentListView.ItemContainerGenerator.ItemFromContainer(this);
                ParentListView.SelectedItem = obj;
            }
        }
    }
}
