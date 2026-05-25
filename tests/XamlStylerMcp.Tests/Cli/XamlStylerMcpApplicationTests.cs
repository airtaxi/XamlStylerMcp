using XamlStylerMcp.CommandExecution;

namespace XamlStylerMcp.Tests.Cli;

public sealed class XamlStylerMcpApplicationTests
{
    [Fact]
    public async Task Help_PrintsCustomHelp()
    {
        var outputWriter = new StringWriter();

        var exitCode = await XamlStylerMcpApplication.RunAsync(["help"], new RecordingProviderCommandRunner(), outputWriter, new StringWriter(), TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains("mcp-install", outputWriter.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task InstallProvider_RunsExpectedCodexCommand()
    {
        var runner = new RecordingProviderCommandRunner();

        var exitCode = await XamlStylerMcpApplication.RunAsync(["mcp-install", "--provider", "codex"], runner, new StringWriter(), new StringWriter(), TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        var command = Assert.Single(runner.Commands);
        Assert.Equal("codex", command.FileName);
        Assert.Equal(["mcp", "add", "xaml-styler", "--", "xaml-styler-mcp"], command.Arguments);
    }

    [Fact]
    public async Task InstallConfig_EditsMcpJson()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var configPath = Path.Combine(temporaryDirectory.Path, "mcp.json");

        var exitCode = await XamlStylerMcpApplication.RunAsync(["mcp-install", "--config", configPath], new RecordingProviderCommandRunner(), new StringWriter(), new StringWriter(), TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains("xaml-styler-mcp", File.ReadAllText(configPath), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProviderAndConfigTogether_ReturnsUsageError()
    {
        var errorWriter = new StringWriter();

        var exitCode = await XamlStylerMcpApplication.RunAsync(["mcp-install", "--provider", "codex", "--config", "mcp.json"], new RecordingProviderCommandRunner(), new StringWriter(), errorWriter, TestContext.Current.CancellationToken);

        Assert.Equal(2, exitCode);
        Assert.Contains("exactly one", errorWriter.ToString(), StringComparison.Ordinal);
    }
}
