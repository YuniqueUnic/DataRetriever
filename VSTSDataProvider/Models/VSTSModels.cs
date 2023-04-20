using System.Collections.Concurrent;
using VSTSDataProvider.Common;

namespace VSTSDataProvider.Models;


public class TestPoint : ITestObject
{
    public string? Name { get; set; }
    public int ID { get; set; }
    public int TestPointId { get; set; }
    public string? Configuration { get; set; }
    public string? AssignedTo { get; set; }
    public string? RunBy { get; set; }

}

public class TestCase : ITestObject
{
    private ProductAreas _productArea { get; set; }
    private TestTools _testTools { get; set; }
    private OutcomeState _outcome { get; set; }

    public string? Name { get; set; }
    public int ID { get; set; }
    public int CQID { get; set; }
    public bool IsAutomated { get; set; }
    public string? ScriptName { get; set; }
    public TestPoint? SelfTestPoint { get; set; }
    public TestSutie? ParentTestSutie { get; set; }
    public ProductAreas ProductArea => _productArea;
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

    //有待完善
    public string ProductAreaStr
    {
        get
        {
            return "";
        }
    }
}
public class TestSutie : ITestObject
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
    public TestSutie? ChildTestSutie { get; set; }
}

public interface ITestObject
{
    public string? Name { get; set; }
    public int ID { get; set; }
}
