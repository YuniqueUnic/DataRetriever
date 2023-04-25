using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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

        bool showConsole = false;
        
        if (e.Args.Length > 0 && bool.TryParse(e.Args[0], out bool argValue))
        {
            showConsole = argValue;
        }

        //创建ViewModel并传递命令行参数
        var viewModel = new ViewModels.MainWindowViewModel(showConsole);

        //创建MainWindow并设置ViewModel
        var mainWindow = new MainWindow();
        mainWindow.DataContext = viewModel;
        mainWindow.Show();
    }
    }

