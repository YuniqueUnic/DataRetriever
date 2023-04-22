using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VSTSDataProvider.Models;

public class ExecuteVSTSModel
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
    private string _querySubOption => $@"&continuationToken=03B{_itemsNum}&expand=true&returnldentityRef={_returnIdentityRef}&excludeFlags={_excludeFlags}&isRecursive={_isRecursive}";

    private string _vstsBaseUrl = @"https://aspentech-alm.visualstudio.com/AspenTech/_apis/testplan/";
    private string _path => $@"Plans/{_testPlanId}/Suites/{_testSuiteId}/TestCase";
    private string _query => $@"witFields=" + string.Join("%2C" , _queryColumnList) + _querySubOption;

    public UriBuilder targetUriBuilder => new UriBuilder(_vstsBaseUrl + _path + _query);

    private int _testPlanId;
    private int _testSuiteId;
    private int _itemsNum;
    private bool _returnIdentityRef;
    private int _excludeFlags;
    private bool _isRecursive;

    public ExecuteVSTSModel(int testPlanId , int testSuiteId , int itemsNum = 1000 , bool returnIdentityRef = false , int excludeFlags = 0 , bool isRecursive = false)
    {
        _testPlanId = testPlanId;
        _testSuiteId = testSuiteId;
        _itemsNum = itemsNum;
        _returnIdentityRef = returnIdentityRef;
        _excludeFlags = excludeFlags;
        _isRecursive = isRecursive;
    }

    private RootObject DeserializeBy(string jsonMetaData)
    {
        try
        {
            // 反序列化 JSON 字符串
            return JsonConvert.DeserializeObject<RootObject>(jsonMetaData);
        }
        catch( JsonReaderException ex )
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}.\n\nLine {ex.LineNumber}, position {ex.LinePosition}\n\npath {ex.Path}");
            return null;
        }
        catch( Exception ex )
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return null;
        }
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
