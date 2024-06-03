using System.Windows;

namespace VSTSDataProvider.Views
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow( )
        {
            InitializeComponent();
            this.DataContext = new ViewModels.AboutViewModel();
        }
    }
}
