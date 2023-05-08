using System.Globalization;
using System.Threading;
using System.Windows;
using VSTSDataProvider.Views;

namespace VSTSDataProvider;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Get the command line arguments at application startup
        string[] args = e.Args;

        // Set the language based on the command line arguments
        CurrentCultureChange(args);

        // Set the Console Display based on the command line arguments
        IsShowMainWindowWithConsole(args);

    }

    private static void CurrentCultureChange(string[] args)
    {
        //-lang en-US/zh-CN
        string langArg = "-lang";
        string langValue = "en-US"; // default en-US
        for( int i = 0; i < args.Length - 1; i++ )
        {
            if( args[i] == langArg )
            {
                string value = args[i + 1];
                try
                {
                    CultureInfo culture = CultureInfo.GetCultureInfo(value);
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                    langValue = value;
                    break;
                }
                catch( CultureNotFoundException )
                {
                    // If the language parameter is invalid, a warning dialog box will be displayed.
                    MessageBox.Show(
                        $"Invalid language code '{value}'. Using default language 'en-US'." ,
                        "Warning" ,
                        MessageBoxButton.OK ,
                        MessageBoxImage.Warning);
                }
            }
        }
    }

    private static void IsShowMainWindowWithConsole(string[] args)
    {
        //-show true/false
        bool showConsole = false;
        string showConsoleArg = "-show";

        for( int i = 0; i < args.Length - 1; i++ )
        {
            if( args[i] == showConsoleArg )
            {
                string value = args[i + 1];
                try
                {
                    bool.TryParse(value , out showConsole);
                    break;
                }
                catch( System.Exception )
                {
                    throw;
                }
            }
        }

        // Create the ViewModel and pass the command line arguments
        var viewModel = new ViewModels.MainWindowViewModel(showConsole);

        // Create the MainWindow and set the ViewModel
        var mainWindow = new MainWindow();
        mainWindow.DataContext = viewModel;
        mainWindow.Show();
    }

}

