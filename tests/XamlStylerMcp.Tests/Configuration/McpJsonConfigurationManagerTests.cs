using System.Text.Json;
using XamlStylerMcp.Configuration;

namespace XamlStylerMcp.Tests.Configuration;

public sealed class McpJsonConfigurationManagerTests
{
    [Fact]
    public void Install_CreatesMcpServerEntryAndBackup()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var configPath = Path.Combine(temporaryDirectory.Path, "mcp.json");
        var json = """
        {
          // Existing servers are preserved.
          "mcpServers": {
            "other": {
              "command": "other",
              "args": [],
            },
          },
        }
        """;
        File.WriteAllText(configPath, json);

        var changed = new McpJsonConfigurationManager().Install(configPath, "xaml-styler", "xaml-styler-mcp");

        Assert.True(changed);
        Assert.True(File.Exists($"{configPath}.bak"));

        using var document = JsonDocument.Parse(File.ReadAllText(configPath));
        var server = document.RootElement.GetProperty("mcpServers").GetProperty("xaml-styler");
        Assert.Equal("xaml-styler-mcp", server.GetProperty("command").GetString());
        Assert.Empty(server.GetProperty("args").EnumerateArray());
    }

    [Fact]
    public void Remove_RemovesMcpServerEntry()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var configPath = Path.Combine(temporaryDirectory.Path, "mcp.json");
        var json = """
        {
          "mcpServers": {
            "xaml-styler": {
              "command": "xaml-styler-mcp",
              "args": []
            }
          }
        }
        """;
        File.WriteAllText(configPath, json);

        var changed = new McpJsonConfigurationManager().Remove(configPath, "xaml-styler");

        Assert.True(changed);
        using var document = JsonDocument.Parse(File.ReadAllText(configPath));
        Assert.False(document.RootElement.GetProperty("mcpServers").TryGetProperty("xaml-styler", out _));
    }
}
