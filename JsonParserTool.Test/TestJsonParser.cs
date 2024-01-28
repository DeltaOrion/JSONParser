using JSONParserTool;
using Xunit.Abstractions;

namespace JsonParserTool.Test;

public class TestJsonParser
{
    private readonly ITestOutputHelper _output;

    public TestJsonParser(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void UseJsonTestCases()
    {
        var paths = Directory.GetFiles(
            "C:\\Users\\User\\Documents\\.Jaeger\\Project\\SystemDesign\\JSONParser\\JSONParser\\JsonParserTool.Test\\TestCases");
        foreach (var path in paths)
        {
            var fileName = Path.GetFileName(path);
            if (fileName.StartsWith("i"))
                continue;

            bool shouldPass = fileName.StartsWith("y");
            _output.WriteLine(fileName);

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    if (!shouldPass)
                    {
                        var successParse = false;
                        try
                        {
                            new JsonParser().Parse(reader);
                            successParse = true;
                        }
                        catch (Exception ex)
                        {
                        }

                        if (successParse)
                        {
                            Assert.Fail($"JSON Parser successfully parsed {fileName} but should have failed");
                        }
                    }
                    else
                    {
                        try
                        {
                            new JsonParser().Parse(reader);
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail($"JSON Parser failed to parse {fileName} but should have succeeded");
                        }
                    }
                }
            }
        }
    }
}