
using System.Text.Json;
using ReSharperToCodeClimate;

namespace ReSharperToCodeClimateTest;

[TestClass]
public class RunToolTest
{
    private const string TestInDir = "test_data/in";
    private const string TestExpectedOutDir = "test_data/expected_out";
    private const string TestOutDir = "test_data/out";
    
    
    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        Directory.CreateDirectory(TestOutDir);
    }

    [TestMethod]
    [DataRow("test.xml", "test.json", "test.json")]
    [DataRow("test_issue_different_severity.xml", "test_issue_different_severity.json", "test_issue_different_severity.json")]
    public void CheckIfToolCanConvertTheFile(string inFilename, string outFilename, string expectedOutFilename)
    {
        string inPath = $"{TestInDir}/{inFilename}";
        string expectedOutPath = $"{TestExpectedOutDir}/{expectedOutFilename}";
        string outPath = $"{TestOutDir}/{outFilename}";

        Program.Main(new []
        {
            inPath,
            outPath
        });

        using var outDoc = JsonDocument.Parse(File.ReadAllText(outPath));
        using var expectedOutDoc = JsonDocument.Parse(File.ReadAllText(expectedOutPath));

        var outDocStr = JsonSerializer.Serialize(outDoc);
        var expectedOutDocStr = JsonSerializer.Serialize(expectedOutDoc);

        Assert.AreEqual(expectedOutDocStr, outDocStr);
    }
}