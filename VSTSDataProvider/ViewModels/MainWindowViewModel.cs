using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using VSTSDataProvider.Common;
using VSTSDataProvider.Properties.Language;
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
        ExportCommand = new RelayCommand(Export);
        ImportCommand = new RelayCommand(Import);
        EditCommand = new RelayCommand(Edit);
        LanguageChangeCommand = new RelayCommand(LanguageChange);
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
    private string _tcsFilterComboBoxText;
    private List<string> _tcsFilterCollectionsComboBox;

    private string _otesFilterComboBoxText;
    private List<string> _otesFilterCollectionsComboBox;

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
            //    // The VstsDataCollectionView filter by the text of TCsFilterComboBoxText
            //    VstsDataCollectionView.Filter = (o) =>
            //    {
            //        if( string.IsNullOrEmpty(TCsFilterComboBoxText) ) return true;
            //        var testCase = o as Models.TestCase;
            //        if( testCase == null ) return false;
            //        return testCase.Contains(TCsFilterComboBoxText);
            //    };

            //    var filterSet = new HashSet<string>();

            //    foreach( var testCase in value )
            //    {
            //        filterSet.UnionWith(testCase.AllToHashSet());
            //    };

            //    var sortedFilterSet = new List<string>(filterSet);
            //    sortedFilterSet.Sort();

            //    TCsFilterCollectionsComboBox = sortedFilterSet;
            //}
            #endregion
            //RefreshComboBoxProperties<Models.TestCase>(value);
            var results
                = RefreshComboBoxProperties<Models.TestCase>(ref _vstsDataCollectionViewTCs , ref value , TCsFilterComboBoxText);
            if( results.succeedRefreshIt == true )
            {
                VstsDataCollectionViewTCs = results.withFilterCollectionView;
                TCsFilterCollectionsComboBox = results.withFilterTextList;

                VstsDataCollectionViewTCs.Filter = (o) =>
                {
                    if( string.IsNullOrEmpty(TCsFilterComboBoxText) ) return true;
                    var testCase = o as Models.TestCase;
                    if( testCase == null ) return false;
                    return testCase.Contains(TCsFilterComboBoxText);
                };
            }
            SetProperty(ref _vstsDataCollectionTCs , value);
        }
    }

    public ConcurrentBag<Models.OTE_OfflineModel> VSTSDataCollectionOTEs
    {
        get => _vstsDataCollectionOTEs ?? new();
        set
        {
            var results
                = RefreshComboBoxProperties<Models.OTE_OfflineModel>(ref _vstsDataCollectionViewOTEs , ref value , OTEsFilterComboBoxText);
            if( results.succeedRefreshIt == true )
            {
                VstsDataCollectionViewOTEs = results.withFilterCollectionView;
                OTEsFilterCollectionsComboBox = results.withFilterTextList;

                VstsDataCollectionViewOTEs.Filter = (o) =>
                {
                    if( string.IsNullOrEmpty(OTEsFilterComboBoxText) ) return true;
                    var testCase = o as Models.OTE_OfflineModel;
                    if( testCase == null ) return false;
                    return testCase.Contains(OTEsFilterComboBoxText);
                };
            }
            SetProperty(ref _vstsDataCollectionOTEs , value);
        }
    }


    private (bool? succeedRefreshIt, ICollectionView withFilterCollectionView, List<string> withFilterTextList) RefreshComboBoxProperties<T>
            (ref ICollectionView targetCollectionView , ref ConcurrentBag<T> value , string targetComboBoxText)
        where T : class, Models.IResultsModel
    {
        if( !Object.Equals(targetCollectionView , value) )
        {

            var newCollectionView = CollectionViewSource.GetDefaultView(value);
            // The VstsDataCollectionView filter by the text of TCsFilterComboBoxText
            //newCollectionView.Filter = (o) =>
            //{
            //    if( string.IsNullOrEmpty(targetComboBoxText) ) return true;
            //    var testCase = o as T;
            //    if( testCase == null ) return false;
            //    return testCase.Contains(targetComboBoxText);
            //};

            var filterSet = new HashSet<string>();

            foreach( var testCase in value )
            {
                filterSet.UnionWith(testCase.AllToHashSet());
            };

            var sortedFilterSet = new List<string>(filterSet);
            sortedFilterSet.Sort();

            return (true, newCollectionView, sortedFilterSet);
        }
        else
        {
            // The value has not changed.
            return (null, null, null);
        }

        return (false, null, null);
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

    //TCs ComboBox FilterText
    public string TCsFilterComboBoxText
    {
        get => _tcsFilterComboBoxText;
        set => SetProperty(ref _tcsFilterComboBoxText , value);
    }

    public List<string> TCsFilterCollectionsComboBox
    {
        get => _tcsFilterCollectionsComboBox;
        set => SetProperty(ref _tcsFilterCollectionsComboBox , value);
    }

    //OTEs ComboBox FilterText
    public string OTEsFilterComboBoxText
    {
        get => _otesFilterComboBoxText;
        set => SetProperty(ref _otesFilterComboBoxText , value);
    }

    public List<string> OTEsFilterCollectionsComboBox
    {
        get => _otesFilterCollectionsComboBox;
        set => SetProperty(ref _otesFilterCollectionsComboBox , value);
    }



    #endregion UI Binding - BindingProperties


    #region UI Binding - RelayCommands

    public RelayCommand MainWindowLoadedCommand { get; private set; }

    private void MainWindowLoaded( )
    {

    }


    public AsyncRelayCommand GetDataButtonClickedCommand { get; private set; }

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

        using( var dataFile = System.IO.File.OpenText(System.IO.Path.GetFullPath(@"C:\Users\Administrator\source\repos\HysysToolModels\VSTSDataProvider\TestData\WithFields.json")) )
        {
            var fileData = await dataFile.ReadToEndAsync();
            exeResult = new TestData.TestVSTSClass().DeserializeBy<Models.ExecuteVSTSModel.RootObject>(fileData);
        }

        using( var dataFile = System.IO.File.OpenText(System.IO.Path.GetFullPath(@"C:\Users\Administrator\source\repos\HysysToolModels\VSTSDataProvider\TestData\TestPoint.json")) )
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


    //private async Task ReleaseMethod_TCs( )
    //{
    //    ConsoleRelated.ConsoleEx.Log("Start getting VSTS Data...");

    //    VSTSDataProvider.Common.VSTSDataProcessing mVSTSDataProvider;
    //    Models.TestPlanSuiteId m_IDGroup;
    //    bool m_succeedMatch = false;

    //    if( isValidID(out m_IDGroup) )
    //    {
    //        mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId).SetCookie(Cookie);
    //    }
    //    else
    //    {
    //        m_IDGroup = VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(CompleteUrl , out m_succeedMatch);
    //        if( m_succeedMatch )
    //        {
    //            TestPlanID = m_IDGroup.PlanId.ToString();
    //            TestSuiteID = m_IDGroup.SuiteId.ToString();
    //        }

    //        mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId).SetCookie(Cookie);
    //    }

    //    var succeedLoadData = await mVSTSDataProvider.PreLoadData();

    //    ConsoleRelated.ConsoleEx.Log("End of getting VSTS Data...");

    //    if( succeedLoadData )
    //    {
    //        ConsoleRelated.ConsoleEx.Log("Start Loading VSTS Data...");

    //        if( IsDetailsChecked )
    //        {
    //            VSTSDataCollectionTCs = await mVSTSDataProvider.GET_TCsAsync();
    //        }
    //        else
    //        {
    //            VSTSDataCollectionOTEs = await mVSTSDataProvider.GET_OTEsAsync();
    //        }

    //        ConsoleRelated.ConsoleEx.Log("End of Loading VSTS Data...");
    //    }
    //}



    public AsyncRelayCommand RefreshButtonClickedCommand { get; private set; }

    private async Task RefreshDataTableAsync(CancellationToken arg)
    {
        if( IsDetailsChecked )
        {
            if( VstsDataCollectionViewTCs is null ) return;
            VstsDataCollectionViewTCs.Refresh();
        }
        else
        {
            if( VstsDataCollectionViewOTEs is null ) return;
            VstsDataCollectionViewOTEs.Refresh();
        }
        //using( var dataFile = File.OpenText(Path.GetFullPath(@"C:\Users\Administrator\Documents\LINQPad Queries\Data\Json\NewExcuateFile.json")) )
        //{
        //    var fileData = await dataFile.ReadToEndAsync();
        //}
    }


    #region Menu Functions
    public ICommand ExportCommand { get; private set; }
    public ICommand ImportCommand { get; private set; }
    public ICommand EditCommand { get; private set; }
    public ICommand LanguageChangeCommand { get; private set; }

    private async void Export( )
    {
        //         .SetSheetName(
        //"TP:" + VSTSDataCollectionTCs.First(i => i is not null).ParentTestSuite.ParentTestPlan.ID +
        // "-" +
        //"TS:" + VSTSDataCollectionTCs.First(i => i is not null).ParentTestSuite.ID
        // )
        //导出逻辑代码
        var succeedExport = await new ExcelOperator(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
            .ExportAs(VSTSDataCollectionTCs);

        //var succeedExport = await new ExcelOperator(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
        //        .SetExcelType(MiniExcelLibs.ExcelType.XLSX)
        //        .SetSheetName("HeloWorld")
        //        .Export<OTE_OfflineModel>(await DebugMethod<OTE_OfflineModel>());

    }

    private void Import( )
    {
        // 导入逻辑代码

    }

    private void Edit( )
    {
        // 编辑逻辑代码

    }
    private void LanguageChange( )
    {
        // 更改界面语言逻辑代码
        if( Resource.Language.Equals("English") )
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        }
        else
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");
        }
        void RestartApplication( )
        {
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName ,
                Arguments = string.Join(" " , Environment.GetCommandLineArgs().Skip(1)) ,
                UseShellExecute = true ,
                Verb = "runas"
            });

            System.Windows.Application.Current.Shutdown();
        }
        RestartApplication();
    }
    #endregion Menu Function



    #endregion UI Binding - RelayCommands

}