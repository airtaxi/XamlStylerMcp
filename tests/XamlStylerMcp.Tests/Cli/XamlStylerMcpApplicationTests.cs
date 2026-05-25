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
    public async Task InstallAllProviders_RunsEveryProviderCommand()
    {
        var runner = new RecordingProviderCommandRunner();
        var outputWriter = new StringWriter();

        var exitCode = await XamlStylerMcpApplication.RunAsync(["mcp-install", "--provider", "all"], runner, outputWriter, new StringWriter(), TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Collection(runner.Commands, command => Assert.Equal("codex", command.FileName), command => Assert.Equal("claude", command.FileName), command => Assert.Equal("copilot", command.FileName), command => Assert.Equal("gemini", command.FileName));
        Assert.Contains("Provider results:", outputWriter.ToString(), StringComparison.Ordinal);
        Assert.Contains("gemini: success", outputWriter.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemoveAllProviders_RunsEveryProviderCommandAndReturnsFailureWhenAnyCommandFails()
    {
        var runner = new RecordingProviderCommandRunner();
        runner.ExitCodes.Enqueue(0);
        runner.ExitCodes.Enqueue(127);
        runner.ExitCodes.Enqueue(0);
        runner.ExitCodes.Enqueue(0);
        var outputWriter = new StringWriter();

        var exitCode = await XamlStylerMcpApplication.RunAsync(["mcp-remove", "--provider", "all"], runner, outputWriter, new StringWriter(), TestContext.Current.CancellationToken);

        Assert.Equal(1, exitCode);
        Assert.Equal(4, runner.Commands.Count);
        Assert.All(runner.Commands, command => Assert.Equal("remove", command.Arguments[1]));
        Assert.Contains("claude: failed with exit code 127", outputWriter.ToString(), StringComparison.Ordinal);
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
