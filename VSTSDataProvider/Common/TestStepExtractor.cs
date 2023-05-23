using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

public class TestStep{
    public int Index {get; set;}=-1;
    public string? Action{get; set;}
    public string? ExpectedResult{get; set;}
    public string? Description{get; set;}
}

public class TestStepExtractor {
    private string xmlContent;
    private XmlDocument xmlDoc;
    
    public TestStepExtractor(string xmlContent) {
        this.xmlContent = xmlContent;
        this.xmlDoc = new XmlDocument();
        this.xmlDoc.LoadXml(xmlContent);
    }

    public List<TestStep> ExtractTestSteps(){
        List<TestStep> testSteps=new List<TestStep>();
        XmlNodeList stepNodes = xmlDoc.SelectNodes("//step");
        int index=1;
        foreach (XmlNode stepNode in stepNodes)
        {
            string actionContent=Regex.Replace(stepNode.SelectSingleNode("./parameterizedString[1]").InnerText.Trim(), "<.*?>", "");
            string expectedResultContent=Regex.Replace(stepNode.SelectSingleNode("./parameterizedString[2]").InnerText.Trim(), "<.*?>", "");
            string descriptionContent=Regex.Replace(stepNode.SelectSingleNode("description").InnerText.Trim(), "<.*?>", "");
            TestStep testStep = new TestStep();
            // testStep.Index = int.Parse(stepNode.Attributes["id"].Value)-1;
            testStep.Index=index++;
            testStep.Action = actionContent;
            testStep.ExpectedResult =expectedResultContent;
            testStep.Description = descriptionContent;
            testSteps.Add(testStep);
        }

        return testSteps;
    }
    public List<string> ExtractStepDescriptions() {
        List<string> descriptions = new List<string>();
        XmlNodeList stepNodes = xmlDoc.SelectNodes("//step");
        foreach(XmlNode stepNode in stepNodes) {
            XmlNode descNode = stepNode.SelectSingleNode("description");
            if(descNode != null) {
                string desc = Regex.Replace(descNode.InnerText.Trim(), "<.*?>", "");
                if(!string.IsNullOrEmpty(desc)) {
                    descriptions.Add(desc);
                }
            }
        }
        return descriptions;
    }
    public List<string> ExtractActions() {
        List<string> parameterizedStrings = new List<string>();
        XmlNodeList stepNodes = xmlDoc.SelectNodes("//step");
        foreach(XmlNode stepNode in stepNodes) {
            XmlNode paramNode = stepNode.SelectSingleNode("parameterizedString");
            if(paramNode != null) {
                string param = Regex.Replace(paramNode.InnerText.Trim(), "<.*?>", "");
                if(!string.IsNullOrEmpty(param)) {
                    parameterizedStrings.Add(param);
                }
            }
        }
        return parameterizedStrings;
    }
}
