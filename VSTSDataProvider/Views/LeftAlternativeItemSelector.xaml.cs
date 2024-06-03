using System.Windows;
using System.Windows.Controls;

namespace VSTSDataProvider.Views
{
    /// <summary>
    /// LeftAlternativeItemSelector.xaml 的交互逻辑
    /// </summary>
    public partial class LeftAlternativeItemSelector : UserControl
    {



        public LeftAlternativeItemSelector( )
        {
            InitializeComponent();
        }

        private void ShowDataGridButton_Click(object sender , RoutedEventArgs e)
        {
            Views.EditedCollectionWindow CollectionWindow = Views.EditedCollectionWindow.CollectionWindow;
            CollectionWindow.WindowState = WindowState.Normal;
            CollectionWindow.Visibility = Visibility.Visible;
            CollectionWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CollectionWindow.DataContext = null;
            //CollectionWindow.Show();
        }
    }
}
