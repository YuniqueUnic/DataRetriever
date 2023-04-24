using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSTSDataProvider.Common;

namespace VSTSDataProvider.Models;
public interface IVSTSModel
{
    UriBuilder TargetUriBuilder { get; }
    Task<T> GetModel<T>( );
}

public abstract class BaseVSTSModel : IVSTSModel
{
    internal string targetVSTSObject = "Custom by inherited Class";
    internal string _vstsBaseUri => $"https://aspentech-alm.visualstudio.com//AspenTech/_apis/testplan/Plans/{TestPlanId}/Suites/{TestSuiteId}/" + targetVSTSObject;

    internal string[] selectFields = new string[]
    {
        "Custom by inherited Class"
    };

    internal Dictionary<string , object> optionalParameters = new Dictionary<string , object>
    {
        {"Custom by inherited Class","ReWrite Var is essential"},
    };
    private bool disposedValue;

    public virtual UriBuilder TargetUriBuilder => ParseToUriBuilder(_vstsBaseUri , selectFields , optionalParameters);

    internal string Cookie { get; }
    internal int TestPlanId { get; }
    internal int TestSuiteId { get; }

    public BaseVSTSModel(string cookie , int testPlanId , int testSuiteId)
    {
        Cookie = cookie;
        TestPlanId = testPlanId;
        TestSuiteId = testSuiteId;
    }

    public virtual async Task<T> GetModel<T>( )
    {
        var responseContent = await NetUtils.SendRequestWithCookieForStr(TargetUriBuilder.ToString() , Cookie);
        return DeserializeBy<T>(responseContent);
    }

    internal virtual UriBuilder ParseToUriBuilder(string url , string[] selectFields , Dictionary<string , object> optionalParameters)
    {

        // 构建查询参数
        var queryParameters = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryParameters["witFields"] = string.Join("," , selectFields);
        foreach( var parameter in optionalParameters )
        {
            queryParameters[parameter.Key] = parameter.Value.ToString();
        }
        // 构建完整的 URI
        var uriBuilder = new UriBuilder(url)
        {
            Query = queryParameters.ToString()
        };
        // 返回 UriBuilder
        return uriBuilder;
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
    internal new string targetVSTSObject = "TestCase";
    internal new string[] selectFields = new string[]
    {
        "System.Id",
        "Aspentech.Common.ProductName",
        "Aspentech.Common.ProductArea",
        "Aspentech.Common.CQID",
        "Aspentech.TestCase.TestTool",
        "Aspentech.TestCase.ScriptName",
        "Aspentech.common.SubmitDate",
        "System.AreaPath",
        "Aspentech.TestCase.StateofTest"
    };

    internal new Dictionary<string , object> optionalParameters = new Dictionary<string , object>
    {
        { "continuationToken", $"0;1000" },
        { "expand", "true" },
        { "returnldentityRef", "true" },
        { "excludeFlags", "0" },
        { "isRecursive", "false" }
    };

    public ExecuteVSTSModel(string cookie , int testPlanId , int testSuiteId)
            : base(cookie , testPlanId , testSuiteId)
    {
        base.targetVSTSObject = this.targetVSTSObject;
        base.selectFields = this.selectFields;
        base.optionalParameters = this.optionalParameters;
    }

    public async Task<ExecuteVSTSModel.RootObject> GetModel( )
    {
        return await base.GetModel<ExecuteVSTSModel.RootObject>();
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
        public int id { get; set; } = -1;
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
        public int id { get; set; } = -1;
        public string name { get; set; }
    }

    public class WorkItem
    {
        public int id { get; set; } = -1;
        public string name { get; set; }
        [JsonProperty("workItemFields")]
        public List<WorkItemField> fields { get; set; }
    }

    public class WorkItemField
    {
        [JsonProperty("System.Id")]
        public int id { get; set; } = -1;
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
        public int id { get; set; } = -1;
        public string configurationName { get; set; }
        public Tester tester { get; set; }
        public int configurationId { get; set; } = -1;
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
        public int count { get; set; } = -1;
        public List<Value> value { get; set; }
    }

    #endregion Json to Entity Class

}
