using System.ComponentModel;
using System.Windows;
using VSTSDataProvider.ViewModels;

namespace VSTSDataProvider.Views
{
    /// <summary>
    /// EditedCollectionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditedCollectionWindow : Window
    {
        private static EditedCollectionWindow editedCollectionWindow;

        private static volatile object _lock = new object();
        //singleton pattern
        public static EditedCollectionWindow CollectionWindow
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

        public EditedCollectionWindow( )
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
            MainWindowViewModel viewModel = DataContext as MainWindowViewModel;
            viewModel.EditDetailObCollectionForWindow = null;
            viewModel.EditingOTEObCollectionForWindow = null;
            //base.OnClosing(e);
        }
    }
}
