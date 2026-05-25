using XamlStylerMcp.CommandExecution;

namespace XamlStylerMcp.Tests;

public sealed class RecordingProviderCommandRunner : IProviderCommandRunner
{
    public List<ProviderCommand> Commands { get; } = [];

    public Queue<int> ExitCodes { get; } = [];

    public Task<int> RunAsync(ProviderCommand command, CancellationToken cancellationToken)
    {
        Commands.Add(command);
        return Task.FromResult(ExitCodes.TryDequeue(out var exitCode) ? exitCode : 0);
    }
}
