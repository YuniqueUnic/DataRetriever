using MiniExcelLibs.Attributes;
using System.Collections.Generic;
using System.Reflection;
using VSTSDataProvider.Common;

namespace VSTSDataProvider.Models;

class CommonModels
{
}

public class OpenSourceProjectInfos
{
    public string? Name { get; set; } = string.Empty;
    public string? Version { get; set; } = string.Empty;
    public string? License { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
}


public class EditingModel : Models.IResultsModel
{
    public int Index { get; set; } = -1;
    public int TestPlanId { get; set; } = -1;
    public int TestSuiteId { get; set; } = -1;
    public int ID { get; set; } = -1;
    public string? Name { get; set; }
    public string? CQID { get; set; }
    public string? ProductArea { get; set; }
    public string? TestStep { get; set; } = string.Empty;
    public string? StepAction { get; set; } = string.Empty;
    public string? StepExpected { get; set; } = string.Empty;
    public string? ScriptName { get; set; }
    public TestTools TestTool { get; set; }
    public OutcomeState Outcome { get; set; }
    public int TestPointId { get; set; } = -1;
    public string? Configuration { get; set; }
    public string? AssignTo { get; set; }
    public string? LastUpdatedDate { get; set; }
    public string? RunBy { get; set; }
    public string? StateofAutomation { get; set; }
    public string? Comment { get; set; } = string.Empty;
    public string? Defects { get; set; } = string.Empty;
    public string? RtfLink { get; set; } = string.Empty;

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

    public void SetPropertyValue(string propertyName , object value)
    {
        throw new System.NotImplementedException();
    }

    HashSet<string> IResultsModel.AllToHashSet( )
    {
        throw new System.NotImplementedException();
    }
}