using System.Windows;
using System.Windows.Controls;

namespace VSTSDataProvider.Views
{
    /// <summary>
    /// LeftAlternativeItemSelector.xaml 的交互逻辑
    /// </summary>
    public partial class LeftAlternativeItemSelector : UserControl
    {
        Views.EditedCollectionWindow editedCollectionWindow;

        private volatile object _lock = new object();
        //Singlaton pattern
        public Views.EditedCollectionWindow CollectionWindow
        {
            get
            {
                if( editedCollectionWindow == null )
                {
                    lock( _lock )
                    {
                        editedCollectionWindow ??= new Views.EditedCollectionWindow();
                    }
                }
                return editedCollectionWindow;
            }
        }

        public LeftAlternativeItemSelector( )
        {
            InitializeComponent();
        }

        private void ShowDataGridButton_Click(object sender , RoutedEventArgs e)
        {
            CollectionWindow.DataContext = this.DataContext;
            CollectionWindow.WindowState = WindowState.Normal;
            CollectionWindow.Visibility = Visibility.Visible;
            CollectionWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CollectionWindow.Show();
        }
    }
}
