using System.Windows;
using System.Windows.Controls;
using SampleWpfApplication.ViewModels;

namespace SampleWpfApplication.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ThisViewModel
        {
            get { return DataContext as MainWindowViewModel; }
        }
        

        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            
            InitializeComponent();
        }


        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = e.OriginalSource as TabControl;
            if (tabControl == null)
                return;

            switch (tabControl.SelectedIndex)
            {
                case 0:
                {
                    ThisViewModel.InicializedSimpleDataSource();
                    break;
                }
                case 1:
                {
                    ThisViewModel.InicializedFilteringAndSortingDataSource();
                    break;
                }
                case 2:
                {
                    ThisViewModel.InicializedItemsChangingDataSource();
                    break;
                }
            }
        }
    }
}
