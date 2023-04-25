using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
// using VSTSDataProvider.TestData;
using VSTSDataProvider.ViewModels.ViewModelBase;

namespace VSTSDataProvider.ViewModels;

public partial class MainWindowViewModel : ViewModelBase.BaseViewModel
{

    public string WindowTitle { get; set; } = "VSTS Data Provider";

    public MainWindowViewModel(Boolean showConsole = false)
    {
        if( showConsole ) ConsoleRelated.ConsoleEx.OpenConsole();
        InitRelayCommands();
    }

    // Init RelayCommands
    private void InitRelayCommands( )
    {
        MainWindowLoadedCommand = new RelayCommand(MainWindowLoaded);
        GetDataButtonClickedCommand = new AsyncRelayCommand(GetVSTSDataTask , CanGetData);
        RefreshButtonClickedCommand = new AsyncRelayCommand(RefreshDataTableAsync , (o) => true);
    }



    #region UI Binding - BindingProperties

    private bool _isDetailsChecked = true;

    private string? _testPlanID;
    private string? _testSuiteID;
    private string? _completeUrl;
    private string? _cookie;
    private bool? _progressBarShowing;

    private ConcurrentBag<Models.TestCase> _vstsDataCollectionTCs;
    private ConcurrentBag<Models.OTE_OfflineModel> _vstsDataCollectionOTEs;
    private ICollectionView _vstsDataCollectionViewTCs;
    private ICollectionView _vstsDataCollectionViewOTEs;
    private string _filterComboBoxText;
    private List<string> _filterCollectionsComboBox;

    public bool IsDetailsChecked
    {
        get => _isDetailsChecked;
        set => SetProperty(ref _isDetailsChecked , value);
    }

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


    public ConcurrentBag<Models.TestCase> VSTSDataCollectionTCs
    {
        get => _vstsDataCollectionTCs ?? new();
        set
        {
            #region Obsolete Code
            //if( !EqualityComparer<ConcurrentBag<Models.TestCase>>.Default.Equals(_vstsDataCollectionTCs , value) )
            //{
            //    VstsDataCollectionView = CollectionViewSource.GetDefaultView(value);
            //    // The VstsDataCollectionView filter by the text of FilterComboBoxText
            //    VstsDataCollectionView.Filter = (o) =>
            //    {
            //        if( string.IsNullOrEmpty(FilterComboBoxText) ) return true;
            //        var testCase = o as Models.TestCase;
            //        if( testCase == null ) return false;
            //        return testCase.Contains(FilterComboBoxText);
            //    };

            //    var filterSet = new HashSet<string>();

            //    foreach( var testCase in value )
            //    {
            //        filterSet.UnionWith(testCase.AllToHashSet());
            //    };

            //    var sortedFilterSet = new List<string>(filterSet);
            //    sortedFilterSet.Sort();

            //    FilterCollectionsComboBox = sortedFilterSet;
            //}
            #endregion
            RefreshComboBoxProperties<Models.TestCase>(value);
            SetProperty(ref _vstsDataCollectionTCs , value);
        }
    }

    public ConcurrentBag<Models.OTE_OfflineModel> VSTSDataCollectionOTEs
    {
        get => _vstsDataCollectionOTEs ?? new();
        set
        {
            RefreshComboBoxProperties<Models.OTE_OfflineModel>(value);
            SetProperty(ref _vstsDataCollectionOTEs , value);
        }
    }

    private bool? RefreshComboBoxProperties<T>(ConcurrentBag<T> value) where T : class, Models.IResultsModel
    {
        bool? succeedRefresh = false;
        if( !Object.Equals(IsDetailsChecked ? _vstsDataCollectionTCs : _vstsDataCollectionOTEs , value) )
        {
            if( IsDetailsChecked )
            {
                VstsDataCollectionViewTCs = CollectionViewSource.GetDefaultView(value);
                // The VstsDataCollectionView filter by the text of FilterComboBoxText
                VstsDataCollectionViewTCs.Filter = (o) =>
                {
                    if( string.IsNullOrEmpty(FilterComboBoxText) ) return true;
                    var testCase = o as T;
                    if( testCase == null ) return false;
                    return testCase.Contains(FilterComboBoxText);
                };
            }
            else
            {
                VstsDataCollectionViewOTEs = CollectionViewSource.GetDefaultView(value);
                // The VstsDataCollectionView filter by the text of FilterComboBoxText
                VstsDataCollectionViewOTEs.Filter = (o) =>
                {
                    if( string.IsNullOrEmpty(FilterComboBoxText) ) return true;
                    var testCase = o as T;
                    if( testCase == null ) return false;
                    return testCase.Contains(FilterComboBoxText);
                };
            }

            var filterSet = new HashSet<string>();

            foreach( var testCase in value )
            {
                filterSet.UnionWith(testCase.AllToHashSet());
            };

            var sortedFilterSet = new List<string>(filterSet);
            sortedFilterSet.Sort();

            FilterCollectionsComboBox = sortedFilterSet;
            succeedRefresh = true;
        }
        else
        {
            // The value has not changed.
            succeedRefresh = null;
        }
        return succeedRefresh;
    }


