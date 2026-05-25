using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using XamlStylerMcp.Cli;
using XamlStylerMcp.CommandExecution;
using XamlStylerMcp.Configuration;

namespace XamlStylerMcp;

public static class XamlStylerMcpApplication
{
    private static readonly McpProvider[] s_supportedProviders = Enum.GetValues<McpProvider>();

    public static async Task<int> RunAsync(string[] args, IProviderCommandRunner? providerCommandRunner = null, TextWriter? outputWriter = null, TextWriter? errorWriter = null, CancellationToken cancellationToken = default)
    {
        outputWriter ??= Console.Out;
        errorWriter ??= Console.Error;
        providerCommandRunner ??= new ProcessProviderCommandRunner(outputWriter, errorWriter);

        if (args.Length == 0) return await RunMcpServerAsync(cancellationToken);
        if (args is ["help"] or ["--help"] or ["-h"])
        {
            outputWriter.WriteLine(HelpText.Create());
            return 0;
        }

        var rootCommand = CreateRootCommand(providerCommandRunner, outputWriter, errorWriter);
        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync(new InvocationConfiguration { Output = outputWriter, Error = errorWriter }, cancellationToken);
    }

    private static RootCommand CreateRootCommand(IProviderCommandRunner providerCommandRunner, TextWriter outputWriter, TextWriter errorWriter)
    {
        var rootCommand = new RootCommand("Runs the XAML Styler MCP server or manages provider MCP registration.");

        var helpCommand = new Command("help", "Show help.");
        helpCommand.SetAction(_ => WriteHelp(outputWriter));

        var installCommand = CreateMcpConfigurationCommand("mcp-install", "Install the MCP server.", isInstall: true, providerCommandRunner, outputWriter, errorWriter);
        var removeCommand = CreateMcpConfigurationCommand("mcp-remove", "Remove the MCP server.", isInstall: false, providerCommandRunner, outputWriter, errorWriter);
        removeCommand.Aliases.Add("mcp-uninstall");

        rootCommand.Subcommands.Add(helpCommand);
        rootCommand.Subcommands.Add(installCommand);
        rootCommand.Subcommands.Add(removeCommand);
        return rootCommand;
    }

    private static Command CreateMcpConfigurationCommand(string name, string description, bool isInstall, IProviderCommandRunner providerCommandRunner, TextWriter outputWriter, TextWriter errorWriter)
    {
        var providerOption = new Option<string?>("--provider")
        {
            Description = "Provider CLI to use: codex, claude, copilot, gemini, or all.",
        };

        var configOption = new Option<string?>("--config")
        {
            Description = "Path to an mcp.json file to edit directly.",
        };

        var nameOption = new Option<string>("--name")
        {
            Description = "MCP server name.",
            DefaultValueFactory = _ => XamlStylerMcpConstants.DefaultServerName,
        };

        var command = new Command(name, description);
        command.Options.Add(providerOption);
        command.Options.Add(configOption);
        command.Options.Add(nameOption);
        command.SetAction((parseResult, cancellationToken) => ExecuteMcpConfigurationCommandAsync(isInstall, parseResult.GetValue(providerOption), parseResult.GetValue(configOption), parseResult.GetValue(nameOption) ?? XamlStylerMcpConstants.DefaultServerName, providerCommandRunner, outputWriter, errorWriter, cancellationToken));

        return command;
    }

    private static int WriteHelp(TextWriter outputWriter)
    {
        outputWriter.WriteLine(HelpText.Create());
        return 0;
    }

