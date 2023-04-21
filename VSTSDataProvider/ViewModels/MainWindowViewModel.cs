using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSTSDataProvider.Common.Helpers;
using VSTSDataProvider.ViewModels.ViewModelBase;

namespace VSTSDataProvider.ViewModels;

public partial class MainWindowViewModel : ViewModelBase.BaseViewModel
{
    public MainWindowViewModel( )
    {
        InitRelayCommand();
    }

    public MainWindowViewModel(Boolean showConsole)
    {
        if( showConsole ) ConsoleRelated.ConsoleEx.OpenConsole();
        InitRelayCommand();
    }

    //初始化 RelayCommand 命令
    private void InitRelayCommand( )
    {
        MainWindowLoadedCommand = new RelayCommand(MainWindowLoaded);
        GetDataButtonClickedCommand = new AsyncRelayCommand(GetVSTSDataTask , CanGetData);
        RefreshButtonClickedCommand = new RelayCommand(RefreshDataTable);
    }



    #region 用于界面 Binding 的属性 - BindingProperties

    private string? _testPlanID;
    private string? _testSuiteID;
    private string? _completeUrl;
    private string? _cookie;
    private string? _comboBoxText;
    private bool? _progressBarShowing;
    public List<string> VSTSDataCollection;


    public string? TestPlanID
    {
        get => _testPlanID ?? "TestTestPlanID";
        set
        {
            SetProperty(ref _testPlanID , value);
            GetDataButtonClickedCommand.RaiseCanExecuteChanged();
        }
    }

    public string? TestSuiteID
    {
        get => _testSuiteID ?? "TestTestSuiteID";
        set
        {
            SetProperty(ref _testSuiteID , value);
            GetDataButtonClickedCommand.RaiseCanExecuteChanged();
        }
    }

    public string CompleteUrl
    {
        get => _completeUrl ?? "";
        set
        {
            SetProperty(ref _completeUrl , value);
            GetDataButtonClickedCommand.RaiseCanExecuteChanged();
        }
    }

    public string Cookie
    {
        get => _cookie ?? "";
        set
        {
            SetProperty(ref _cookie , value);
            GetDataButtonClickedCommand.RaiseCanExecuteChanged();
        }
    }
    public string? ComboBoxText
    {
        get { return _comboBoxText ?? string.Empty; }
        set
        {
            SetProperty(ref _comboBoxText , value);
        }
    }


    public bool? ProgressShowing
    {
        get => _progressBarShowing ?? false;
        set
        {
            SetProperty(ref _progressBarShowing , value);
        }
    }



    #endregion 用于界面 Binding 的属性 - BindingProperties


    #region 用于界面 Binding 的命令 - RelayCommands
    public RelayCommand MainWindowLoadedCommand { get; set; }

    private void MainWindowLoaded( )
    {
        bool hello = DataObjectsHelper.IsDefault<string>(null);
    }


    public AsyncRelayCommand GetDataButtonClickedCommand { get; set; }

    public bool CanGetData(object p)
    {
        bool hasValidID = int.TryParse(TestPlanID , out int testPlanID) && int.TryParse(TestSuiteID , out int testSuiteID);
        bool hasValidCookie = !string.IsNullOrEmpty(Cookie);
        bool hasValidUrl = !string.IsNullOrEmpty(CompleteUrl);

        return (hasValidID && hasValidCookie) || (hasValidUrl && hasValidCookie);
    }


    private async Task GetVSTSDataTask(CancellationToken cts)
    {
        ConsoleRelated.ConsoleEx.Log("Hello World!");
        await Task.Delay(5000);
        ConsoleRelated.ConsoleEx.Log("Get 5000 done");
        Task.Delay(4000);
        ConsoleRelated.ConsoleEx.Log("Get 4000 done, New Task Run");
        Task.Run(async ( ) =>
        {
            ConsoleRelated.ConsoleEx.Log("newTask.Run 0 start");
            await Task.Delay(5000);
            ConsoleRelated.ConsoleEx.Log("newTask.Run 5000 done");
        });
        ConsoleRelated.ConsoleEx.Log("Below Task Run");
        int a = 0;
        LoadVSTSDataIntoTable(a);
    }

    private void LoadVSTSDataIntoTable(object data)
    {

    }

    public RelayCommand RefreshButtonClickedCommand { get; set; }

    private void RefreshDataTable( )
    {
        ProgressShowing = !ProgressShowing;
    }



    #endregion 用于界面 Binding 的命令 - RelayCommands

}