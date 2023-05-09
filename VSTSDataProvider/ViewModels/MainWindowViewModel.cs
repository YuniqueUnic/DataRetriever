using Microsoft.Win32;
using MiniExcelLibs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using VSTSDataProvider.Common;
using VSTSDataProvider.Properties.Language;
// using VSTSDataProvider.TestData;
using VSTSDataProvider.ViewModels.ViewModelBase;
using VSTSDataProvider.Views;

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
        AboutCommand = new RelayCommand(About);
    }



    #region UI Binding - BindingProperties

    private bool _isDetailsChecked = true;

    private string? _testPlanID;
    private string? _testSuiteID;
    private string? _completeUrl;
    private string? _cookie;
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

    public ConcurrentBag<Models.DetailModel> VSTSDataCollectionDetails
    {
        get => _vstsDataCollectionDetails ?? new();
        set
        {

            var results
                = RefreshComboBoxProperties<Models.DetailModel>(ref _vstsDataCollectionViewTCs , ref value , DetailsFilterComboBoxText);
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

    public ICollectionView VstsDataCollectionViewDetails
    {
        get => _vstsDataCollectionViewDetails;
        set => SetProperty(ref _vstsDataCollectionViewDetails , value);
    }

    public ICollectionView VstsDataCollectionViewOTEs
    {
        get => _vstsDataCollectionViewOTEs;
        set => SetProperty(ref _vstsDataCollectionViewOTEs , value);
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
            //VSTSDataCollectionTCs = await DebugMethod<Models.TestCase>();
            VSTSDataCollectionDetails = await DebugMethod<Models.DetailModel>();
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
        else if( typeof(T) == typeof(Models.DetailModel) )
        {
            ConcurrentBag<Models.DetailModel> newTCsModel = new TestData.TestVSTSClass().MergeModelstoDetailsBy(exeResult , queResult , out bool succeedMergeTcs);
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
    //            VSTSDataCollectionDetails = await mVSTSDataProvider.GET_DetailsAsync();
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
            //if( VstsDataCollectionViewTCs is null ) return;
            //VstsDataCollectionViewTCs.Refresh();            

            if( VstsDataCollectionViewDetails is null ) return;
            VstsDataCollectionViewDetails.Refresh();
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
    public ICommand AboutCommand { get; private set; }

    private async void Export( )
    {
        // create a SaveFileDialog Instance
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        // set the default Title and File name
        saveFileDialog.Title = Resource.SaveFileDialogTitle;
        saveFileDialog.FileName = (IsDetailsChecked ? "Detail_" : "OTE_") + $"{Guid.NewGuid()}";

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
            ExcelType excelType = ExcelOperator.ParseExcelType(fileName);

            // export as Excel
            var exportResult = await new ExcelOperator(directoryPath)
                .SetSheetName(IsDetailsChecked ? Resource.Detail : Resource.OTE)
                .setFileName(fileName)
                .SetExcelType(excelType)
                .ExportAsync(IsDetailsChecked ? VstsDataCollectionViewDetails : VstsDataCollectionViewOTEs);

            if( exportResult.SucceedDone )
            {
                // MessageBox show the successfully saving information, 
                var userSelection = MessageBox.Show(
                     $"Saved Path: {saveFileDialog.FileName}\n\n" +
                     $"Click Yes to open the directory of it." ,
                     Resource.SaveFileSuccessfully ,
                     MessageBoxButton.YesNo ,
                     MessageBoxImage.Information);

                // and if user click ok to open the directory of saved file.
                if( userSelection == MessageBoxResult.Yes )
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
                MessageBox.Show(
                    $"{Resource.SaveFileFailed}\n\n" +
                    $"Fail Reason: {exportResult.Info}" ,
                    Resource.SaveFileFailed ,
                    MessageBoxButton.OK ,
                    MessageBoxImage.Error);
            }
        }

    }

    private async void Import( )
    {
        // create an OpenFileDialog Instance
        OpenFileDialog openFileDialog = new OpenFileDialog();

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

            ExcelType excelType = ExcelOperator.ParseExcelType(fileName);

            ExcelOperatorResult importResult;
            // ImportFile
            if( IsDetailsChecked )
            {
                importResult = await new ExcelOperator(fileName , directoryPath)
                               .SetExcelType(excelType)
                               .ImportAsync<Models.DetailModel>();
            }
            else
            {
                importResult = await new ExcelOperator(fileName , directoryPath)
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

    private void Edit( )
    {
        // 编辑逻辑代码
        var EditWindow = new EditTableWindow();
        EditWindow.DataContext = this;
        EditWindow.Show();
    }

    private void LanguageChange(object param)
    {
        // 更改界面语言逻辑代码
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

    #region About Command

    //TODO: 违背了 MVVM 的原则, 以后用 behavior 重构
    private void About(object owerWindow)
    {
        var AboutWindowDialog = new AboutWindow();
        AboutWindowDialog.DataContext = new AboutViewModel();
        AboutWindowDialog.Owner = owerWindow as Window;
        AboutWindowDialog.Show();
    }

    #endregion About Command

    #endregion Menu Function



    #endregion UI Binding - RelayCommands

}