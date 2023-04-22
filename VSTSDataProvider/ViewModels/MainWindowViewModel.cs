using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        RefreshButtonClickedCommand = new AsyncRelayCommand(RefreshDataTableAsync , (o) => true);
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

    public AsyncRelayCommand RefreshButtonClickedCommand { get; set; }

    private async Task RefreshDataTableAsync(CancellationToken arg)
    {
        using( var dataFile = File.OpenText(Path.GetFullPath(@"C:\Users\Administrator\Documents\LINQPad Queries\Data\Json\NewExcuateFile.json")) )
        {
            var fileData = await dataFile.ReadToEndAsync();
            int a = 0;
            var responseJson = JsonConvert.DeserializeObject<string>(fileData);

            int vb = 0;
        }
    }



    #endregion 用于界面 Binding 的命令 - RelayCommands

}