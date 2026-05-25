namespace XamlStylerMcp.Cli;

public static class HelpText
{
    public static string Create()
    {
        return """
        xaml-styler-mcp

        Runs a stdio MCP server that formats XAML through XamlStyler.

        Usage:
          xaml-styler-mcp
          xaml-styler-mcp help
          xaml-styler-mcp mcp-install --provider codex|claude|copilot|gemini [--name xaml-styler]
          xaml-styler-mcp mcp-install --config <mcp.json> [--name xaml-styler]
          xaml-styler-mcp mcp-remove --provider codex|claude|copilot|gemini [--name xaml-styler]
          xaml-styler-mcp mcp-uninstall --provider codex|claude|copilot|gemini [--name xaml-styler]

        Providers:
          codex    Uses: codex mcp add/remove
          claude   Uses: claude mcp add/remove
          copilot  Uses: copilot mcp add/remove
          gemini   Uses: gemini mcp add/remove

        Options:
          --provider <name>  Install or remove through a supported provider CLI.
          --config <path>    Edit an arbitrary mcp.json directly for other providers.
          --name <name>      MCP server name. Defaults to xaml-styler.

        Examples:
          xaml-styler-mcp mcp-install --provider codex
          xaml-styler-mcp mcp-install --config C:\path\to\mcp.json --name xaml-styler
          xaml-styler-mcp mcp-remove --provider claude
        """;
    }
}
