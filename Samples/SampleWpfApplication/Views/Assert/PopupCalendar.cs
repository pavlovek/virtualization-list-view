using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SampleWpfApplication.Views.Assert
{
    public static class PopupCalendar
    {
        private static CalandarPopupControl _popupCalControl;


        public static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached("State",
                                                typeof(CalandarState),
                                                typeof(PopupCalendar),
                                                new FrameworkPropertyMetadata(CalandarState.Normal,
                                                                              OnStateChanged));

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static CalandarState GetState(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (CalandarState)element.GetValue(StateProperty);
        }

        public static void SetState(DependencyObject element, CalandarState value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(StateProperty, value);
        }
        
        private static void OnStateChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = element as FrameworkElement;

            if ((frameworkElement != null) && (_popupCalControl != null))
            {
                _popupCalControl.State = GetState(frameworkElement);
            }
        }


        public static readonly DependencyProperty TargetTextBox =
           DependencyProperty.RegisterAttached("TargetTextBox",
                                               typeof(UIElement),
                                               typeof(PopupCalendar),
                                               new FrameworkPropertyMetadata(null,
                                                                             OnPlacementTargetChanged));

        private static void OnPlacementTargetChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = element as FrameworkElement;

            if (_popupCalControl == null)
            {
                _popupCalControl = new CalandarPopupControl();

                _popupCalControl.AddHandler(UIElement.LostKeyboardFocusEvent,
                                            new KeyboardFocusChangedEventHandler(PopupCalendarLostKeyboardFocus));
            }
            if (frameworkElement != null 
                && _popupCalControl != null)
            {
                _popupCalControl.TargetTextBox = GetTargetTextBox(frameworkElement);
                frameworkElement.AddHandler(UIElement.GotKeyboardFocusEvent,
                                            new KeyboardFocusChangedEventHandler(FrameworkElementGotKeyboardFocus),
                                            true);
                frameworkElement.AddHandler(UIElement.LostKeyboardFocusEvent,
                                            new KeyboardFocusChangedEventHandler(FrameworkElementLostKeyboardFocus));
            }
        }

        private static void FrameworkElementGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;

            if (frameworkElement != null)
            {
                if (_popupCalControl == null)
                {
                    _popupCalControl = new CalandarPopupControl();

                    _popupCalControl.AddHandler(UIElement.LostKeyboardFocusEvent,
                                                new KeyboardFocusChangedEventHandler(PopupCalendarLostKeyboardFocus));
                }

                _popupCalControl.TargetTextBox = GetTargetTextBox(frameworkElement);
                _popupCalControl.IsOpen = true;
            }
        }

        private static void PopupCalendarLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_popupCalControl != null
                && !(e.NewFocus is CalendarDayButton)
                && !(e.NewFocus is CalendarButton))
            {
                _popupCalControl.IsOpen = false;
                _popupCalControl = null;
            }
        }

        private static void FrameworkElementLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;

            if (frameworkElement != null)
            {
                if (_popupCalControl != null
                    && !(e.NewFocus is CalendarDayButton)
                    && !(e.NewFocus is CalendarButton))
                {
                    // Retrieves the setting for the State property
                    SetState(frameworkElement, _popupCalControl.State);

                    _popupCalControl.IsOpen = false;
                    _popupCalControl = null;
                }
            }
        }


        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static UIElement GetTargetTextBox(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (UIElement)element.GetValue(TargetTextBox);
        }

        public static void SetTargetTextBox(DependencyObject element, UIElement value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(TargetTextBox, value);
        }

    }
}
