using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VirtualizationListViewControl.Helpers
{
    public static class BindableSelectedItemHelper
    {
        #region Properties

        public static readonly DependencyProperty SelectedItemProperty 
            = DependencyProperty.RegisterAttached("SelectedItem", 
                                                  typeof(object), 
                                                  typeof(BindableSelectedItemHelper),
                                                  new FrameworkPropertyMetadata(null, OnSelectedItemPropertyChanged));

        public static readonly DependencyProperty AttachProperty 
            = DependencyProperty.RegisterAttached("Attach", 
                                                  typeof(bool), 
                                                  typeof(BindableSelectedItemHelper), 
                                                  new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty 
            = DependencyProperty.RegisterAttached("IsUpdating", 
                                                  typeof(bool), 
                                                  typeof(BindableSelectedItemHelper));

        #endregion

        #region Implementation

        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static string GetSelectedItem(DependencyObject dp)
        {
            return (string)dp.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject dp, object value)
        {
            dp.SetValue(SelectedItemProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeListView = sender as TreeView;
            if (treeListView != null)
            {
                if ((bool)e.OldValue)
                    treeListView.SelectedItemChanged -= SelectedItemChanged;

                if ((bool)e.NewValue)
                    treeListView.SelectedItemChanged += SelectedItemChanged;
            }
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeListView = sender as TreeView;
            if (treeListView != null
                && e.NewValue != null)
            {
                treeListView.SelectedItemChanged -= SelectedItemChanged;

                if (!(bool)GetIsUpdating(treeListView))
                {
                    var item = GetTreeViewItem(treeListView, e.NewValue);
                    if (item != null)
                    {
                        item.IsSelected = true;
                    }
                }

                treeListView.SelectedItemChanged += SelectedItemChanged;
            }
        }

        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a TreeView or a TreeViewItem.
        /// </param>
        /// <param name="item">
        /// The item to search for.
        /// </param>
        /// <returns>
        /// The TreeViewItem that contains the specified item.
        /// </returns>
        private static TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container != null)
            {
                if (container.DataContext.Equals(item))
                {
                    if (container is TreeViewItem)
                        return container as TreeViewItem;
                    if (container is TreeView)
                    {
                        var treeView = container as TreeView;
                        var treeViewItem = container.ItemContainerGenerator.ContainerFromItem(treeView.Items.CurrentItem);
                        return treeViewItem as TreeViewItem;
                    }
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the 
                // virtualizing case even if the item is marked 
                // expanded we still need to do this step in order to 
                // regenerate the visuals because they may have been virtualized away.
                container.ApplyTemplate();
                var itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = container.GetVisualDescendant<ItemsPresenter>();
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();
                        itemsPresenter = container.GetVisualDescendant<ItemsPresenter>();
                    }
                }

                var itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

                // Ensure that the generator for this panel has been created.
#pragma warning disable 168
                var children = itemsHostPanel.Children;
#pragma warning restore 168

                var bringIndexIntoView = GetBringIndexIntoView(itemsHostPanel);
                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;
                    if (bringIndexIntoView != null)
                    {
                        // Bring the item into view so 
                        // that the container will be generated.
                        bringIndexIntoView(i);
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                                                    ContainerFromIndex(i);
                    }
                    else
                    {
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                                                    ContainerFromIndex(i);

                        // Bring the item into view to maintain the 
                        // same behavior as with a virtualizing panel.
                        subContainer.BringIntoView();
                    }

                    if (subContainer == null)
                    {
                        continue;
                    }

                    // Search the next level for the object.
                    var resultContainer = GetTreeViewItem(subContainer, item);
                    if (resultContainer != null)
                    {
                        return resultContainer;
                    }

                    // The object is not under this TreeViewItem
                    // so collapse it.
                    subContainer.IsExpanded = false;
                }
            }

            return null;
        }

        private static Action<int> GetBringIndexIntoView(Panel itemsHostPanel)
        {
            var virtualizingPanel = itemsHostPanel as VirtualizingStackPanel;
            if (virtualizingPanel == null)
            {
                return null;
            }

            var method = virtualizingPanel.GetType().GetMethod(
                "BringIndexIntoView",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new[] { typeof(int) },
                null);
            if (method == null)
            {
                return null;
            }

            return i => method.Invoke(virtualizingPanel, new object[] { i });
        }

        private static void SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeView treeListView = sender as TreeView;
            if (treeListView != null)
            {
                SetIsUpdating(treeListView, true);
                SetSelectedItem(treeListView, treeListView.SelectedItem);
                SetIsUpdating(treeListView, false);
            }
        }
        #endregion
    }
}
