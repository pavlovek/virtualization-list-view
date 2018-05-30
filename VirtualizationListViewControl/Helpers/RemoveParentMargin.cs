using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VirtualizationListViewControl.Helpers
{
    public class RemoveParentMargin : Decorator
    {
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            FrameworkElement fe = VisualTreeHelper.GetParent(this) as FrameworkElement;

            if (fe != null)
            {
                fe.Margin = new Thickness(0.0);
                fe.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }
    }
}
