using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VSTSDataProvider.Models;

namespace VSTSDataProvider.Common;

public class VSTSDataProcessing : ViewModels.ViewModelBase.BaseViewModel
{
    private Models.QueryVSTSModel.RootObject _queryRootObject;
    private Models.ExecuteVSTSModel.RootObject _exeRootObject;

    private string? _cookie;
    private int _testPlanID;
    private int _testSuiteID;
    private int _totalCount;

    private TestPlan? _testPlan;
    private TestSuite? _testSuite;
    private ConcurrentBag<TestCase> _testCases = new();
    private ConcurrentBag<OTE_OfflineModel> _otesOfflineModel = new();

    private bool _testCasesLoadOver = false;

    public string? Cooike => _cookie;
    public int TestSuiteID => _testSuiteID;
    public int TestPlanID => _testPlanID;
    public int TotalCount => _totalCount;

    public TestPlan TestPlan
    {
        get => _testPlan;
        private set => SetProperty(ref _testPlan , value);
    }

    public TestSuite TestSuite
    {
        get => _testSuite;
        private set => SetProperty(ref _testSuite , value);
    }

    public ConcurrentBag<TestCase> TestCasesModel
    {
        get => _testCases;
        private set { SetProperty(ref _testCases , value); }
    }

    public ConcurrentBag<OTE_OfflineModel> OTEs_OfflineModel
    {
        get => _otesOfflineModel;
        private set => SetProperty(ref _otesOfflineModel , value);
    }

    public bool IsTestCasesLoadOver
    {
        get => _testCasesLoadOver;
        private set { SetProperty(ref _testCasesLoadOver , value); }
    }

    public VSTSDataProcessing SetCookie(string cookie)
    {
        _cookie = cookie;
        return this;
    }

    public VSTSDataProcessing ParseUriToId(string uri)
    {
        var idStruct = TryGetTestPlanSuiteId(uri , out bool succeedMatch);

        if( !succeedMatch ) { throw new System.ArgumentException($"Failed to parse {uri} to TestPlanSuiteID"); }

        this._testPlanID = idStruct.PlanId;
        this._testSuiteID = idStruct.SuiteId;
        return this;
    }

    public VSTSDataProcessing SetTestPlanSuiteID(int planID , int suiteID)
    {
        _testPlanID = planID;
        _testSuiteID = suiteID;
        return this;
    }

    public bool SucceedLoadData() => CheckModels(_exeRootObject, _queryRootObject);

    public async Task<bool> PreLoadData(System.Action action =null)
    {
        var executeVSTSModel = await new ExecuteVSTSModel(_cookie , _testPlanID , _testSuiteID).GetModel(action);
        var queryVSTSModel = await new QueryVSTSModel(_cookie , _testPlanID , _testSuiteID).GetModel(action);

        if( CheckModels(executeVSTSModel,queryVSTSModel) )
        {
            _queryRootObject = queryVSTSModel;
            _exeRootObject = executeVSTSModel;
            return true;
        }

        return false;
    }

    //TODO to show the concat progress
    public async Task<ConcurrentBag<Models.TestCase>> GET_TCsAsync()
    {
        IsTestCasesLoadOver = false;

        if (!CheckModels(_exeRootObject, _queryRootObject) && !(await PreLoadData()))
        {
            return null;
        }

        var newTCModels = MergeModelstoTCsBy(_exeRootObject, _queryRootObject, out bool succeedMerge);

        if (succeedMerge)
        {
            TestCasesModel = newTCModels;
            TestSuite = TestCasesModel.First().ParentTestSuite;
            TestPlan = TestSuite.ParentTestPlan;
            IsTestCasesLoadOver = true;
        }

        return TestCasesModel;
    }

    //TODO to show the concat progress
    public async Task<ConcurrentBag<Models.OTE_OfflineModel>> GET_OTEsAsync()
    {
        IsTestCasesLoadOver = false;

        if (!CheckModels(_exeRootObject, _queryRootObject) && !(await PreLoadData()))
        {
            return null;
        }

        var newOTEModels = MergeModelstoOTEsBy(_exeRootObject, _queryRootObject, out bool succeedMerge);

        if (succeedMerge)
        {
            OTEs_OfflineModel = newOTEModels;
            IsTestCasesLoadOver = true;

        }
        return OTEs_OfflineModel;
    }

    private bool CheckModels(ExecuteVSTSModel.RootObject exeModel , QueryVSTSModel.RootObject querModel)
    {
        if( exeModel == null || querModel == null ) { return false; }

        if( exeModel.count != querModel.count ) { return false; }

        if( exeModel.value[0].testPlan.id != querModel.value[0].testPlan.id ) return false;

        return true;
    }

