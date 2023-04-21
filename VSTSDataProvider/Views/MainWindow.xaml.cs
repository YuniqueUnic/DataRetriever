using System.Windows;

namespace VSTSDataProvider.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow( )
    {
        InitializeComponent();
        this.DataContext = new ViewModels.MainWindowViewModel(true);
    }
}

