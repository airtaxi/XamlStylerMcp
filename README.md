# Format XAML from your agent.

[![NuGet](https://img.shields.io/nuget/v/xaml-styler-mcp.svg)](https://www.nuget.org/packages/xaml-styler-mcp)
[![NuGet downloads](https://img.shields.io/nuget/dt/xaml-styler-mcp.svg)](https://www.nuget.org/packages/xaml-styler-mcp)
[![Pack and Publish](https://github.com/airtaxi/XamlStylerMcp/actions/workflows/pack-and-publish.yml/badge.svg)](https://github.com/airtaxi/XamlStylerMcp/actions/workflows/pack-and-publish.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)

🌐 English | [한국어](README.ko.md)

xaml-styler-mcp is a .NET global tool that exposes [XamlStyler](https://github.com/Xavalon/XamlStyler) through a stdio MCP server. Local coding agents can call it to format XAML strings, format XAML files, or check whether a file would change.

## Highlights

- Format XAML through MCP tools from Codex, Claude Code, GitHub Copilot CLI, Gemini CLI, or any provider that supports stdio MCP servers.
- Register or remove the server with supported provider CLIs using `mcp-install`, `mcp-remove`, or `mcp-uninstall`.
- Edit arbitrary `mcp.json` files directly for providers that do not have a built-in installer command.
- Reuse `Settings.XamlStyler` configuration files and automatically discover them from file paths.
- Preserve file encoding when formatting files on disk.

## Install

```powershell
dotnet tool install --global xaml-styler-mcp
```

Update an existing install:

```powershell
dotnet tool update --global xaml-styler-mcp
```

## Provider Setup

Use a supported provider CLI:

```powershell
xaml-styler-mcp mcp-install --provider codex
xaml-styler-mcp mcp-install --provider claude
xaml-styler-mcp mcp-install --provider copilot
xaml-styler-mcp mcp-install --provider gemini
xaml-styler-mcp mcp-install --provider all
```

Remove the registration:

```powershell
xaml-styler-mcp mcp-remove --provider codex
xaml-styler-mcp mcp-uninstall --provider claude
xaml-styler-mcp mcp-remove --provider all
```

The default MCP server name is `xaml-styler`. Override it when needed:

```powershell
xaml-styler-mcp mcp-install --provider codex --name xaml-styler
```

## Custom mcp.json

For other providers, pass the target MCP config file directly:

```powershell
xaml-styler-mcp mcp-install --config "C:\path\to\mcp.json"
```

This adds:

```json
{
  "mcpServers": {
    "xaml-styler": {
      "command": "xaml-styler-mcp",
      "args": []
    }
  }
}
```

The config editor accepts JSON comments and trailing commas when reading. It writes standard formatted JSON and creates a `.bak` file before modifying an existing config.

## MCP Tools

- `format_xaml`: Formats a XAML string and returns the formatted content.
- `format_xaml_file`: Formats a `.xaml` or `.axaml` file and writes changes by default.
- `check_xaml_file`: Checks whether a `.xaml` or `.axaml` file would change.

Each tool accepts an optional `configPath` pointing to a XamlStyler settings file. File-based tools search parent directories for `Settings.XamlStyler` when `configPath` is omitted.

## Manual Server Command

Running without arguments starts the stdio MCP server:

```powershell
xaml-styler-mcp
```

Show CLI help:

```powershell
xaml-styler-mcp help
xaml-styler-mcp --help
```

## Package Development

```powershell
dotnet restore
dotnet build
dotnet test
dotnet pack
```

## License

xaml-styler-mcp is licensed under the [MIT License](LICENSE.txt).

XAML formatting is powered by [XamlStyler](https://github.com/Xavalon/XamlStyler), licensed under Apache-2.0. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) and [licenses/XamlStyler-Apache-2.0.txt](licenses/XamlStyler-Apache-2.0.txt).

## Author

Created by [Howon Lee (airtaxi)](https://github.com/airtaxi).

Built with help from OpenAI Codex.