    public ConcurrentBag<TestCase> MergeModelstoTCsBy(ExecuteVSTSModel.RootObject exeModel , QueryVSTSModel.RootObject querModel , out bool succeedMerge)
    {
        succeedMerge = false;

        if( !CheckModels(exeModel , querModel) ) return null;

        Models.TestPlan mTestPlan = new Models.TestPlan
        {
            ID = exeModel.value[0].testPlan.id ,
            Name = exeModel.value[0].testPlan.name
        };

        Models.TestSuite mTestSuite = new Models.TestSuite
        {
            ID = exeModel.value[0].testSuite.id ,
            Name = exeModel.value[0].testSuite.name
        };

        var TestCases = new ConcurrentBag<TestCase>(exeModel.value.Select(v =>
        {
            return new TestCase()
            {
                ID = v.workItem.id ,
                Name = v.workItem.name ,
                CQID = v.workItem.fields.FirstOrDefault(field => field.CQId != null)?.CQId ,
                IsAutomated = v.workItem.fields.FirstOrDefault(field => field.stateofAutomation != null)?.stateofAutomation.Contains("Automated" , System.StringComparison.OrdinalIgnoreCase) ,
                OutcomeStr = querModel.value.FirstOrDefault(tempQueryModel => tempQueryModel.testCaseReference.id == v.workItem.id)?.results.outcome ,
                ProductArea = v.workItem.fields.FirstOrDefault(field => field.productArea != null)?.productArea ,
                ScriptName = v.workItem.fields.FirstOrDefault(field => field.scriptName != null)?.scriptName ,
                SelfTestPoint = new TestPoint()
                {

                    Name = v.pointAssignments.FirstOrDefault(point => point.configurationName != null)?.configurationName ,
                    ID = (int)v.pointAssignments.FirstOrDefault(point => point.id >= default(int))?.id ,
                    ConfigurationId = (int)v.pointAssignments.FirstOrDefault(point => point.configurationId >= default(int))?.configurationId ,
                    lastUpdatedDate = querModel.value.FirstOrDefault(tempQueryModel => tempQueryModel.testCaseReference.id == v.workItem.id)?.lastUpdatedDate ,
                    displayName = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.displayName ,
                    uniqueName = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.uniqueName ,
                } ,
                TestToolStr = v.workItem.fields.FirstOrDefault(field => field.testTool != null)?.testTool ,
                ParentTestSuite = mTestSuite ,
            };
        }));

        mTestPlan.ChildTestSuite = mTestSuite;
        mTestSuite.ParentTestPlan = mTestPlan;
        mTestSuite.ChildTestCases = TestCases;

        succeedMerge = true;
        return TestCases;
    }

    public ConcurrentBag<OTE_OfflineModel> MergeModelstoOTEsBy(ExecuteVSTSModel.RootObject exeModel , QueryVSTSModel.RootObject querModel , out bool succeedMerge)
    {
        succeedMerge = false;

        if( !CheckModels(exeModel , querModel) ) return null;

        var OTEModels = new ConcurrentBag<OTE_OfflineModel>(exeModel.value.Select(v =>
        {
            return new OTE_OfflineModel()
            {
                TestCaseId = v.workItem.id ,
                Title = v.workItem.name ,
                TestPointId = (int)v.pointAssignments.FirstOrDefault(point => point.id >= default(int))?.id ,
                Configuration = v.pointAssignments.FirstOrDefault(point => point.configurationName != null)?.configurationName ,
                AssignTo = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.displayName ,
                OutcomeStr = querModel.value.FirstOrDefault(tempQueryModel => tempQueryModel.testCaseReference.id == v.workItem.id)?.results.outcome ,
                RunBy = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.uniqueName ,
            };
        }));

        succeedMerge = true;
        return OTEModels;
    }

    /// <summary>
    /// ���Դ������� URI �ַ����л�ȡ TestPlan �� TestSuite �� ID��
    /// </summary>
    /// <param name="completeUri">������ URI �ַ�����</param>
    /// <param name="succeedMatch">����ɹ�ƥ�� URI �ַ�������Ϊ true������Ϊ false��</param>
    /// <returns>����ɹ�ƥ�� URI �ַ������򷵻ذ��� TestPlan �� TestSuite ID �� TestPlanSuiteId �ṹ�壻���򷵻�Ĭ��ֵ��</returns>
    public static TestPlanSuiteId TryGetTestPlanSuiteId(string completeUri , out bool succeedMatch)
    {
        succeedMatch = false;
        string pattern = @"planId=(?<planId>\d+)&suiteId=(?<suiteId>\d+)";
        RegexOptions options = RegexOptions.IgnoreCase;

        Match m = Regex.Match(completeUri , pattern , options);

        if( m.Success )
        {
            int planId = int.Parse(m.Groups["planId"].Value);
            int suiteId = int.Parse(m.Groups["suiteId"].Value);
            succeedMatch = true;
            return new TestPlanSuiteId(planId , suiteId);
        }
        else
        {
            return new TestPlanSuiteId();
        }
    }
}
