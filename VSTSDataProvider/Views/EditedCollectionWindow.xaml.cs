using System.ComponentModel;
using System.Windows;

namespace VSTSDataProvider.Views
{
    /// <summary>
    /// EditedCollectionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditedCollectionWindow : Window
    {
        public EditedCollectionWindow( )
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
            //base.OnClosing(e);
        }
    }
}
