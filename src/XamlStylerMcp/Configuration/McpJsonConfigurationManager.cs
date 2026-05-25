using System.Text.Json;
using System.Text.Json.Nodes;

namespace XamlStylerMcp.Configuration;

public sealed class McpJsonConfigurationManager
{
    private static readonly JsonSerializerOptions s_writeOptions = new()
    {
        WriteIndented = true,
    };

    private static readonly JsonDocumentOptions s_readOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    public bool Install(string configPath, string serverName, string toolCommandName)
    {
        var fileExisted = File.Exists(configPath);
        var rootObject = LoadRootObject(configPath);
        var serverObject = GetOrCreateServersObject(rootObject);
        var desiredServerObject = new JsonObject
        {
            ["command"] = toolCommandName,
            ["args"] = new JsonArray(),
        };

        if (JsonNode.DeepEquals(serverObject[serverName], desiredServerObject)) return false;

        serverObject[serverName] = desiredServerObject;
        Save(configPath, rootObject, fileExisted);
        return true;
    }

    public bool Remove(string configPath, string serverName)
    {
        if (!File.Exists(configPath)) return false;

        var rootObject = LoadRootObject(configPath);
        if (rootObject["mcpServers"] is not JsonObject serverObject) return false;
        if (!serverObject.Remove(serverName)) return false;

        Save(configPath, rootObject, fileExisted: true);
        return true;
    }

    private static JsonObject LoadRootObject(string configPath)
    {
        if (!File.Exists(configPath)) return [];

        var json = File.ReadAllText(configPath);
        if (string.IsNullOrWhiteSpace(json)) return [];

        var node = JsonNode.Parse(json, documentOptions: s_readOptions);
        return node as JsonObject ?? throw new InvalidOperationException("The MCP configuration root must be a JSON object.");
    }

    private static JsonObject GetOrCreateServersObject(JsonObject rootObject)
    {
        if (rootObject["mcpServers"] is JsonObject existingServerObject) return existingServerObject;
        if (rootObject["mcpServers"] is not null) throw new InvalidOperationException("The 'mcpServers' property must be a JSON object.");

        var serverObject = new JsonObject();
        rootObject["mcpServers"] = serverObject;
        return serverObject;
    }

    private static void Save(string configPath, JsonObject rootObject, bool fileExisted)
    {
        if (fileExisted) File.Copy(configPath, $"{configPath}.bak", overwrite: true);

        var directory = Path.GetDirectoryName(Path.GetFullPath(configPath));
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

        File.WriteAllText(configPath, rootObject.ToJsonString(s_writeOptions) + Environment.NewLine);
    }
}
