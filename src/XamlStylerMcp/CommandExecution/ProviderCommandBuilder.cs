namespace XamlStylerMcp.CommandExecution;

public static class ProviderCommandBuilder
{
    public static ProviderCommand CreateInstallCommand(McpProvider provider, string serverName, string toolCommandName)
    {
        return provider switch
        {
            McpProvider.Codex => new ProviderCommand("codex", ["mcp", "add", serverName, "--", toolCommandName]),
            McpProvider.Claude => new ProviderCommand("claude", ["mcp", "add", "--scope", "user", serverName, "--", toolCommandName]),
            McpProvider.Copilot => new ProviderCommand("copilot", ["mcp", "add", serverName, "--", toolCommandName]),
            McpProvider.Gemini => new ProviderCommand("gemini", ["mcp", "add", "--scope", "user", serverName, toolCommandName]),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
        };
    }

    public static ProviderCommand CreateRemoveCommand(McpProvider provider, string serverName)
    {
        return provider switch
        {
            McpProvider.Codex => new ProviderCommand("codex", ["mcp", "remove", serverName]),
            McpProvider.Claude => new ProviderCommand("claude", ["mcp", "remove", "--scope", "user", serverName]),
            McpProvider.Copilot => new ProviderCommand("copilot", ["mcp", "remove", serverName]),
            McpProvider.Gemini => new ProviderCommand("gemini", ["mcp", "remove", "--scope", "user", serverName]),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
        };
    }
}
