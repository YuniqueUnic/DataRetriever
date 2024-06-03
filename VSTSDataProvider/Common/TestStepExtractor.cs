using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

public class TestStep
{
    public int Index { get; set; } = -1;
    public string? Action { get; set; }
    public string? ExpectedResult { get; set; }
    public string? Description { get; set; }
}

public class TestStepExtractor
{
    private string xmlContent;
    private XmlDocument xmlDoc;

    public TestStepExtractor(string xmlContent)
    {
        try
        {
            this.xmlContent = xmlContent;
            this.xmlDoc = new XmlDocument();
            if( xmlContent != null ) { this.xmlDoc.LoadXml(xmlContent); }
        }
        catch( XmlException )
        {
            xmlDoc.CreateTextNode(xmlContent);
            //throw;
        }

    }

    public List<TestStep> ExtractTestSteps( )
    {
        List<TestStep> testSteps = new List<TestStep>();

        if( xmlContent == null )
        {
            return testSteps;
        }

        XmlNodeList stepNodes = xmlDoc.SelectNodes("//step");
        int index = 1;
        foreach( XmlNode stepNode in stepNodes )
        {
            string a = stepNode.SelectSingleNode("description").InnerText.Trim();
            string actionContent = Regex.Replace(stepNode.SelectSingleNode("./parameterizedString[1]").InnerText.Trim() , "<.*?>" , "");
            string expectedResultContent = Regex.Replace(stepNode.SelectSingleNode("./parameterizedString[2]").InnerText.Trim() , "<.*?>" , "");
            string descriptionContent = Regex.Replace(stepNode.SelectSingleNode("./description").InnerText.Trim() , "<.*?>" , "");
            TestStep testStep = new TestStep();
            // testStep.Index = int.Parse(stepNode.Attributes["id"].Value)-1;
            testStep.Index = index++;
            testStep.Action = WebUtility.HtmlDecode(actionContent);
            testStep.ExpectedResult = WebUtility.HtmlDecode(expectedResultContent);
            //testStep.Description = WebUtility.HtmlDecode(descriptionContent); ;
            testSteps.Add(testStep);
        }
        //string descriptionContent = Regex.Replace(stepNodes[0].SelectSingleNode("description").InnerText.Trim(), "<.*?>", "");

        return testSteps;
    }
    public List<string> ExtractStepDescriptions( )
    {
        List<string> descriptions = new List<string>();
        XmlNodeList stepNodes = xmlDoc.SelectNodes("//step");
        foreach( XmlNode stepNode in stepNodes )
        {
            XmlNode descNode = stepNode.SelectSingleNode("description");
            if( descNode != null )
            {
                string desc = Regex.Replace(descNode.InnerText.Trim() , "<.*?>" , "");
                if( !string.IsNullOrEmpty(desc) )
                {
                    descriptions.Add(desc);
                }
            }
        }
        return descriptions;
    }
    public List<string> ExtractActions( )
    {
        List<string> parameterizedStrings = new List<string>();
        XmlNodeList stepNodes = xmlDoc.SelectNodes("//step");
        foreach( XmlNode stepNode in stepNodes )
        {
            XmlNode paramNode = stepNode.SelectSingleNode("parameterizedString");
            if( paramNode != null )
            {
                string param = Regex.Replace(paramNode.InnerText.Trim() , "<.*?>" , "");
                if( !string.IsNullOrEmpty(param) )
                {
                    parameterizedStrings.Add(param);
                }
            }
        }
        return parameterizedStrings;
    }
}
