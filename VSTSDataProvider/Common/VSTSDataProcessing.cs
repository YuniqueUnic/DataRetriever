using VSTSDataProvider.Models;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace VSTSDataProvider.Common;

public class VSTSDataProcessing
{
    private static string vstsBaseUrl = @"https://aspentech-alm.visualstudio.com/AspenTech/_apis/testplan/";
    private static TestPlan _testPlan = new();
    private static TestSutie _testSutie = new();
    private static ConcurrentBag<TestCase> _testCases = new();
    private static readonly HttpClient httpClient = new HttpClient();
    private static bool _testCasesLoadedOver = false;


    public TestPlan TestPlan { get => _testPlan; }
    public TestSutie TestSutie { get => _testSutie; }
    public ConcurrentBag<TestCase> TestCases { get => _testCases; }
    public bool IsTestCasesLoaded { get => _testCasesLoadedOver; }

    public static T NewVSTSObject<T>(JToken targetJToken)
    where T : ITestObject, new()
    {
        return new T
        {
            Name = (string)targetJToken["name"],
            ID = (int)targetJToken["id"]
        };
    }

    // This method get the planId and suiteId in a string and an out boolean parameter and returns a dictionary
    public static Dictionary<string, int> TryGetTestPlanSuiteID(string completeUri, out bool succeedMatch)
    {
        succeedMatch = false;
        Dictionary<string, int> IdDic = new Dictionary<string, int>();
        string pattern = @"planId=(?<planId>\d+)&suiteId=(?<suiteId>\d+)";
        RegexOptions options = RegexOptions.IgnoreCase;

        Match m = Regex.Match(completeUri, pattern, options);
        for (int i = 1; i < m.Groups.Count; i++)
        {
            IdDic.Add(m.Groups[i].Name, int.Parse(m.Groups[i].Value));
        }
        succeedMatch = m.Success;
        return IdDic;
    }

}
