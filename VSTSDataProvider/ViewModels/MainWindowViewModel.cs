using MiniExcelLibs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VSTSDataProvider.Common;
using VSTSDataProvider.Common.Helpers;
using VSTSDataProvider.Properties.Language;
using VSTSDataProvider.ViewModels.ViewModelBase;
// using VSTSDataProvider.TestData;

namespace VSTSDataProvider.ViewModels;

public partial class MainWindowViewModel : ViewModelBase.BaseViewModel
{
    public string WindowTitle { get; set; } = "VSTS Data Provider";
    private bool IsCompleteUrlUpdated = false;


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
        RefreshButtonClickedCommand = new RelayCommand(RefreshDataTable , canRefresh);
        ExportCommand = new RelayCommand(Export);
        ImportCommand = new RelayCommand(Import);
        ModeSwitchCommand = new RelayCommand(ModeSwitch);
        LanguageChangeCommand = new RelayCommand(LanguageChange);
        AboutCommand = new RelayCommand(About);
        ShowStepsCommand = new RelayCommand(ShowSteps);
        InitRelayCommandsForEditingWindow();
    }



    #region UI Binding - BindingProperties

    private int _totalCount;
    private bool _isDetailsChecked = true;
    private bool _isAccessByToken = true;
    private bool _modeToggleButtonState = true;

    private string? _testPlanName;
    private string? _testSuiteName;

    private string? _testPlanID;
    private string? _testSuiteID;
    private string? _completeUrl;
    private string? _cookie;
    private string? _accessToken;
    private bool? _progressBarShowing;

    #region Obsolete TCModels

    [Obsolete("Recommend using DetailModel.")]
    private ConcurrentBag<Models.TestCase> _vstsDataCollectionTCs;

    [Obsolete("Recommend using DetailModel.")]
    private ICollectionView _vstsDataCollectionViewTCs;

    [Obsolete("Recommend using DetailModel.")]
    private string _tcsFilterComboBoxText;

    [Obsolete("Recommend using DetailModel.")]
    private List<string> _tcsFilterCollectionsComboBox;

    [Obsolete("Recommend using DetailModel.")]
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

            //    var sortedFilterHashSet = new List<string>(filterSet);
            //    sortedFilterHashSet.Sort();

            //    TCsFilterCollectionsComboBox = sortedFilterHashSet;
            //}
            #endregion
            //RefreshComboBoxProperties<Models.TestCase>(value);
            var results
                = RefreshComboBoxProperties<Models.TestCase>(ref _vstsDataCollectionViewTCs , ref value);
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

    //DataGrid DataCollectionView
    [Obsolete("Recommend using DetailModel.")]
    public ICollectionView VstsDataCollectionViewTCs
    {
        get => _vstsDataCollectionViewTCs;
        set => SetProperty(ref _vstsDataCollectionViewTCs , value);
    }

    //TCs ComboBox FilterText
    [Obsolete("Recommend using DetailModel.")]
    public string TCsFilterComboBoxText
    {
        get => _tcsFilterComboBoxText;
        set => SetProperty(ref _tcsFilterComboBoxText , value);
    }

    [Obsolete("Recommend using DetailModel.")]
    public List<string> TCsFilterCollectionsComboBox
    {
        get => _tcsFilterCollectionsComboBox;
        set => SetProperty(ref _tcsFilterCollectionsComboBox , value);
    }

    #endregion Obsolete TCModels

    private ConcurrentBag<Models.DetailModel> _vstsDataCollectionDetails;
    private ConcurrentBag<Models.OTE_OfflineModel> _vstsDataCollectionOTEs;
    private ICollectionView _vstsDataCollectionViewDetails;
    private ICollectionView _vstsDataCollectionViewOTEs;

    private string _detailsFilterComboBoxText;
    private List<string> _detailsFilterCollectionsComboBox;

    private string _otesFilterComboBoxText;
    private List<string> _otesFilterCollectionsComboBox;

    public int TotalCount
    {
        get { return _totalCount; }
        set
        {
            int tmpCount = 0;
            if( IsDetailsChecked )
            {
                tmpCount = VSTSDataCollectionDetails.Count;
            }
            else
            {
                tmpCount = VSTSDataCollectionOTEs.Count;
            }
            SetProperty(ref _totalCount , tmpCount);
        }
    }

    public bool IsDetailsChecked
    {
        get => _isDetailsChecked;
        set
        {
            SetProperty(ref _isDetailsChecked , value);
            RefreshButtonClickedCommand.RaiseCanExecuteChanged();

            TotalCount = 0;
        }
    }

    public bool IsAccessByToken
    {
        get => _isAccessByToken;
        set
        {
            SetProperty(ref _isAccessByToken , value);
            GetDataButtonClickedCommand.RaiseCanExecuteChanged();
        }
    }

    public bool ModeToggleButtonState
    {
        get => _modeToggleButtonState;
        set => SetProperty(ref _modeToggleButtonState , value);
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

    public string? TestPlanName
    {
        get => _testPlanName ?? "TestPlanName";
        set
        {
            SetProperty(ref _testPlanName , value);
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

    public string? TestSuiteName
    {
        get => _testSuiteName ?? "TestSuiteName";
        set
        {
            SetProperty(ref _testSuiteName , value);
        }
    }

    public string CompleteUrl
    {
        get => _completeUrl ?? "";
        set
        {

            if( !EqualityComparer<string>.Default.Equals(_completeUrl , value) )
            {
                IsCompleteUrlUpdated = true;
            }

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

    public string AccessToken
    {
        get => _accessToken ?? "";
        set
        {
            SetProperty(ref _accessToken , value);
            GetDataButtonClickedCommand.RaiseCanExecuteChanged();
        }
    }

    public bool? ProgressShowing
    {
        get => _progressBarShowing ?? false;
        set => SetProperty(ref _progressBarShowing , value);
    }

    public ConcurrentBag<Models.DetailModel> VSTSDataCollectionDetails
    {
        get => _vstsDataCollectionDetails ?? new();
        set
        {

            var results
                = RefreshComboBoxProperties<Models.DetailModel>(ref _vstsDataCollectionViewDetails , ref value);

            if( results.succeedRefreshIt == true )
            {
                VstsDataCollectionViewDetails = results.withFilterCollectionView;
                DetailsFilterCollectionsComboBox = results.withFilterTextList;

                VstsDataCollectionViewDetails.Filter = (o) =>
                {
                    if( string.IsNullOrEmpty(DetailsFilterComboBoxText) ) return true;
                    var testCase = o as Models.DetailModel;
                    if( testCase == null ) return false;
                    return testCase.Contains(DetailsFilterComboBoxText);
                };

            }
            SetProperty(ref _vstsDataCollectionDetails , value);
            //just RaisePropertyChange
            TotalCount = 0;
        }
    }

    public ConcurrentBag<Models.OTE_OfflineModel> VSTSDataCollectionOTEs
    {
        get => _vstsDataCollectionOTEs ?? new();
        set
        {
            var results
                = RefreshComboBoxProperties<Models.OTE_OfflineModel>(ref _vstsDataCollectionViewOTEs , ref value);

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
            //just RaisePropertyChange
            TotalCount = 0;
        }
    }

    private (bool? succeedRefreshIt, ICollectionView withFilterCollectionView, List<string> withFilterTextList) RefreshComboBoxProperties<T>
            (ref ICollectionView targetCollectionView , ref ConcurrentBag<T> value)
        where T : class, Models.IResultsModel
    {
        if( !Object.Equals(targetCollectionView , value) )
        {

            var newCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(value);

            var filterSet = new HashSet<string>();

            foreach( var testCase in value )
            {
                filterSet.UnionWith(testCase.AllToHashSet());
            };

            var sortedFilterHashSet = new List<string>(filterSet);
            sortedFilterHashSet.Sort();

            return (true, newCollectionView, sortedFilterHashSet);
        }
        else
        {
            // The value has not changed.
            return (null, null, null);
        }

        return (false, null, null);
    }

    public ICollectionView VstsDataCollectionViewDetails
    {
        get => _vstsDataCollectionViewDetails;
        set
        {
            SetProperty(ref _vstsDataCollectionViewDetails , value);
            RefreshButtonClickedCommand.RaiseCanExecuteChanged();
        }
    }

    public ICollectionView VstsDataCollectionViewOTEs
    {
        get => _vstsDataCollectionViewOTEs;
        set
        {
            SetProperty(ref _vstsDataCollectionViewOTEs , value);
            RefreshButtonClickedCommand.RaiseCanExecuteChanged();
        }
    }

    //Details ComboBox FilterText
    public string DetailsFilterComboBoxText
    {
        get => _detailsFilterComboBoxText;
        set => SetProperty(ref _detailsFilterComboBoxText , value);
    }

    public List<string> DetailsFilterCollectionsComboBox
    {
        get => _detailsFilterCollectionsComboBox;
        set => SetProperty(ref _detailsFilterCollectionsComboBox , value);
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
    public AsyncRelayCommand GetDataButtonClickedCommand { get; private set; }
    public RelayCommand RefreshButtonClickedCommand { get; private set; }

    private void MainWindowLoaded( ) { }

    #region Get Data Async

    public bool CanGetData(object p)
    {
        bool hasValidID = isValidID(out _);
        bool hasValidCookieOrToken = IsAccessByToken ? !string.IsNullOrEmpty(AccessToken) : !string.IsNullOrEmpty(Cookie);
        bool hasValidUrl = !string.IsNullOrEmpty(CompleteUrl);

        return (hasValidID && hasValidCookieOrToken) || (hasValidUrl && hasValidCookieOrToken);
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
        if( IsDetailsChecked )
        {
            //VSTSDataCollectionTCs = await DebugMethod<Models.TestCase>();
            VSTSDataCollectionDetails = await DebugMethod<Models.DetailModel>();
            EditDetailsCollection = await DebugMethod<Models.DetailModel>();
        }
        else
        {
            VSTSDataCollectionOTEs = await DebugMethod<Models.OTE_OfflineModel>();
            EditOTEsCollection = await DebugMethod<Models.OTE_OfflineModel>();
        }

    }

    private async Task<ConcurrentBag<T>> DebugMethod<T>( ) where T : class, Models.IResultsModel
    {
        Models.ExecuteVSTSModel.RootObject exeResult;
        Models.QueryVSTSModel.RootObject queResult;


        using( var dataFile = System.IO.File.OpenText(System.IO.Path.GetFullPath(@"C:\Users\Administrator\source\repos\HysysToolModels\VSTSDataProvider\TestData\StepWithDetail.json")) )
        {
            var fileData = await dataFile.ReadToEndAsync();
            exeResult = new TestData.TestVSTSClass().DeserializeBy<Models.ExecuteVSTSModel.RootObject>(fileData);
        }

        using( var dataFile = System.IO.File.OpenText(System.IO.Path.GetFullPath(@"C:\Users\Administrator\source\repos\HysysToolModels\VSTSDataProvider\TestData\StepWithOTE.json")) )
        {
            var fileData = await dataFile.ReadToEndAsync();
            queResult = new TestData.TestVSTSClass().DeserializeBy<Models.QueryVSTSModel.RootObject>(fileData);
        }

        TestPlanName = exeResult.value[0].testPlan.name;
        TestSuiteName = queResult.value[0].testSuite.name;

        if( typeof(T) == typeof(Models.OTE_OfflineModel) )
        {
            ConcurrentBag<Models.OTE_OfflineModel> newOTEsModel = new Common.VSTSDataProcessing().MergeModelstoOTEsBy(exeResult , queResult , out bool succeedMergeOTEs);
            return succeedMergeOTEs ? (ConcurrentBag<T>)(object)newOTEsModel : null;
        }
        else if( typeof(T) == typeof(Models.TestCase) )
        {
            ConcurrentBag<Models.TestCase> newTCsModel = new Common.VSTSDataProcessing().MergeModelstoTCsBy(exeResult , queResult , out bool succeedMergeTcs);
            return succeedMergeTcs ? (ConcurrentBag<T>)(object)newTCsModel : null;
        }
        else if( typeof(T) == typeof(Models.DetailModel) )
        {
            ConcurrentBag<Models.DetailModel> newTCsModel = new Common.VSTSDataProcessing().MergeModelstoDetailsBy(exeResult , queResult , out bool succeedMergeTcs);
            return succeedMergeTcs ? (ConcurrentBag<T>)(object)newTCsModel : null;
        }
        else
        {
            throw new ArgumentException("Invalid type parameter T. T must be either Models.OTE_OfflineModel or Models.TestCase.");
        }
    }

    private async Task ReleaseMethod( )
    {
        ConsoleRelated.ConsoleEx.Log("Start getting VSTS Data...");

        var mVSTSDataProvider = GetVSTSDataProvider();
        mVSTSDataProvider.UsingTokenToGET = true;
        var succeedLoadData = await mVSTSDataProvider.PreLoadData();

        ConsoleRelated.ConsoleEx.Log("End of getting VSTS Data...");

        if( succeedLoadData )
        {
            ConsoleRelated.ConsoleEx.Log("Start Loading VSTS Data...");

            if( IsDetailsChecked )
            {
                VSTSDataCollectionDetails = await mVSTSDataProvider.GET_DetailsAsync();
                EditDetailsCollection = await mVSTSDataProvider.MergeLocalModelsAgainAsync<Models.DetailModel>();
            }
            else
            {
                VSTSDataCollectionOTEs = await mVSTSDataProvider.GET_OTEsAsync();
                EditOTEsCollection = await mVSTSDataProvider.MergeLocalModelsAgainAsync<Models.OTE_OfflineModel>();
            }

            TestPlanName = mVSTSDataProvider.TestPlan.Name;
            TestSuiteName = mVSTSDataProvider.TestSuite.Name;

            ConsoleRelated.ConsoleEx.Log("End of Loading VSTS Data...");
        }
    }

    public VSTSDataProvider.Common.VSTSDataProcessing GetVSTSDataProvider( )
    {
        VSTSDataProvider.Common.VSTSDataProcessing mVSTSDataProvider;
        Models.TestPlanSuiteId m_IDGroup;
        bool m_succeedMatch = false;

        if( IsCompleteUrlUpdated )
        {
            m_IDGroup = VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(CompleteUrl , out m_succeedMatch);
            if( m_succeedMatch )
            {
                TestPlanID = m_IDGroup.PlanId.ToString();
                TestSuiteID = m_IDGroup.SuiteId.ToString();
            }

            mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId);
            mVSTSDataProvider = IsAccessByToken ? mVSTSDataProvider.SetToken(AccessToken) : mVSTSDataProvider.SetCookie(Cookie);
        }
        else
        {
            if( isValidID(out m_IDGroup) )
            {
                mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId);
                mVSTSDataProvider = IsAccessByToken ? mVSTSDataProvider.SetToken(AccessToken) : mVSTSDataProvider.SetCookie(Cookie);
            }
            else
            {
                m_IDGroup = VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(CompleteUrl , out m_succeedMatch);
                if( m_succeedMatch )
                {
                    TestPlanID = m_IDGroup.PlanId.ToString();
                    TestSuiteID = m_IDGroup.SuiteId.ToString();
                }

                mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId);
                mVSTSDataProvider = IsAccessByToken ? mVSTSDataProvider.SetToken(AccessToken) : mVSTSDataProvider.SetCookie(Cookie);
            }
        }

        IsCompleteUrlUpdated = false;
        return mVSTSDataProvider;
    }


    #endregion Get Data Async

    private bool canRefresh(object obj)
    {
        if( IsDetailsChecked )
        {
            if( VstsDataCollectionViewDetails is null ) return false;
            return true;
        }
        else
        {
            if( VstsDataCollectionViewOTEs is null ) return false;
            return true;
        }
    }

    private void RefreshDataTable(object param)
    {
        if( IsDetailsChecked )
        {
            VstsDataCollectionViewDetails.Refresh();
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


    #region MainMenu Functions

    public ICommand ExportCommand { get; private set; }
    public ICommand ImportCommand { get; private set; }
    public ICommand ModeSwitchCommand { get; private set; }
    public ICommand LanguageChangeCommand { get; private set; }
    public ICommand AboutCommand { get; private set; }
    public ICommand ShowStepsCommand { get; private set; }

    private async void Export( )
    {
        // create a SaveFileDialog Instance
        Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

        // set the default Title and File name
        saveFileDialog.Title = Resource.SaveFileDialogTitle;
        saveFileDialog.FileName = (IsDetailsChecked ? "Detail_" : "OTE_") + $"{TestSuiteName ?? Guid.NewGuid().ToString()}";

        // set the file Filter
        saveFileDialog.Filter = "Excel (*.xlsx)|*.xlsx|CSV (*.csv)|*.csv|All (*.*)|*.*";

        // show the dialog and get the result
        bool? result = saveFileDialog.ShowDialog();

        if( result == true )
        {
            // get the selected or input file name after clicking the save button
            string fileName = saveFileDialog.SafeFileName;
            string directoryPath = saveFileDialog.FileName.Replace(fileName , "");

            // According to the file extension to determine the ExcelType
            ExcelType excelType = Common.ExcelOperator.ParseExcelType(fileName);

            // export as Excel
            var exportResult = await new Common.ExcelOperator(directoryPath)
                .SetSheetName(IsDetailsChecked ? Resource.Detail : Resource.OTE)
                .setFileName(fileName)
                .SetExcelType(excelType)
                .ExportAsync(IsDetailsChecked ?
                ModeToggleButtonState ? VstsDataCollectionViewDetails : EditDetailsCollectionView
                : ModeToggleButtonState ? VstsDataCollectionViewOTEs : EditOTEsCollectionView);

            if( exportResult.SucceedDone )
            {
                // Modify some string to ideal result
                await Task.Yield();

                var modifyRules = new ExcelModifyRule[]
                {
                    new()
                    {
                        ColumnName="Outcome",
                        ModifyRule=(result)=>
                        {if( string.Equals(result.ToString() , "Active" , StringComparison.OrdinalIgnoreCase) )
                          {
                              return string.Empty;
                          }
                          else
                          {
                              return result;
                          } }
                    },
                    new()
                    {
                        ColumnName="TestTool",
                        ModifyRule=(result)=>
                        {
                            if( string.Equals(result.ToString() , "Null" , StringComparison.OrdinalIgnoreCase) )
                            {
                                return string.Empty;
                            }
                            else if(string.Equals(result.ToString(),"UFT", StringComparison.OrdinalIgnoreCase))
                            {
                                return "UFT Developer";
                            }
                            else
                            {
                                return result;
                            } }
                    },
                };

                var modifyResult = await ExcelOperator.ModifyColumn(saveFileDialog.FileName , modifyRules);

                if( modifyResult.SucceedDone )
                {
                    // MessageBox show the successfully saving information, 
                    var userSelection = System.Windows.MessageBox.Show(
                         $"Saved Path: {saveFileDialog.FileName}\n\n" +
                         $"Click Yes to open the directory of it." ,
                         Resource.SaveFileSuccessfully ,
                         System.Windows.MessageBoxButton.YesNo ,
                         System.Windows.MessageBoxImage.Information);

                    // and if user click ok to open the directory of saved file.
                    if( userSelection == System.Windows.MessageBoxResult.Yes )
                    {
                        try
                        {
                            Process.Start("explorer.exe" , $"/select,\"{exportResult.FullPath}\"");
                        }
                        catch( Exception ex )
                        {
                            // exception dealing
                        }
                    }

                }
                else
                {
                    System.Windows.MessageBox.Show(
                        $"{Resource.SaveFileFailed}\n\n" +
                        $"Fail Reason: {exportResult.Info}" ,
                        Resource.SaveFileFailed ,
                        System.Windows.MessageBoxButton.OK ,
                        System.Windows.MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    $"{Resource.SaveFileFailed}\n\n" +
                    $"Fail Reason: {exportResult.Info}" ,
                    Resource.SaveFileFailed ,
                    System.Windows.MessageBoxButton.OK ,
                    System.Windows.MessageBoxImage.Error);
            }
        }

    }

    private async void Import( )
    {
        // create an OpenFileDialog Instance
        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

        // set the default Title and File name
        openFileDialog.Title = Resource.OpenFileDialogTitle;
        openFileDialog.FileName = "";

        // set the file Filter
        openFileDialog.Filter = "Excel (*.xlsx)|*.xlsx|CSV (*.csv)|*.csv|All (*.*)|*.*";

        // show the dialog and get the result
        bool? result = openFileDialog.ShowDialog();

        if( result == true )
        {
            // get the selected or input file name after clicking the save button
            string fileName = openFileDialog.SafeFileName;
            string directoryPath = openFileDialog.FileName.Replace(fileName , "");
            string fileExtension = Path.GetExtension(fileName);

            ExcelType excelType = Common.ExcelOperator.ParseExcelType(fileName);

            Common.ExcelOperatorResult importResult;
            // ImportFile
            if( IsDetailsChecked )
            {
                importResult = await new Common.ExcelOperator(fileName , directoryPath)
                               .SetExcelType(excelType)
                               .ImportAsync<Models.DetailModel>();
            }
            else
            {
                importResult = await new Common.ExcelOperator(fileName , directoryPath)
                               .SetExcelType(excelType)
                               .ImportAsync<Models.OTE_OfflineModel>();
            }


            if( importResult.SucceedDone == true )
            {
                if( IsDetailsChecked )
                {
                    VSTSDataCollectionDetails = new ConcurrentBag<Models.DetailModel>((IEnumerable<Models.DetailModel>)importResult.resultModels);
                }
                else
                {
                    VSTSDataCollectionOTEs = new ConcurrentBag<Models.OTE_OfflineModel>((IEnumerable<Models.OTE_OfflineModel>)importResult.resultModels);
                }
            }

        }


    }

    private void ModeSwitch( ) => ModeToggleButtonState = !ModeToggleButtonState;

    private void LanguageChange(object param)
    {
        // ���Ľ��������߼�����
        if( param.Equals("English") )
        {
            RestartApplication("en-US");
        }
        else
        {
            RestartApplication("zh-CN");
        }

        void RestartApplication(string culture)
        {
            string fileName = Process.GetCurrentProcess().MainModule.FileName;

            string[] args = new[] {
                "-lang",culture,
            };

            Process.Start(new ProcessStartInfo
            {
                FileName = fileName ,
                Arguments = string.Join(" " , args) ,
                UseShellExecute = false ,
                Verb = "runas"
            });

            System.Windows.Application.Current.Shutdown();
        }
    }

    //TODO: Violated the MVVM design pattern and will be replaced with behavior in the future.
    private void About(object owerWindow)
    {
        var AboutWindow = new VSTSDataProvider.Views.AboutWindow();
        AboutWindow.Owner = owerWindow as System.Windows.Window;
        AboutWindow.Show();
    }

    private void ShowSteps(object param)
    {
        var stepsWindow = new VSTSDataProvider.Views.StepsWindow();
        stepsWindow.DataContext = param as Models.DetailModel;
        stepsWindow.Show();
    }

    #endregion MainMenu Function


    #region Edit Page

    private void InitRelayCommandsForEditingWindow( )
    {
        SaveEditingItemCommand = new RelayCommand(SaveEditingItem);
        ShowEditedCollectionViewCommand = new RelayCommand(ShowEditedCollectionView);
        ResetLeftBoxContentToInitCommand = new RelayCommand(ResetLeftBoxContentToInit);
        CancelEditCommand = new RelayCommand(CancelEdit);
    }

    public IOrderedEnumerable<System.Windows.Media.FontFamily> FontFamiliesList { get; private set; } = System.Windows.Media.Fonts.SystemFontFamilies.OrderBy(f => f.Source);
    public List<Int32> FontSizeList { get; private set; } = new List<Int32>() { 2 , 4 , 6 , 8 , 10 , 12 , 14 , 16 , 18 , 20 , 22 , 24 , 26 , 28 , 32 , 48 , 72 };

    private string _leftEditTextBoxTitle;
    private string _rightEditRichTextBoxTitle;

    private string _leftEditTextBoxDocument;
    private string _rightEditRichTextBoxDocument;


    private ConcurrentBag<Models.DetailModel> _editDetailsCollection;
    private ConcurrentBag<Models.OTE_OfflineModel> _editOTEsCollection;
    private ICollectionView _editDetailsCollectionView;
    private ICollectionView _editOTEsCollectionView;

    private string _editDetailsFilterComboBoxText;
    private List<string> _editDetailsFilterCollectionsComboBox;

    private string _editOTEsFilterComboBoxText;
    private List<string> _editOTEsFilterCollectionsComboBox;


    public string LeftEditTextBoxTitle
    {
        get => _leftEditTextBoxTitle;
        set
        {
            if( !EqualityComparer<string>.Default.Equals(_leftEditTextBoxTitle , value) )
            {
                IsAutoFillLeftTextBox = true;
            }

            SetProperty(ref _leftEditTextBoxTitle , value);
        }
    }

    public string RightEditRichTextBoxTitle
    {
        get => _rightEditRichTextBoxTitle;
        set => SetProperty(ref _rightEditRichTextBoxTitle , value);
    }

    private bool IsAutoFillLeftTextBox = true;
    private string LeftEditTextBoxDocumentBackUp = string.Empty;
    public string LeftEditTextBoxDocument
    {
        get => _leftEditTextBoxDocument;
        set
        {
            SetProperty(ref _leftEditTextBoxDocument , value);

            if( IsAutoFillLeftTextBox )
            {
                LeftEditTextBoxDocumentBackUp = value;
                IsAutoFillLeftTextBox = false;
            }
        }
    }

    public string RightEditRichTextBoxDocument
    {
        get => _rightEditRichTextBoxDocument;
        set
        {
            SetProperty(ref _rightEditRichTextBoxDocument , value);
        }
    }

    public ConcurrentBag<Models.DetailModel> EditDetailsCollection
    {
        get => _editDetailsCollection ?? new();
        set
        {
            if( !EqualityComparer<ConcurrentBag<Models.DetailModel>>.Default.Equals(_editDetailsCollection , value) )
            {
                EditDetailsCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(value);

                var filterSet = new HashSet<string>();

                foreach( var testCase in value )
                {
                    filterSet.Add(testCase.ID.ToString());
                    filterSet.Add(testCase.Name ?? string.Empty);
                };

                var sortedFilterHashSet = new List<string>(filterSet);
                sortedFilterHashSet.Sort();

                EditDetailsFilterCollectionsComboBox = sortedFilterHashSet;

                EditDetailsCollectionView.Filter = (o) =>
                {
                    if( string.IsNullOrEmpty(EditDetailsFilterComboBoxText) ) return true;
                    var testCase = o as Models.DetailModel;
                    if( testCase == null ) return false;
                    return testCase.Contains(EditDetailsFilterComboBoxText);
                };

            }

            SetProperty(ref _editDetailsCollection , value);
        }
    }

    public ConcurrentBag<Models.OTE_OfflineModel> EditOTEsCollection
    {
        get => _editOTEsCollection ?? new();
        set
        {
            if( !EqualityComparer<ConcurrentBag<Models.OTE_OfflineModel>>.Default.Equals(_editOTEsCollection , value) )
            {
                EditOTEsCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(value);

                var filterSet = new HashSet<string>();

                foreach( var testCase in value )
                {
                    filterSet.Add(testCase.TestCaseId.ToString());
                    filterSet.Add(testCase.Title ?? string.Empty);
                };

                var sortedFilterHashSet = new List<string>(filterSet);
                sortedFilterHashSet.Sort();

                EditOTEsFilterCollectionsComboBox = sortedFilterHashSet;

                EditOTEsCollectionView.Filter = (o) =>
                {
                    if( string.IsNullOrEmpty(EditOTEsFilterComboBoxText) ) return true;
                    var testCase = o as Models.OTE_OfflineModel;
                    if( testCase == null ) return false;
                    return testCase.Contains(EditOTEsFilterComboBoxText);
                };

            }

            SetProperty(ref _editOTEsCollection , value);
        }
    }

    public ICollectionView EditDetailsCollectionView
    {
        get => _editDetailsCollectionView;
        set => SetProperty(ref _editDetailsCollectionView , value);
    }

    public ICollectionView EditOTEsCollectionView
    {
        get => _editOTEsCollectionView;
        set => SetProperty(ref _editOTEsCollectionView , value);
    }

    // Edit Details ComboBox FilterText
    public string EditDetailsFilterComboBoxText
    {
        get => _editDetailsFilterComboBoxText;
        set => SetProperty(ref _editDetailsFilterComboBoxText , value);
    }

    public List<string> EditDetailsFilterCollectionsComboBox
    {
        get => _editDetailsFilterCollectionsComboBox;
        set => SetProperty(ref _editDetailsFilterCollectionsComboBox , value);
    }

    // Edit OTEs ComboBox FilterText
    public string EditOTEsFilterComboBoxText
    {
        get => _editOTEsFilterComboBoxText;
        set => SetProperty(ref _editOTEsFilterComboBoxText , value);
    }

    public List<string> EditOTEsFilterCollectionsComboBox
    {
        get => _editOTEsFilterCollectionsComboBox;
        set => SetProperty(ref _editOTEsFilterCollectionsComboBox , value);
    }

    public ObservableCollection<Models.DetailModel> EditingDetailObCollection { get; set; } = new ObservableCollection<Models.DetailModel>();

    public ObservableCollection<Models.OTE_OfflineModel> EditingOTEObCollection { get; set; } = new ObservableCollection<Models.OTE_OfflineModel>();

    public ObservableCollection<Models.DetailModel> EditDetailObCollectionForWindow { get; set; }

    public ObservableCollection<Models.OTE_OfflineModel> EditingOTEObCollectionForWindow { get; set; }


    public ICommand ShowEditedCollectionViewCommand { get; private set; }
    public ICommand SaveEditingItemCommand { get; private set; }
    public ICommand ResetLeftBoxContentToInitCommand { get; private set; }
    public ICommand CancelEditCommand { get; private set; }



    public void ShowEditedCollectionView( )
    {
        Views.EditedCollectionWindow CollectionWindow = Views.EditedCollectionWindow.CollectionWindow;

        if( IsDetailsChecked )
        {
            EditDetailObCollectionForWindow = new ObservableCollection<Models.DetailModel>(EditDetailsCollection);
        }
        else
        {
            EditingOTEObCollectionForWindow = new ObservableCollection<Models.OTE_OfflineModel>(EditOTEsCollection);
        }
        CollectionWindow.DataContext = this;
        CollectionWindow.Show();
    }

    //TODO: Add a command to save the edited item
    private void SaveEditingItem( )
    {
        if( LeftEditTextBoxTitle.IsNullOrWhiteSpaceOrEmpty() ) { return; }

        int testcaseID = -1;
        string testcaseName = string.Empty;
        bool success = false;

        if( IsDetailsChecked )
        {
            testcaseID = EditingDetailObCollection.First().ID;
            testcaseName = EditingDetailObCollection.First().Name;
            success = EditingDetailObCollection.First().SetPropertyValue(LeftEditTextBoxTitle , LeftEditTextBoxDocument.TrimEnd());
            EditingDetailObCollection.Clear();
            EditingDetailObCollection.Add(EditDetailsCollection.First(i => i.ID == testcaseID || i.Name == testcaseName));
            EditDetailsCollectionView.Refresh();
        }
        else
        {
            testcaseID = EditingOTEObCollection.First().TestCaseId;
            testcaseName = EditingOTEObCollection.First().Title;
            success = EditingOTEObCollection.First().SetPropertyValue(LeftEditTextBoxTitle , LeftEditTextBoxDocument.TrimEnd());
            EditingOTEObCollection.Clear();
            EditingOTEObCollection.Add(EditOTEsCollection.First(i => i.TestCaseId == testcaseID || i.Title == testcaseName));
            EditOTEsCollectionView.Refresh();
        }

        if( success ) { LeftEditTextBoxDocumentBackUp = LeftEditTextBoxDocument; }

    }

    private void ResetLeftBoxContentToInit( ) { LeftEditTextBoxDocument = LeftEditTextBoxDocumentBackUp; }

    private void CancelEdit( )
    {
        LeftEditTextBoxDocumentBackUp = string.Empty;
        LeftEditTextBoxDocument = string.Empty;
        LeftEditTextBoxTitle = string.Empty;
        if( IsDetailsChecked )
        {
            EditingDetailObCollection.Clear();
        }
        else
        {
            EditingOTEObCollection.Clear();
        }
    }

    #endregion Edit Page

    #endregion UI Binding - RelayCommands

}