    private static async Task<int> ExecuteMcpConfigurationCommandAsync(bool isInstall, string? providerValue, string? configPath, string serverName, IProviderCommandRunner providerCommandRunner, TextWriter outputWriter, TextWriter errorWriter, CancellationToken cancellationToken)
    {
        var hasProvider = !string.IsNullOrWhiteSpace(providerValue);
        var hasConfig = !string.IsNullOrWhiteSpace(configPath);

        if (hasProvider == hasConfig)
        {
            errorWriter.WriteLine("Specify exactly one of --provider or --config.");
            return 2;
        }

        if (hasProvider)
        {
            if (IsAllProviders(providerValue)) return await ExecuteAllProviderCommandsAsync(isInstall, serverName, providerCommandRunner, outputWriter, cancellationToken);

            if (!TryParseProvider(providerValue, out var provider))
            {
                errorWriter.WriteLine($"Unsupported provider '{providerValue}'. Supported providers: codex, claude, copilot, gemini, all.");
                return 2;
            }

            return await ExecuteProviderCommandAsync(isInstall, provider, serverName, providerCommandRunner, outputWriter, cancellationToken);
        }

        var configurationManager = new McpJsonConfigurationManager();
        var changed = isInstall ? configurationManager.Install(configPath!, serverName, XamlStylerMcpConstants.ToolCommandName) : configurationManager.Remove(configPath!, serverName);
        outputWriter.WriteLine(changed ? $"{(isInstall ? "Installed" : "Removed")} '{serverName}' in {Path.GetFullPath(configPath!)}." : $"No changes were needed for '{serverName}' in {Path.GetFullPath(configPath!)}.");
        return 0;
    }

    private static async Task<int> ExecuteAllProviderCommandsAsync(bool isInstall, string serverName, IProviderCommandRunner providerCommandRunner, TextWriter outputWriter, CancellationToken cancellationToken)
    {
        var results = new List<ProviderCommandResult>();

        foreach (var provider in s_supportedProviders)
        {
            var exitCode = await ExecuteProviderCommandAsync(isInstall, provider, serverName, providerCommandRunner, outputWriter, cancellationToken);
            results.Add(new ProviderCommandResult(provider, exitCode));
        }

        outputWriter.WriteLine("Provider results:");

        var hasFailure = false;
        foreach (var result in results)
        {
            if (result.ExitCode == 0)
            {
                outputWriter.WriteLine($"  {FormatProviderName(result.Provider)}: success");
                continue;
            }

            hasFailure = true;
            outputWriter.WriteLine($"  {FormatProviderName(result.Provider)}: failed with exit code {result.ExitCode}");
        }

        return hasFailure ? 1 : 0;
    }

    private static async Task<int> ExecuteProviderCommandAsync(bool isInstall, McpProvider provider, string serverName, IProviderCommandRunner providerCommandRunner, TextWriter outputWriter, CancellationToken cancellationToken)
    {
        var providerCommand = isInstall ? ProviderCommandBuilder.CreateInstallCommand(provider, serverName, XamlStylerMcpConstants.ToolCommandName) : ProviderCommandBuilder.CreateRemoveCommand(provider, serverName);
        outputWriter.WriteLine($"Running: {FormatProviderCommand(providerCommand)}");
        return await providerCommandRunner.RunAsync(providerCommand, cancellationToken);
    }

    private static bool IsAllProviders(string? value)
    {
        return string.Equals(value?.Trim(), "all", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseProvider(string? value, out McpProvider provider)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "codex":
                provider = McpProvider.Codex;
                return true;
            case "claude":
            case "claude-code":
                provider = McpProvider.Claude;
                return true;
            case "copilot":
            case "github-copilot":
                provider = McpProvider.Copilot;
                return true;
            case "gemini":
                provider = McpProvider.Gemini;
                return true;
            default:
                provider = default;
                return false;
        }
    }

    private static string FormatProviderCommand(ProviderCommand command)
    {
        return string.Join(" ", [command.FileName, .. command.Arguments.Select(QuoteArgument)]);
    }

    private static string FormatProviderName(McpProvider provider)
    {
        return provider.ToString().ToLowerInvariant();
    }

    private static string QuoteArgument(string argument)
    {
        return argument.Contains(' ', StringComparison.Ordinal) ? $"\"{argument.Replace("\"", "\\\"", StringComparison.Ordinal)}\"" : argument;
    }

    private static async Task<int> RunMcpServerAsync(CancellationToken cancellationToken)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync(cancellationToken);
        return 0;
    }

    private sealed record ProviderCommandResult(McpProvider Provider, int ExitCode);
}
