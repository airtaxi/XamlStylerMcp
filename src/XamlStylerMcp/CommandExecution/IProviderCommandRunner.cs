namespace XamlStylerMcp.CommandExecution;

public interface IProviderCommandRunner
{
    Task<int> RunAsync(ProviderCommand command, CancellationToken cancellationToken);
}
