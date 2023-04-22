using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSTSDataProvider.Models;

public class QueryVSTSModel : BaseVSTSModel
{
    internal override string QuerySubOption => $@"
             continuationToken=-2147483648;{ItemsNum}
             &returnIdentityRef={ReturnIdentityRef}
             &includePointDetails={IncludePointDetails}
             &isRecursive={IsRecursive}";

    public override string Query => $@"?" + QuerySubOption;


    public QueryVSTSModel(string cookie , int testPlanId , int testSuiteId , int itemsNum = 1000 , bool returnIdentityRef = false , bool includePointDetails = false , bool isRecursive = false)
            : base(cookie , testPlanId , testSuiteId , itemsNum , returnIdentityRef , includePointDetails , 0 , isRecursive)
    {

    }

    public Task<QueryVSTSModel.RootObject> GetModel( )
    {
        return base.GetModel<QueryVSTSModel.RootObject>();
    }

    #region Json to Entity Class
    public class Tester
    {
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public Links _links { get; set; }
        public string Id { get; set; }
        public string UniqueName { get; set; }
        public string ImageUrl { get; set; }
        public string Descriptor { get; set; }
    }

    public class Configuration
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Visibility { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public class TestPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TestSuite
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class LastUpdatedBy
    {
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public Links _links { get; set; }
        public string Id { get; set; }
        public string UniqueName { get; set; }
        public string ImageUrl { get; set; }
        public string Descriptor { get; set; }
    }

    public class RunBy
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
    }

    public class LastResultDetails
    {
        public int Duration { get; set; }
        public DateTime DateCompleted { get; set; }
        public RunBy RunBy { get; set; }
    }

    public class Results
    {
        public LastResultDetails LastResultDetails { get; set; }
        public int LastResultId { get; set; }
        public string LastRunBuildNumber { get; set; }
        public string State { get; set; }
        public string LastResultState { get; set; }
        public string Outcome { get; set; }
        public int LastTestRunId { get; set; }
    }

    public class Links
    {
        public Avatar Avatar { get; set; }
    }

    public class Avatar
    {
        public string Href { get; set; }
    }

    public class TestCaseReference
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
    }

    public class Value
    {
        public int Id { get; set; }
        public Tester Tester { get; set; }
        public Configuration Configuration { get; set; }
        public bool IsAutomated { get; set; }
        public Project Project { get; set; }
        public TestPlan TestPlan { get; set; }
        public TestSuite TestSuite { get; set; }
        public LastUpdatedBy LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public Results Results { get; set; }
        public DateTime LastResetToActive { get; set; }
        public bool IsActive { get; set; }
        public Links Links { get; set; }
        public TestCaseReference TestCaseReference { get; set; }
    }

    public class RootObject
    {
        public List<Value> Value { get; set; }
        public int Count { get; set; }
    }
    #endregion Json to Entity Class
}
