using MiniExcelLibs.Attributes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
    //private TestTools? _testTools { get; set; }
    //private OutcomeState? _outcome { get; set; }

    public int ID { get; set; } = -1;
    public string? Name { get; set; }
    public string? CQID { get; set; }
    public string? StateofAutomation { get; set; }
    public string? ScriptName { get; set; }
    [ExcelIgnore]
    public TestPoint? SelfTestPoint { get; set; }
    [ExcelIgnore]
    public TestSuite? ParentTestSuite { get; set; }
    public string? ProductArea { get; set; }
    public TestTools TestTool { get; set; }
    public OutcomeState Outcome { get; set; }

    [ExcelIgnore]
    public string? TestToolStr
    {
        get => TestTool.GetStringValue();
        set => TestTool = value.SetEnumValueIgnoreCase<TestTools>();
    }

    [ExcelIgnore]
    public string? OutcomeStr
    {
        get => Outcome.GetStringValue();
        set => Outcome = value.SetEnumValueIgnoreCase<OutcomeState>();
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

    public bool SetPropertyValue(string propertyName , object value)
    {
        throw new System.NotImplementedException();
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




public class DetailModel : IResultsModel
{
    [ExcelIgnore]
    public int Index { get; set; } = -1;
    public int TestPlanId { get; set; } = -1;
    public int TestSuiteId { get; set; } = -1;
    public int ID { get; set; } = -1;
    [ExcelColumnName("Title")]
    public string? Name { get; set; }
    public string? CQID { get; set; }
    public string? ProductArea { get; set; }
    public string? ScriptName { get; set; }
    public TestTools TestTool { get; set; }
    public OutcomeState Outcome { get; set; }
    public int TestPointId { get; set; } = -1;
    public string? Configuration { get; set; }
    public string? LastUpdatedDate { get; set; }
    public string? RunBy { get; set; }
    public string? StateofAutomation { get; set; }
    [ExcelIgnore]
    public List<TestStep>? TestSteps { get; set; }

    [ExcelIgnore]
    public string? TestToolStr
    {
        get => TestTool.GetStringValue();
        set => TestTool = value.SetEnumValueIgnoreCase<TestTools>();
    }

    [ExcelIgnore]
    public string? OutcomeStr
    {
        get => Outcome.GetStringValue();
        set => Outcome = value.SetEnumValueIgnoreCase<OutcomeState>();
    }

    [ExcelIgnore]
    public string? TestStepsStr
    {
        //  TestSteps.Aggregate((current, next) => current + System.Environment.NewLine + next)
        get => "";
        set
        {
            List<TestStep> stepsStrings = new TestStepExtractor(value).ExtractTestSteps();
            TestSteps = stepsStrings;
        }
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

    private static readonly Dictionary<string , string> PropertyNameMappings = new Dictionary<string , string>
     {
          { "Title", nameof(Name) },
          { "TestCaseId", nameof(ID) },
          { "Outcome", nameof(OutcomeStr) },
          { "TestTool", nameof(TestToolStr) }
      };

    public bool SetPropertyValue(string propertyName , object value)
    {
        bool success = false;
        if( PropertyNameMappings.TryGetValue(propertyName , out var mappedName) )
        {
            propertyName = mappedName;
        }

        // Get the properties infos
        PropertyInfo propertyInfo = typeof(DetailModel).GetProperty(propertyName);

        // Judge the type of property and convert to the case
        var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
        if( propertyInfo.CanWrite && converter.CanConvertFrom(value.GetType()) )
        {
            try
            {
                var convertedValue = converter.ConvertFrom(value);
                propertyInfo.SetValue(this , convertedValue);
                success = true;
            }
            catch( System.Exception e )
            {
                Console.WriteLine(e.Message);
                //throw;
            }
        }
        return success;
    }
}

public class OTE_OfflineModel : IResultsModel
{
    //private OutcomeState? _outcome { get; set; }
    [ExcelIgnore]
    public int Index { get; set; } = -1;
    public int TestCaseId { get; set; } = -1;
    public string? Title { get; set; }
    public string? TestStep { get; set; } = string.Empty;
    public string? StepAction { get; set; } = string.Empty;
    public string? StepExpected { get; set; } = string.Empty;
    public int TestPointId { get; set; } = -1;
    public string? Configuration { get; set; }

    [ExcelColumnName("AssignedTo")]
    public string? AssignTo { get; set; }
    public OutcomeState Outcome { get; set; }
    public string? Comment { get; set; } = string.Empty;
    public string? Defects { get; set; } = string.Empty;
    public string? RunBy { get; set; }


    [ExcelIgnore]
    public string? OutcomeStr
    {
        get => Outcome.GetStringValue();
        set => Outcome = value.SetEnumValueIgnoreCase<OutcomeState>();
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


    private static readonly Dictionary<string , string> PropertyNameMappings = new Dictionary<string , string>
    {
          { "Outcome", nameof(OutcomeStr) },
      };


    public bool SetPropertyValue(string propertyName , object value)
    {
        bool success = false;
        if( PropertyNameMappings.TryGetValue(propertyName , out var mappedName) )
        {
            propertyName = mappedName;
        }
        // Get the properties infos
        PropertyInfo propertyInfo = typeof(OTE_OfflineModel).GetProperty(propertyName);

        // Judge the type of property and convert to the case
        var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
        if( propertyInfo.CanWrite && converter.CanConvertFrom(value.GetType()) )
        {
            try
            {
                var convertedValue = converter.ConvertFrom(value);
                propertyInfo.SetValue(this , convertedValue);
                success = true;
            }
            catch( System.Exception e )
            {
                Console.WriteLine(e.Message);
                //throw;
            }
        }
        return success;

    }

}

public interface IResultsModel
{
    public abstract bool Contains(string value);
    public abstract HashSet<string> AllToHashSet( );
    public abstract bool SetPropertyValue(string propertyName , object value);
}