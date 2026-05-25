using System.Diagnostics;

namespace XamlStylerMcp.CommandExecution;

public sealed class ProcessProviderCommandRunner(TextWriter outputWriter, TextWriter errorWriter) : IProviderCommandRunner
{
    public async Task<int> RunAsync(ProviderCommand command, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo(command.FileName)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        foreach (var argument in command.Arguments) processStartInfo.ArgumentList.Add(argument);

        using var process = Process.Start(processStartInfo) ?? throw new InvalidOperationException($"Failed to start '{command.FileName}'.");

        var outputTask = CopyOutputAsync(process.StandardOutput, outputWriter, cancellationToken);
        var errorTask = CopyOutputAsync(process.StandardError, errorWriter, cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        await Task.WhenAll(outputTask, errorTask);
        return process.ExitCode;
    }

    private static async Task CopyOutputAsync(TextReader reader, TextWriter writer, CancellationToken cancellationToken)
    {
        var buffer = new char[4096];

        while (true)
        {
            var readCount = await reader.ReadAsync(buffer, cancellationToken);
            if (readCount == 0) return;
            await writer.WriteAsync(buffer.AsMemory(0, readCount), cancellationToken);
            await writer.FlushAsync(cancellationToken);
        }
    }
}
