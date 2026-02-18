using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.ApplicationInspector.Commands;
using Microsoft.ApplicationInspector.Common;
using Microsoft.ApplicationInspector.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AppInspector.Tests.Commands;

[ExcludeFromCodeCoverage]
public class TestExportTagsCmd
{
    private ILoggerFactory factory = new NullLoggerFactory();

    private readonly LogOptions logOptions = new();
    private string testRulesPath = string.Empty;
    public TestExportTagsCmd()
    {
        factory = logOptions.GetLoggerFactory();
        testRulesPath = Path.Combine("TestData","TestExportTagsCmd","Rules", "TestRules.json");
    }

    [Fact]
    public void ExportCustom()
    {
        ExportTagsOptions options = new()
        {
            IgnoreDefaultRules = true,
            CustomRulesPath = testRulesPath
        };
        ExportTagsCommand command = new(options, factory);
        var result = command.GetResult();
        Assert.Contains("Test.Tags.Linux", result.TagsList);
        Assert.Contains("Test.Tags.Windows", result.TagsList);
        Assert.Equal(2, result.TagsList.Count);
        Assert.Equal(ExportTagsResult.ExitCode.Success, result.ResultCode);
    }

    [Fact]
    public void ExportDefault()
    {
        ExportTagsOptions options = new()
        {
            IgnoreDefaultRules = false
        };
        ExportTagsCommand command = new(options, factory);
        var result = command.GetResult();
        Assert.Equal(ExportTagsResult.ExitCode.Success, result.ResultCode);
    }

    [Fact]
    public void NoDefaultNoCustomRules()
    {
        ExportTagsOptions options = new()
        {
            IgnoreDefaultRules = true
        };
        Assert.Throws<OpException>(() => new ExportTagsCommand(options));
    }

    [Fact]
    public void ExportJsonSerialization()
    {
        ExportTagsOptions options = new()
        {
            IgnoreDefaultRules = true,
            CustomRulesPath = testRulesPath
        };
        ExportTagsCommand command = new(options, factory);
        var result = command.GetResult();
        
        // Test JSON serialization to ensure tags are included
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        };
        
        // Serialize using the actual runtime type (the fix)
        string json = System.Text.Json.JsonSerializer.Serialize(result, result.GetType(), jsonOptions);
        
        // Verify tags are present in JSON
        Assert.Contains("Test.Tags.Linux", json);
        Assert.Contains("Test.Tags.Windows", json);
        Assert.Contains("tagsList", json);
        Assert.Contains("appVersion", json);
    }
}