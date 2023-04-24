using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSTSDataProvider.Models;

public class QueryVSTSModel : BaseVSTSModel
{

    internal new string targetVSTSObject = "TestPoint";

    internal new string[] selectFields = new string[] { };

    internal new Dictionary<string , object> optionalParameters = new Dictionary<string , object>
    {
        { "continuationToken", $"0;1000" },
        { "returnldentityRef", "true" },
        { "includePointDetails", "true" },
        { "isRecursive", "false" }
    };

    public QueryVSTSModel(string cookie , int testPlanId , int testSuiteId)
            : base(cookie , testPlanId , testSuiteId)
    {
        base.targetVSTSObject = this.targetVSTSObject;
        base.selectFields = this.selectFields;
        base.optionalParameters = this.optionalParameters;
    }

    public async Task<QueryVSTSModel.RootObject> GetModel( )
    {
        return await base.GetModel<QueryVSTSModel.RootObject>();
    }


    #region Json to Entity Class
    public class Tester
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class Configuration
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

    public class TestPlan
    {
        public int id { get; set; } = -1;
        public string name { get; set; }
    }

    public class TestSuite
    {
        public int id { get; set; } = -1;
        public string name { get; set; }
    }

    public class LastUpdatedBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class LastResultDetails
    {
        public int duration { get; set; }
        public string dateCompleted { get; set; }
        public RunBy runBy { get; set; }
    }

    public class RunBy
    {
        public string displayName { get; set; }
        public string id { get; set; }
    }

    public class Results
    {
        public LastResultDetails lastResultDetails { get; set; }
        public string state { get; set; }
        public string outcome { get; set; }
    }

    public class Links
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
    }

    public class TestCaseReference
    {
        public int id { get; set; } = -1;
        public string name { get; set; }
        public string state { get; set; }
    }

    public class Value
    {
        public int id { get; set; } = -1;
        public Tester tester { get; set; }
        public Configuration configuration { get; set; }
        public bool isAutomated { get; set; }
        public Project project { get; set; }
        public TestPlan testPlan { get; set; }
        public TestSuite testSuite { get; set; }
        public LastUpdatedBy lastUpdatedBy { get; set; }
        public string lastUpdatedDate { get; set; }
        public Results results { get; set; }
        public string lastResetToActive { get; set; }
        public bool isActive { get; set; }
        public Links links { get; set; }
        public TestCaseReference testCaseReference { get; set; }
    }

    public class RootObject
    {
        public List<Value> value { get; set; }
        public int count { get; set; } = -1;
    }
    #endregion Json to Entity Class
}
