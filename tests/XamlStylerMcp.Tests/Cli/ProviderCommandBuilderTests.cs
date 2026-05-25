using XamlStylerMcp;
using XamlStylerMcp.CommandExecution;

namespace XamlStylerMcp.Tests.Cli;

public sealed class ProviderCommandBuilderTests
{
    [Fact]
    public void CreateInstallCommand_BuildsGeminiCommand()
    {
        var command = ProviderCommandBuilder.CreateInstallCommand(McpProvider.Gemini, "xaml-styler", "xaml-styler-mcp");

        Assert.Equal("gemini", command.FileName);
        Assert.Equal(["mcp", "add", "--scope", "user", "xaml-styler", "xaml-styler-mcp"], command.Arguments);
    }

    [Fact]
    public void CreateRemoveCommand_BuildsClaudeCommand()
    {
        var command = ProviderCommandBuilder.CreateRemoveCommand(McpProvider.Claude, "xaml-styler");

        Assert.Equal("claude", command.FileName);
        Assert.Equal(["mcp", "remove", "--scope", "user", "xaml-styler"], command.Arguments);
    }
}
