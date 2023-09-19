
using ReSharperToCodeClimate;

namespace ReSharperToCodeClimateTest;

[TestClass]
public class RunToolTest
{
    [TestMethod]
    [DeploymentItem(@"test.xml", "optionalOutFolder")]
    public void CheckIfToolCanConvertTheFile()
    {
        Program.Main(new []
        {
            "test.xml",
            "test.json"
        });
        var content = File.ReadAllText("test.json");
        Assert.AreEqual(
            "[{\"check_name\":\"RedundantUsingDirective\",\"description\":\"Using directive is not required by the code and can be safely removed\",\"fingerprint\":\"dc7a773952228dfee22347f60fe68adc\",\"severity\":\"major\",\"location\":{\"path\":\"test.xml\",\"lines\":{\"begin\":2}}}]", content);
    }
}