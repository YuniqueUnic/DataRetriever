using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public int PlanId { get; set; } = -1;

    /// <summary>
    /// TestSuite ID。如果未设置，则为 -1。
    /// </summary>
    public int SuiteId { get; set; } = -1;

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
    public int ID { get; set; } = -1;
    public int TestPointId => ID;
    public int ConfigurationId { get; set; } = -1;
    public string? ConfiguratioName => Name;
    public string? lastUpdatedDate { get; set; }
    public string? displayName { get; set; }
    public string? uniqueName { get; set; }
}

public class TestCase : ITestObject, IResultsModel
{
    private TestTools? _testTools { get; set; }
    private OutcomeState? _outcome { get; set; }

    public string? Name { get; set; }
    public int ID { get; set; } = -1;
    public string? CQID { get; set; }
    public bool? IsAutomated { get; set; }
    public string? ScriptName { get; set; }
    public TestPoint? SelfTestPoint { get; set; }
    public TestSuite? ParentTestSuite { get; set; }
    public string? ProductArea { get; set; }
    public TestTools? TestTool { get => _testTools; }
    public OutcomeState? Outcome => _outcome;

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

    public bool Contains(string value)
    {
        // search for the value in all public properties and fields of the object
        foreach( var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) )
        {
            var propertyValue = property.GetValue(this)?.ToString();
            if( propertyValue != null && propertyValue.Contains(value) )
            {
                return true;
            }
        }

        foreach( var field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance) )
        {
            var fieldValue = field.GetValue(this)?.ToString();
            if( fieldValue != null && fieldValue.Contains(value) )
            {
                return true;
            }
        }

        return false;
    }

    public HashSet<string> AllToHashSet( )
    {
        var hashSet = new HashSet<string>();

        foreach( var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) )
        {
            var propertyValue = property.GetValue(this)?.ToString();
            if( propertyValue != null )
            {
                hashSet.Add(propertyValue);
            }

            if( typeof(IEnumerable).IsAssignableFrom(property.PropertyType) )
            {
                var enumerable = property.GetValue(this) as IEnumerable;
                if( enumerable != null )
                {
                    foreach( var testCase in enumerable.OfType<TestCase>() )
                    {
                        hashSet.UnionWith(testCase.AllToHashSet());
                    }
                }
            }
            else if( property.PropertyType.IsClass && property.PropertyType != typeof(object) )
            {
                var nestedTestCase = property.GetValue(this) as TestCase;
                if( nestedTestCase != null )
                {
                    hashSet.UnionWith(nestedTestCase.AllToHashSet());
                }
            }
        }

        foreach( var field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance) )
        {
            var fieldValue = field.GetValue(this)?.ToString();
            if( fieldValue != null )
            {
                hashSet.Add(fieldValue);
            }

            if( typeof(IEnumerable).IsAssignableFrom(field.FieldType) )
            {
                var enumerable = field.GetValue(this) as IEnumerable;
                if( enumerable != null )
                {
                    foreach( var testCase in enumerable.OfType<TestCase>() )
                    {
                        hashSet.UnionWith(testCase.AllToHashSet());
                    }
                }
            }
            // else if( field.FieldType.IsClass && field.FieldType != typeof(object) )
            // {
            //     var nestedTestCase = field.GetValue(this) as TestCase;
            //     if( nestedTestCase != null )
            //     {
            //         hashSet.UnionWith(nestedTestCase.AllToHashSet());
            //     }
            // }
        }

        return hashSet;
    }
}
public class TestSuite : ITestObject
{
    public string? Name { get; set; }
    public int ID { get; set; } = -1;
    public ConcurrentBag<TestCase>? ChildTestCases { get; set; } = new();
    public TestPlan? ParentTestPlan { get; set; }

}
public class TestPlan : ITestObject
{
    public string? Name { get; set; }
    public int ID { get; set; } = -1;
    public TestSuite? ChildTestSuite { get; set; }

}

public interface ITestObject
{
    public string? Name { get; set; }
    public int ID { get; set; }
}


public class OTE_OfflineModel : IResultsModel
{
    private OutcomeState? _outcome { get; set; }

    public int testCaseId { get; set; } = -1;
    public string? title { get; set; }
    public string? testStep { get; private set; } = string.Empty;
    public string? stepAction { get; private set; } = string.Empty;
    public string? stepExpected { get; private set; } = string.Empty;
    public int testPointId { get; set; } = -1;
    public string? configuration { get; set; }
    public string? assignTo { get; set; }
    public string? comment { get; private set; } = string.Empty;
    public string? defects { get; private set; } = string.Empty;
    public string? runBy { get; set; }

    public OutcomeState? Outcome => _outcome;

    public string? OutcomeStr
    {
        get => _outcome.GetStringValue();
        set => _outcome = value.SetEnumValueIgnoreCase<OutcomeState>();
    }

    public bool Contains(string value)
    {
        // search for the value in all public properties and fields of the object
        foreach( var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) )
        {
            var propertyValue = property.GetValue(this)?.ToString();
            if( propertyValue != null && propertyValue.Contains(value) )
            {
                return true;
            }
        }

        foreach( var field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance) )
        {
            var fieldValue = field.GetValue(this)?.ToString();
            if( fieldValue != null && fieldValue.Contains(value) )
            {
                return true;
            }
        }

        return false;
    }
    public HashSet<string> AllToHashSet( )
    {
        var hashSet = new HashSet<string>();

        foreach( var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) )
        {
            var propertyValue = property.GetValue(this)?.ToString();
            if( propertyValue != null )
            {
                hashSet.Add(propertyValue);
            }

            if( typeof(IEnumerable).IsAssignableFrom(property.PropertyType) )
            {
                var enumerable = property.GetValue(this) as IEnumerable;
                if( enumerable != null )
                {
                    foreach( var testCase in enumerable.OfType<TestCase>() )
                    {
                        hashSet.UnionWith(testCase.AllToHashSet());
                    }
                }
            }
            else if( property.PropertyType.IsClass && property.PropertyType != typeof(object) )
            {
                var nestedTestCase = property.GetValue(this) as TestCase;
                if( nestedTestCase != null )
                {
                    hashSet.UnionWith(nestedTestCase.AllToHashSet());
                }
            }
        }

        foreach( var field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance) )
        {
            var fieldValue = field.GetValue(this)?.ToString();
            if( fieldValue != null )
            {
                hashSet.Add(fieldValue);
            }

            if( typeof(IEnumerable).IsAssignableFrom(field.FieldType) )
            {
                var enumerable = field.GetValue(this) as IEnumerable;
                if( enumerable != null )
                {
                    foreach( var testCase in enumerable.OfType<TestCase>() )
                    {
                        hashSet.UnionWith(testCase.AllToHashSet());
                    }
                }
            }
        }

        return hashSet;
    }
}

interface IResultsModel
{
    public abstract bool Contains(string value);
    public abstract HashSet<string> AllToHashSet( );
}