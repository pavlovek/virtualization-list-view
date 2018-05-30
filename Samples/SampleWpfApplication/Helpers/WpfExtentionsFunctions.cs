using System.Windows;
using System.Windows.Media;

namespace SampleWpfApplication.Helpers
{
    public static class WpfExtentionsFunctions
    {
        public static T GetVisualParent<T>(this DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            var parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return GetVisualParent<T>(parentObject);
        }
    }
}
