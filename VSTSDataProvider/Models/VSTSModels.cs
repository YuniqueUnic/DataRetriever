
using System;
using System.Collections.Concurrent;
using VSTSDataProvider.Common;

namespace VSTSDataProvider.Models;

public enum OutcomeState
{
    [StringValue("failed")]
    Failed,//failed

    [StringValue("unspecified")]
    Active,//unspecified

    [StringValue("passed")]
    Passed,//passed
}

public enum TestTools
{

    [StringValue]
    SilkTest,

    [StringValue]
    Silk4Net,

    [StringValue]
    UFT,

}

//还待完善,暂定
public enum ProductAreas
{
    AAM,
    AcidGas,
    AI,
    AOT,
    Aprop,
    BLOWDOWN,
    Dyn,
    EA,
    EmbeddedAI,
    EO,
    FlareNet,
    GUI,
    HYSYS,
    OLI,
    PSV,
    RefSYS,
    Samples,
    SS,
    Sulsim,
    UnitOps,
    Upstream,
}

public class TestPoint : ITestObject
{
    public string Name { get; set; }
    public int ID { get; set; }
    public int TestPointId { get; set; }
    public string Configuration { get; set; }
    public string AssignedTo { get; set; }
    public string RunBy { get; set; }

}

public class TestCase : ITestObject
{

    public string Name { get; set; }
    public int ID { get; set; }
    public int CQID { get; set; }
    public bool IsAutomated { get; set; }
    public string? ScriptName{ get; set; }
    private TestTools _testTools { get; set; }
    public TestTools? TestTool { get => _testTools; }
    public TestPoint? SelfTestPoint { get; set; }
    private ProductAreas _productArea { get; set; }
    public ProductAreas ProductArea => _productArea;
    public TestSutie ParentTestSutie { get; set; }
    private OutcomeState _outcome { get; set; }
    public OutcomeState Outcome => _outcome;

    public string? TestToolStr
    {
        get => _testTools.GetStringValue();
        set => _testTools = value.SetEnumValueIgnoreCase<TestTools>();
    }

    // public string OutcomeStr
    // {
    //     get => _outcome.GetStringValue();
    //     set => _outcome = value.SetEnumValueIgnoreCase<OutcomeState>();
    // }

    public string? OutcomeStr
    {
        get
        {
            switch (_outcome)
            {
                case OutcomeState.Failed:
                    return "failed";
                case OutcomeState.Active:
                    return "unspecified"; // Active对应的字符串为"unspecified"
                case OutcomeState.Passed:
                    return "passed";
                default:
                    throw new Exception($"Unexpected Outcome value: {_outcome}");
                    // throw new InvalidOperationException($"Unexpected Outcome value: {_outcome}");
            }
        }
        set
        {
            switch (value.ToLower())
            {
                case "failed":
                    _outcome = OutcomeState.Failed;
                    break;
                case "unspecified": // Active对应的字符串为"unspecified"
                    _outcome = OutcomeState.Active;
                    break;
                case "passed":
                    _outcome = OutcomeState.Passed;
                    break;
                default:
                    throw new ArgumentException($"Invalid Outcome value: {value}", nameof(OutcomeStr));
                    // throw new ArgumentException($"Invalid Outcome value: {value}", nameof(OutcomeStr));
            }
        }
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
    public string Name { get; set; }
    public int ID { get; set; }
    public ConcurrentBag<TestCase> ChildTestCases { get; set; } = new();
    public TestPlan ParentTestPlan { get; set; }

}
public class TestPlan : ITestObject
{
    public string Name { get; set; }
    public int ID { get; set; }
    public TestSutie ChildTestSutie { get; set; }
}

public interface ITestObject
{
    public string Name { get; set; }
    public int ID { get; set; }
}

