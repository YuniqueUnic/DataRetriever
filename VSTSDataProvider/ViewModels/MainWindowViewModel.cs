using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using VSTSDataProvider.Models;
using VSTSDataProvider.TestData;
using VSTSDataProvider.ViewModels.ViewModelBase;

namespace VSTSDataProvider.ViewModels;

public partial class MainWindowViewModel : ViewModelBase.BaseViewModel
{

    public string WindowTitle { get; set; } = "VSTS Data Provider";

    public MainWindowViewModel(Boolean showConsole = false)
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

    public HashSet<string> _filterCollectionsComboBox;
    private ConcurrentBag<Models.TestCase> _vstsDataCollection;
    private ICollectionView _vstsDataCollectionView;
    private string _filterComboBoxText;

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

    public bool? ProgressShowing
    {
        get => _progressBarShowing ?? false;
        set
        {
            SetProperty(ref _progressBarShowing , value);
        }
    }


    public HashSet<string> FilterCollectionsComboBox
    {
        get => _filterCollectionsComboBox;
        set => SetProperty(ref _filterCollectionsComboBox , value);
    }

    public ConcurrentBag<Models.TestCase> VSTSDataCollection
    {
        get => _vstsDataCollection ?? new();
        set
        {
            SetProperty(ref _vstsDataCollection , value);
            // put the all value of ConcurrentBag<Models.TestCase> into HashSet<string> FilterCollectionsComboBox
            //if( !EqualityComparer<ConcurrentBag<Models.TestCase>>.Default.Equals(_vstsDataCollection , value) )
            //{
            //    var filterSet = new HashSet<string>();
            //    Parallel.ForEach(value , testCase =>
            //    {
            //        filterSet.UnionWith(testCase.AllToHashSet());
            //    });
            //    FilterCollectionsComboBox = filterSet;
            //}

        }
    }

    //DataGrid DataCollectionView
    public ICollectionView VstsDataCollectionView
    {
        get => _vstsDataCollectionView;
        set => SetProperty(ref _vstsDataCollectionView , value);
    }

    //ComboBox FilterText
    public string FilterComboBoxText
    {
        get => _filterComboBoxText;
        set => SetProperty(ref _filterComboBoxText , value);
    }


    #endregion 用于界面 Binding 的属性 - BindingProperties


    #region 用于界面 Binding 的命令 - RelayCommands

    public RelayCommand MainWindowLoadedCommand { get; set; }

    private void MainWindowLoaded( )
    {
        VstsDataCollectionView = CollectionViewSource.GetDefaultView(VSTSDataCollection);
        // The VstsDataCollectionView filter by the text of FilterComboBoxText
        VstsDataCollectionView.Filter = (o) =>
        {
            if( string.IsNullOrEmpty(FilterComboBoxText) ) return true;
            var testCase = o as Models.TestCase;
            if( testCase == null ) return false;
            return testCase.Contains(FilterComboBoxText);
        };
    }


    public AsyncRelayCommand GetDataButtonClickedCommand { get; set; }

    public bool CanGetData(object p)
    {
        bool hasValidID = isValidID(out _);
        bool hasValidCookie = !string.IsNullOrEmpty(Cookie);
        bool hasValidUrl = !string.IsNullOrEmpty(CompleteUrl);

        return (hasValidID && hasValidCookie) || (hasValidUrl && hasValidCookie);
    }

    private bool isValidID(out Models.TestPlanSuiteId idGroup)
    {
        int m_testPlanID, m_testSuiteID = -1;
        idGroup = new Models.TestPlanSuiteId(-1 , -1);

        bool hasValidID = int.TryParse(TestPlanID , out m_testPlanID) && int.TryParse(TestSuiteID , out m_testSuiteID);
        if( hasValidID ) idGroup = new Models.TestPlanSuiteId(m_testPlanID , m_testSuiteID);

        return hasValidID;
    }

    private async Task GetVSTSDataTask(CancellationToken cts)
    {
        //await ReleaseMethod();
        VSTSDataCollection = await DebugMethod();
    }


    private async Task<ConcurrentBag<TestCase>> DebugMethod( )
    {
        ExecuteVSTSModel.RootObject exeResult;
        QueryVSTSModel.RootObject queResult;
        using( var dataFile = File.OpenText(Path.GetFullPath(@"C:\Users\Administrator\source\repos\HysysToolModels\VSTSDataProvider\TestData\WithFields.json")) )
        {
            var fileData = await dataFile.ReadToEndAsync();
            exeResult = new TestVSTSClass().DeserializeBy<ExecuteVSTSModel.RootObject>(fileData);
        }

        using( var dataFile = File.OpenText(Path.GetFullPath(@"C:\Users\Administrator\source\repos\HysysToolModels\VSTSDataProvider\TestData\TestPoint.json")) )
        {
            var fileData = await dataFile.ReadToEndAsync();
            queResult = new TestVSTSClass().DeserializeBy<QueryVSTSModel.RootObject>(fileData);
        }

        ConcurrentBag<TestCase> newModel = new TestVSTSClass().MergeModels(exeResult , queResult , out bool succeedMerge);

        if( succeedMerge ) { return newModel; }
        else { return null; }
    }


    private async Task ReleaseMethod( )
    {
        ConsoleRelated.ConsoleEx.Log("Start getting VSTS Data...");
        // await Task.Delay(5000);
        // ConsoleRelated.ConsoleEx.Log("Get 5000 done");
        // Task.Delay(4000);
        // ConsoleRelated.ConsoleEx.Log("Get 4000 done, New Task Run");
        // Task.Run(async ( ) =>
        // {
        //     ConsoleRelated.ConsoleEx.Log("newTask.Run 0 start");
        //     await Task.Delay(5000);
        //     ConsoleRelated.ConsoleEx.Log("newTask.Run 5000 done");
        // });
        // ConsoleRelated.ConsoleEx.Log("Below Task Run");
        VSTSDataProvider.Common.VSTSDataProcessing mVSTSDataProvider;
        Models.TestPlanSuiteId m_IDGroup;
        bool m_succeedMatch = false;

        if( isValidID(out m_IDGroup) )
        {
            mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId).SetCookie(Cookie);
        }
        else
        {
            m_IDGroup = VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(CompleteUrl , out m_succeedMatch);
            if( m_succeedMatch )
            {
                TestPlanID = m_IDGroup.PlanId.ToString();
                TestSuiteID = m_IDGroup.SuiteId.ToString();
            }

            mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId).SetCookie(Cookie);
        }

        var succeedGET = await mVSTSDataProvider.RunAsync();
        ConsoleRelated.ConsoleEx.Log("End of getting VSTS Data...");

        if( succeedGET )
        {
            ConsoleRelated.ConsoleEx.Log("Start Loading VSTS Data...");
            VSTSDataCollection = mVSTSDataProvider.TestCases;
            ConsoleRelated.ConsoleEx.Log("End of Loading VSTS Data...");
        }
    }



    public AsyncRelayCommand RefreshButtonClickedCommand { get; set; }

    private async Task RefreshDataTableAsync(CancellationToken arg)
    {
        VstsDataCollectionView.Refresh();
        //using( var dataFile = File.OpenText(Path.GetFullPath(@"C:\Users\Administrator\Documents\LINQPad Queries\Data\Json\NewExcuateFile.json")) )
        //{
        //    var fileData = await dataFile.ReadToEndAsync();
        //}
    }


    #endregion 用于界面 Binding 的命令 - RelayCommands

}