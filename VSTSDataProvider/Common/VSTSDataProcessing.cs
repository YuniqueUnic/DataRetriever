using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VSTSDataProvider.Models;

namespace VSTSDataProvider.Common;

public class VSTSDataProcessing : ViewModels.ViewModelBase.BaseViewModel
{
    #region Unless Code
    private readonly HttpClient httpClient = new HttpClient();
    private static string vstsBaseUrl = @"https://aspentech-alm.visualstudio.com/AspenTech/_apis/testplan/";
    private string? _requestUri;
    public string? RequestUri => _requestUri;
    #endregion Unless Code

    private string? _cookie;
    private int _testPlanID;
    private int _testSuiteID;
    private int _totalCount;

    private TestPlan? _testPlan;
    private TestSuite? _testSuite;
    private ConcurrentBag<TestCase> _testCases = new();

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

    public ConcurrentBag<TestCase> TestCases
    {
        get => _testCases;
        private set
        {
            SetProperty(ref _testCases , value);
        }
    }

    public bool IsTestCasesLoadOver
    {
        get => _testCasesLoadOver;
        private set
        {
            SetProperty(ref _testCasesLoadOver , value);
        }
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

    //TODO Concat Two Model to One.
    public async Task<bool> RunAsync( )
    {
        var executeVSTSModel = await new ExecuteVSTSModel(_cookie , _testPlanID , _testSuiteID).GetModel();
        var queryVSTSModel = await new QueryVSTSModel(_cookie , _testPlanID , _testSuiteID).GetModel();

        if( executeVSTSModel == null || queryVSTSModel == null ) { return false; }

        if( executeVSTSModel.count != queryVSTSModel.count ) { return false; }

        if( executeVSTSModel.value[0].testPlan.id != queryVSTSModel.value[0].testPlan.id ) return false;

        TestPlan = new TestPlan
        {
            ID = executeVSTSModel.value[0].testPlan.id ,
            Name = executeVSTSModel.value[0].testPlan.name
        };

        TestSuite = new TestSuite
        {
            ID = executeVSTSModel.value[0].testSuite.id ,
            Name = executeVSTSModel.value[0].testSuite.name
        };

        _totalCount = executeVSTSModel.count;

        TestCases = new ConcurrentBag<TestCase>(executeVSTSModel.value.Select(v =>
        {
            return new TestCase()
            {
                ID = v.workItem.id ,
                Name = v.workItem.name ,
                CQID = v.workItem.fields.FirstOrDefault(field => field.CQId != null)?.CQId ,
                IsAutomated = v.workItem.fields.FirstOrDefault(field => field.stateofAutomation != null)?.stateofAutomation.Contains("Automated" , System.StringComparison.OrdinalIgnoreCase) ,
                OutcomeStr = queryVSTSModel.value.FirstOrDefault(queryModel => queryModel.testCaseReference.id == v.workItem.id)?.results.outcome ,
                ProductArea = v.workItem.fields.FirstOrDefault(field => field.productArea != null)?.productArea ,
                ScriptName = v.workItem.fields.FirstOrDefault(field => field.scriptName != null)?.scriptName ,
                SelfTestPoint = new TestPoint()
                {
                    TestPointId = v.pointAssignments.FirstOrDefault(point => point.configurationId != default(int))?.configurationId ,
                    Name = v.pointAssignments.FirstOrDefault(point => point.configurationName != null)?.configurationName ,
                    displayName = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.displayName ,
                    uniqueName = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.uniqueName ,
                } ,
                TestToolStr = v.workItem.fields.FirstOrDefault(field => field.testTool != null)?.testTool ,
                ParentTestSuite = TestSuite ,
            };
        }));

        TestPlan.ChildTestSuite = TestSuite;
        TestSuite.ParentTestPlan = TestPlan;
        TestSuite.ChildTestCases = TestCases;


        return _testCasesLoadOver = true;
    }

    /// <summary> 
    /// Creates an instance of a VSTS object that implements the ITestObject interface and initializes its properties using the specified JSON token.
    /// </summary>
    /// <typeparam name="T">The type of the VSTS object to create. Such as TestPlan, TestSuite, TestCase...</typeparam>
    /// <param name="targetJToken">The JSON token used to initialize the object's properties.</param>
    /// <returns>An instance of the specified VSTS object with its properties initialized.</returns>
    private static T NewVSTSObjectBase<T>(JToken targetJToken)
        where T : ITestObject, new()
    {
        return new T
        {
            Name = (string)targetJToken["name"] ,
            ID = (int)targetJToken["id"]
        };
    }

    /// <summary>
    /// 尝试从完整的 URI 字符串中获取 TestPlan 和 TestSuite 的 ID。
    /// </summary>
    /// <param name="completeUri">完整的 URI 字符串。</param>
    /// <param name="succeedMatch">如果成功匹配 URI 字符串，则为 true；否则为 false。</param>
    /// <returns>如果成功匹配 URI 字符串，则返回包含 TestPlan 和 TestSuite ID 的 TestPlanSuiteId 结构体；否则返回默认值。</returns>
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
