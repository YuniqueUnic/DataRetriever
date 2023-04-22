using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VSTSDataProvider.Models;

namespace VSTSDataProvider.Common;

public class VSTSDataProcessing : ViewModels.ViewModelBase.BaseViewModel
{
    private static string vstsBaseUrl = @"https://aspentech-alm.visualstudio.com/AspenTech/_apis/testplan/";
    private int _testPlanID;
    private int _testSuiteID;
    private string? _requestUri;
    private string? _cookie;
    private int _totalCount;

    private TestPlan? _testPlan;
    private TestSuite? _testSuite;
    private ConcurrentBag<TestCase> _testCases = new();
    private readonly HttpClient httpClient = new HttpClient();
    private bool _testCasesLoadedOver = false;


    public int TestSuiteID => _testSuiteID;
    public int TestPlanID => _testPlanID;
    public string? RequestUri => _requestUri;
    public string? Cooike => _cookie;
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

    public bool IsTestCasesLoaded
    {
        get => _testCasesLoadedOver;
        private set
        {
            SetProperty(ref _testCasesLoadedOver , value);
        }
    }


    public VSTSDataProcessing SetCookie(string cookie)
    {
        _cookie = cookie;
        return this;
    }

    public VSTSDataProcessing SetRequestUri(string uri)
    {
        _requestUri = uri;
        return this;
    }

    public VSTSDataProcessing SetTestPlanSuiteIDint(int planID , int suiteID)
    {
        _testPlanID = planID;
        _testSuiteID = suiteID;
        return this;
    }

    public async Task<bool> RunAsync( )
    {
        var vstsJObject = await NetUtils.SendRequestWithCookieForJObj(_requestUri , _cookie);

        if( vstsJObject["value"] is JObject values && values != null )
        {
            _totalCount = (int)(vstsJObject["count"] ?? -1);
            _testPlan = NewVSTSObjectBase<TestPlan>(values[0]!["testPlan"]!);
            _testSuite = NewVSTSObjectBase<TestSuite>(values[0]!["testSuite"]!);
            //var parallelResult = Parallel.ForEach<JToken>(values , testCaseItem =>
            //{
            //    JObject m_TCDetail = testCaseItem["workItem"];
            //    TestCase m_TestCase = new()
            //    {
            //        Name = m_TCDetail["name"] ,
            //        ID = m_TCDetail["id"] ,
            //        ProductAreaStr = m_TCDetail["workItemFields"] ,

            //    };

            //});
        }
        return true;
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
