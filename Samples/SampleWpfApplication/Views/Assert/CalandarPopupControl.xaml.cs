using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace SampleWpfApplication.Views.Assert
{
    /// <summary>
    /// Interaction logic for CalandarPopupControl.xaml
    /// </summary>
    public partial class CalandarPopupControl : UserControl
    {
        public CalandarPopupControl()
        {
            InitializeComponent();
        }


        void CalandarPopup_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = TargetTextBox as TextBox;

            if (textBox != null)
            {
                var dateTime = GetDate(textBox);
                if (CalandarControl.SelectedDates.Count == 0)
                    CalandarControl.SelectedDates.Add(dateTime);
                else
                    CalandarControl.SelectedDates[0] = dateTime;
                CalandarControl.DisplayDate = dateTime;

            }
            CalandarControl.SelectedDatesChanged += CalandarControlSelectedDatesChanged;
        }

        private Popup _parentPopup;

        private static DateTime GetDate(TextBox textBox)
        {
            //var culture = Thread.CurrentThread.CurrentCulture;
            DateTime dateTime;

            if (string.IsNullOrEmpty(textBox.Text)
                || !DateTime.TryParse(textBox.Text, out dateTime))
                //|| !DateTime.TryParseExact(textBox.Text, "D", culture.DateTimeFormat, DateTimeStyles.None, out dateTime))
            {
                dateTime = DateTime.Today;
            }
            return dateTime;
        }

        void CalandarControlSelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalandarControl.SelectedDate.HasValue)
            {
                var textBox = TargetTextBox as TextBox;
                var culture = Thread.CurrentThread.CurrentCulture;
                if (textBox != null)
                {
                    textBox.Text = CalandarControl.SelectedDate.Value.ToString(culture.DateTimeFormat);

                    var binding = BindingOperations.GetBindingExpression(_parentPopup.PlacementTarget, TextBox.TextProperty);
                    if (binding != null)
                        binding.UpdateSource();
                }
                IsOpen = false;
            }
        }
        

        public static readonly DependencyProperty TargetTextboxProperty =
            Popup.PlacementTargetProperty.AddOwner(typeof(CalandarPopupControl));

        public UIElement TargetTextBox
        {
            get { return (UIElement)GetValue(TargetTextboxProperty); }
            set { SetValue(TargetTextboxProperty, value); }
        }


        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State",
                                        typeof(CalandarState),
                                        typeof(CalandarPopupControl),
                                        new FrameworkPropertyMetadata(CalandarState.Normal,
                                                                      StateChanged));

        public CalandarState State
        {
            get { return (CalandarState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
        
        private static void StateChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CalandarPopupControl)element;

            if ((CalandarState)e.NewValue == CalandarState.Normal)
            {
                ctrl.IsOpen = true;
            }
            else if ((CalandarState)e.NewValue == CalandarState.Hidden)
            {
                ctrl.IsOpen = false;
            }
        }


        public static readonly DependencyProperty IsOpenProperty =
         Popup.IsOpenProperty.AddOwner(
             typeof(CalandarPopupControl),
             new FrameworkPropertyMetadata(
                 false,
                 FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                 IsOpenChanged));

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static void IsOpenChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CalandarPopupControl)element;

            if ((bool)e.NewValue)
            {
                if (ctrl._parentPopup == null)
                {
                    ctrl.HookupParentPopup();
                }
            }
        }

        private void HookupParentPopup()
        {
            _parentPopup = new Popup { AllowsTransparency = true, PopupAnimation = PopupAnimation.Scroll };

            Popup.CreateRootPopup(_parentPopup, this);
        }
    }

    public enum CalandarState
    {
        Normal,
        Hidden
    }
}
