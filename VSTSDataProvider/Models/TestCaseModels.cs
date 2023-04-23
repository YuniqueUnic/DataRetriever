using System.Collections.Concurrent;
using VSTSDataProvider.Common;

namespace VSTSDataProvider.Models;

/// <summary>
/// 表示 TestPlan 和 TestSuite 的 ID。
/// </summary>
public struct TestPlanSuiteId
{
    /// <summary>
    /// TestPlan ID。如果未设置，则为 -1。
    /// </summary>
    public int PlanId { get; } = -1;

    /// <summary>
    /// TestSuite ID。如果未设置，则为 -1。
    /// </summary>
    public int SuiteId { get; } = -1;

    /// <summary>
    /// 初始化一个新的 TestPlanSuiteId 结构。
    /// </summary>
    /// <param name="planId">要设置的 TestPlan ID。</param>
    /// <param name="suiteId">要设置的 TestSuite ID。</param>
    public TestPlanSuiteId(int planId , int suiteId)
    {
        PlanId = planId;
        SuiteId = suiteId;
    }
}


public class TestPoint : ITestObject
{
    public string? Name { get; set; }
    public int ID { get; set; }
    public int TestPointId { get; set; }
    public string? Configuration { get; set; }
    public string? AssignedTo { get; set; }
    public string? RunBy { get; set; }
    public string? displayName { get; set; }
    public string? uniqueName { get; set; }
}

public class TestCase : ITestObject
{
    private TestTools _testTools { get; set; }
    private OutcomeState _outcome { get; set; }

    public string? Name { get; set; }
    public int ID { get; set; }
    public int CQID { get; set; }
    public bool IsAutomated { get; set; }
    public string? ScriptName { get; set; }
    public TestPoint? SelfTestPoint { get; set; }
    public TestSuite? ParentTestSuite { get; set; }
    public string? ProductArea { get; set; }
    public TestTools? TestTool { get => _testTools; }
    public OutcomeState Outcome => _outcome;

    public string? TestToolStr
    {
        get => _testTools.GetStringValue();
        set => _testTools = value.SetEnumValueIgnoreCase<TestTools>();
    }

    public string? OutcomeStr
    {
        get => _outcome.GetStringValue();
        set => _outcome = value.SetEnumValueIgnoreCase<OutcomeState>();
    }

}
public class TestSuite : ITestObject
{
    public string? Name { get; set; }
    public int ID { get; set; }
    public ConcurrentBag<TestCase> ChildTestCases { get; set; } = new();
    public TestPlan? ParentTestPlan { get; set; }

}
public class TestPlan : ITestObject
{
    public string? Name { get; set; }
    public int ID { get; set; }
    public TestSuite? ChildTestSuite { get; set; }

}

public interface ITestObject
{
    public string? Name { get; set; }
    public int ID { get; set; }
}
