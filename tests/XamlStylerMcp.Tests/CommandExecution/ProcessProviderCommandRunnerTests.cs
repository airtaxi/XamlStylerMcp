using XamlStylerMcp.CommandExecution;

namespace XamlStylerMcp.Tests.CommandExecution;

public sealed class ProcessProviderCommandRunnerTests
{
    [Fact]
    public async Task RunAsync_WhenCommandIsMissing_ReturnsErrorMessage()
    {
        var outputWriter = new StringWriter();
        var errorWriter = new StringWriter();
        var runner = new ProcessProviderCommandRunner(outputWriter, errorWriter);

        var exitCode = await runner.RunAsync(new ProviderCommand("xaml-styler-mcp-missing-provider-command", []), TestContext.Current.CancellationToken);

        Assert.Equal(127, exitCode);
        Assert.Contains("was not found", errorWriter.ToString(), StringComparison.Ordinal);
        Assert.Contains("xaml-styler-mcp-missing-provider-command", errorWriter.ToString(), StringComparison.Ordinal);
    }
}
