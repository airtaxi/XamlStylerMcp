using System.ComponentModel;
using ModelContextProtocol.Server;
using XamlStylerMcp.Formatting;

namespace XamlStylerMcp.Mcp;

[McpServerToolType]
public static class XamlStylerTools
{
    [McpServerTool(Name = "format_xaml", ReadOnly = true), Description("Formats XAML content and returns the formatted result.")]
    public static string FormatXaml([Description("XAML content to format.")] string content, [Description("Optional Settings.XamlStyler path.")] string? configPath = null)
    {
        return new XamlFormattingService().FormatXaml(content, configPath);
    }

    [McpServerTool(Name = "format_xaml_file", Destructive = true, UseStructuredContent = true), Description("Formats a .xaml or .axaml file and optionally writes changes to disk.")]
    public static XamlFileFormatResult FormatXamlFile([Description("Path to a .xaml or .axaml file.")] string filePath, [Description("Optional Settings.XamlStyler path.")] string? configPath = null, [Description("Whether to write formatting changes to disk.")] bool writeChanges = true)
    {
        return new XamlFormattingService().FormatFile(filePath, configPath, writeChanges);
    }

    [McpServerTool(Name = "check_xaml_file", ReadOnly = true, UseStructuredContent = true), Description("Checks whether a .xaml or .axaml file is already formatted.")]
    public static XamlFileCheckResult CheckXamlFile([Description("Path to a .xaml or .axaml file.")] string filePath, [Description("Optional Settings.XamlStyler path.")] string? configPath = null)
    {
        return new XamlFormattingService().CheckFile(filePath, configPath);
    }
}
