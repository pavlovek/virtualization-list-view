using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SampleWpfApplication.Helpers
{
    public static class InputBindingManager
    {
        #region UpdatePropertySourceWhenEnterPressed

        public static readonly DependencyProperty UpdatePropertySourceWhenEnterPressedProperty =
            DependencyProperty.RegisterAttached("UpdatePropertySourceWhenEnterPressed", typeof (DependencyProperty),
                typeof (InputBindingManager),
                new PropertyMetadata(null, OnUpdatePropertySourceWhenEnterPressedChanged));

        public static void SetUpdatePropertySourceWhenEnterPressed(DependencyObject dp, DependencyProperty value)
        {
            dp.SetValue(UpdatePropertySourceWhenEnterPressedProperty, value);
        }

        public static DependencyProperty GetUpdatePropertySourceWhenEnterPressed(DependencyObject dp)
        {
            return (DependencyProperty) dp.GetValue(UpdatePropertySourceWhenEnterPressedProperty);
        }

        private static void OnUpdatePropertySourceWhenEnterPressedChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var element = dp as UIElement;
            if (element== null)
                return;

            if (e.OldValue != null)
            {
                element.PreviewKeyDown -= HandlePreviewKeyDown;
            }
            if (e.NewValue != null)
            {
                element.PreviewKeyDown += HandlePreviewKeyDown;
            }
        }

        private static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
                return;
            
            //DoUpdateSource(e.Source);

            var elt = e.Source as UIElement;
            if (elt != null
                && e.Key == Key.Enter)
            {
                DoUpdateSource(e.Source);

                var control = elt.GetVisualParent<Control>();
                control.Focus();
            }
        }

        private static void DoUpdateSource(object source)
        {
            var property = GetUpdatePropertySourceWhenEnterPressed(source as DependencyObject);
            if (property == null)
                return;

            var elt = source as UIElement;
            if (elt == null)
                return;

            var binding = BindingOperations.GetBindingExpression(elt, property);
            if (binding != null)
                binding.UpdateSource();
        }

        #endregion

        #region UnfocusedCommand

        public static readonly DependencyProperty UnfocusedCommandProperty =
            DependencyProperty.RegisterAttached("UnfocusedCommand", typeof(ICommand),
                typeof(InputBindingManager),
                new PropertyMetadata(null, OnUnfocusedCommandChanged));

        public static void SetUnfocusedCommand(DependencyObject dp, ICommand value)
        {
            dp.SetValue(UnfocusedCommandProperty, value);
        }

        public static ICommand GetUnfocusedCommand(DependencyObject dp)
        {
            return (ICommand)dp.GetValue(UnfocusedCommandProperty);
        }

        private static void OnUnfocusedCommandChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var element = dp as UIElement;
            if (element == null)
                return;

            if (e.OldValue != null)
                element.LostFocus -= HandleOnLostFocus;
            if (e.NewValue != null)
                element.LostFocus += HandleOnLostFocus;
        }

        private static void HandleOnLostFocus(object sender, RoutedEventArgs e)
        {
            var command = GetUnfocusedCommand(e.Source as DependencyObject);
            if (command == null)
                return;

            var elt = e.Source as UIElement;
            if (elt == null)
                return;

            var control = elt.GetVisualParent<Control>();
            if (control == null
                || control.DataContext == null)
                return;

            var prop = control.DataContext.GetType().GetProperty("Data");
            if (prop == null)
                return;

            var data = prop.GetValue(control.DataContext);

            if (command.CanExecute(data))
                command.Execute(data);
        }

        #endregion
    }
}