    //DataGrid DataCollectionView
    public ICollectionView VstsDataCollectionViewTCs
    {
        get => _vstsDataCollectionViewTCs;
        set => SetProperty(ref _vstsDataCollectionViewTCs , value);
    }

    public ICollectionView VstsDataCollectionViewOTEs
    {
        get => _vstsDataCollectionViewOTEs;
        set => SetProperty(ref _vstsDataCollectionViewOTEs , value);
    }

    //ComboBox FilterText
    public string FilterComboBoxText
    {
        get => _filterComboBoxText;
        set => SetProperty(ref _filterComboBoxText , value);
    }

    public List<string> FilterCollectionsComboBox
    {
        get => _filterCollectionsComboBox;
        set => SetProperty(ref _filterCollectionsComboBox , value);
    }

    #endregion UI Binding - BindingProperties


    #region UI Binding - RelayCommands

    public RelayCommand MainWindowLoadedCommand { get; set; }

    private void MainWindowLoaded( )
    {

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
        //await ReleaseMethod_TCs();
        if( IsDetailsChecked )
        {
            VSTSDataCollectionTCs = await DebugMethod<Models.TestCase>();
        }
        else
        {
            VSTSDataCollectionOTEs = await DebugMethod<Models.OTE_OfflineModel>();
        }

    }


    private async Task<ConcurrentBag<T>> DebugMethod<T>( ) where T : class, Models.IResultsModel
    {
        Models.ExecuteVSTSModel.RootObject exeResult;
        Models.QueryVSTSModel.RootObject queResult;

        using( var dataFile = File.OpenText(Path.GetFullPath(@"C:\Users\Administrator\source\repos\HysysToolModels\VSTSDataProvider\TestData\WithFields.json")) )
        {
            var fileData = await dataFile.ReadToEndAsync();
            exeResult = new TestData.TestVSTSClass().DeserializeBy<Models.ExecuteVSTSModel.RootObject>(fileData);
        }

        using( var dataFile = File.OpenText(Path.GetFullPath(@"C:\Users\Administrator\source\repos\HysysToolModels\VSTSDataProvider\TestData\TestPoint.json")) )
        {
            var fileData = await dataFile.ReadToEndAsync();
            queResult = new TestData.TestVSTSClass().DeserializeBy<Models.QueryVSTSModel.RootObject>(fileData);
        }

        if( typeof(T) == typeof(Models.OTE_OfflineModel) )
        {
            ConcurrentBag<Models.OTE_OfflineModel> newOTEsModel = new TestData.TestVSTSClass().MergeModelstoOTEs(exeResult , queResult , out bool succeedMergeOTEs);
            return succeedMergeOTEs ? (ConcurrentBag<T>)(object)newOTEsModel : null;
        }
        else if( typeof(T) == typeof(Models.TestCase) )
        {
            ConcurrentBag<Models.TestCase> newTCsModel = new TestData.TestVSTSClass().MergeModelstoTCs(exeResult , queResult , out bool succeedMergeTcs);
            return succeedMergeTcs ? (ConcurrentBag<T>)(object)newTCsModel : null;
        }
        else
        {
            throw new ArgumentException("Invalid type parameter T. T must be either Models.OTE_OfflineModel or Models.TestCase.");
        }
    }


    private async Task ReleaseMethod_TCs( )
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

        var succeedGET = await mVSTSDataProvider.RunGET_TCSAsync();
        ConsoleRelated.ConsoleEx.Log("End of getting VSTS Data...");

        if( succeedGET )
        {
            ConsoleRelated.ConsoleEx.Log("Start Loading VSTS Data...");
            if( IsDetailsChecked ) { VSTSDataCollectionTCs = mVSTSDataProvider.TestCases; }
            ConsoleRelated.ConsoleEx.Log("End of Loading VSTS Data...");
        }
    }



    public AsyncRelayCommand RefreshButtonClickedCommand { get; set; }


    private async Task RefreshDataTableAsync(CancellationToken arg)
    {
        if( IsDetailsChecked )
        {
            VstsDataCollectionViewTCs.Refresh();
        }
        else
        {
            VstsDataCollectionViewOTEs.Refresh();
        }
        //using( var dataFile = File.OpenText(Path.GetFullPath(@"C:\Users\Administrator\Documents\LINQPad Queries\Data\Json\NewExcuateFile.json")) )
        //{
        //    var fileData = await dataFile.ReadToEndAsync();
        //}
    }


    #endregion UI Binding - RelayCommands

}