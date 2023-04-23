using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSTSDataProvider.Common;

namespace VSTSDataProvider.Models;

public interface IVSTSModel
{
    string Path { get; }
    string Query { get; }
    UriBuilder TargetUriBuilder { get; }
    Task<T> GetModel<T>( );
}

public abstract class BaseVSTSModel : IVSTSModel
{
    internal virtual string QuerySubOption => $@"
        &continuationToken=03B{ItemsNum}
        &expand=true
        &returnldentityRef={ReturnIdentityRef}
        &excludeFlags={ExcludeFlags}
        &isRecursive={IsRecursive}";

    internal string _vstsBaseUrl = @"https://aspentech-alm.visualstudio.com/AspenTech/_apis/testplan/";
    public virtual string Path => $@"Plans/{TestPlanId}/Suites/{TestSuiteId}/TestCase";
    public virtual string Query => "?";

    public virtual UriBuilder TargetUriBuilder => new UriBuilder(_vstsBaseUrl + Path + Query);

    internal string Cookie { get; }
    internal int TestPlanId { get; }
    internal int TestSuiteId { get; }
    internal int ItemsNum { get; }
    internal bool ReturnIdentityRef { get; }
    public bool IncludePointDetails { get; }
    internal int ExcludeFlags { get; }
    internal bool IsRecursive { get; }

    public BaseVSTSModel(string cookie , int testPlanId , int testSuiteId , int itemsNum = 1000 , bool returnIdentityRef = false , bool includePointDetails = false , int excludeFlags = 0 , bool isRecursive = false)
    {
        Cookie = cookie;
        TestPlanId = testPlanId;
        TestSuiteId = testSuiteId;
        ItemsNum = itemsNum;
        ReturnIdentityRef = returnIdentityRef;
        IncludePointDetails = includePointDetails;
        ExcludeFlags = excludeFlags;
        IsRecursive = isRecursive;
    }

    public virtual async Task<T> GetModel<T>( )
    {
        var responseContent = await NetUtils.SendRequestWithCookieForStr(TargetUriBuilder.Uri , Cookie);
        return DeserializeBy<T>(responseContent);
    }

    internal virtual T DeserializeBy<T>(string jsonMetaData)
    {
        try
        {
            // 反序列化 JSON 字符串
            return JsonConvert.DeserializeObject<T>(jsonMetaData);
        }
        catch( JsonReaderException ex )
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}.\n\nLine {ex.LineNumber}, position {ex.LinePosition}\n\npath {ex.Path}");
            return default(T);
        }
        catch( Exception ex )
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return default(T);
        }
    }
}

public class ExecuteVSTSModel : BaseVSTSModel
{
    private List<string> _queryColumnList = new List<string>()
    {
        "System.Id",
        "Aspentech.Common.CQID",
        "Aspentech.Common.ProductName",
        "Aspentech.Common.ProductArea",
        "Aspentech.TestCase.TestTool",
        "Aspentech.TestCase.ScriptName",
        "Aspentech.common.SubmitDate",
        "System.AreaPath",
    };

    private string _querySubOption => base.QuerySubOption;

    private string _vstsBaseUrl => base._vstsBaseUrl;
    private string _path => base.Path;
    public override string Query => $@"?witFields=" + string.Join("%2C" , _queryColumnList) + _querySubOption;

    public ExecuteVSTSModel(string cookie , int testPlanId , int testSuiteId , int itemsNum = 1000 , bool returnIdentityRef = false , int excludeFlags = 0 , bool isRecursive = false)
            : base(cookie , testPlanId , testSuiteId , itemsNum , returnIdentityRef , false , excludeFlags , isRecursive)
    {

    }

    public Task<RootObject> GetModel( )
    {
        return GetModel<RootObject>();
    }

    #region Json to Entity Class

    public class Value
    {
        public TestPlan testPlan { get; set; }
        public Project project { get; set; }
        public TestSuite testSuite { get; set; }
        public WorkItem workItem { get; set; }
        public List<PointAssignment> pointAssignments { get; set; }
        public Links links { get; set; }
    }

    public class TestPlan
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string visibility { get; set; }
        public string lastUpdateTime { get; set; }
    }

    public class TestSuite
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class WorkItem
    {
        public int id { get; set; }
        public string name { get; set; }
        [JsonProperty("workItemFields")]
        public List<WorkItemField> fields { get; set; }
    }

    public class WorkItemField
    {
        [JsonProperty("System.Id")]
        public int id { get; set; }
        [JsonProperty("Aspentech.Common.CQID")]
        public string CQId { get; set; }
        [JsonProperty("Aspentech.Common.ProductName")]
        public string productName { get; set; }
        [JsonProperty("Aspentech.Common.ProductArea")]
        public string productArea { get; set; }
        [JsonProperty("Aspentech.TestCase.TestTool")]
        public string testTool { get; set; }
        [JsonProperty("Aspentech.TestCase.ScriptName")]
        public string scriptName { get; set; }
        [JsonProperty("System.AreaPath")]
        public string areaPath { get; set; }
        [JsonProperty("System.TestCase.StateofTest")]
        public string stateofAutomation { get; set; }
    }

    public class PointAssignment
    {
        public int id { get; set; }
        public string configurationName { get; set; }
        public Tester tester { get; set; }
        public int configurationId { get; set; }
    }

    public class Tester
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links1 _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class Links1
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
    }

    public class Links
    {
        public Self self { get; set; }
        public TestPoints testPoints { get; set; }
        public Suites suites { get; set; }
        public Plan plan { get; set; }
        public Parent parent { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class TestPoints
    {
        public string href { get; set; }
    }

    public class Suites
    {
        public string href { get; set; }
    }

    public class Plan
    {
        public string href { get; set; }
    }

    public class Parent
    {
        public string href { get; set; }
    }

    public class RootObject
    {
        public int count { get; set; }
        public List<Value> value { get; set; }
    }

    #endregion Json to Entity Class

}
