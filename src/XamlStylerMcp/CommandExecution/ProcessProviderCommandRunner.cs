using System.ComponentModel;
using System.Diagnostics;

namespace XamlStylerMcp.CommandExecution;

public sealed class ProcessProviderCommandRunner(TextWriter outputWriter, TextWriter errorWriter) : IProviderCommandRunner
{
    private const int CommandNotFoundExitCode = 127;
    private const int FileNotFoundErrorCode = 2;
    private const int PathNotFoundErrorCode = 3;
    private const int StartFailureExitCode = 1;

    public async Task<int> RunAsync(ProviderCommand command, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo(command.FileName)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        foreach (var argument in command.Arguments) processStartInfo.ArgumentList.Add(argument);

        Process? process;
        try
        {
            process = Process.Start(processStartInfo);
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode is FileNotFoundErrorCode or PathNotFoundErrorCode)
        {
            errorWriter.WriteLine($"Provider command '{command.FileName}' was not found. Install the '{command.FileName}' CLI or add it to PATH, then try again.");
            return CommandNotFoundExitCode;
        }
        catch (Win32Exception exception)
        {
            errorWriter.WriteLine($"Failed to start provider command '{command.FileName}': {exception.Message}");
            return StartFailureExitCode;
        }

        if (process is null)
        {
            errorWriter.WriteLine($"Failed to start provider command '{command.FileName}'.");
            return StartFailureExitCode;
        }

        using (process)
        {
            var outputTask = CopyOutputAsync(process.StandardOutput, outputWriter, cancellationToken);
            var errorTask = CopyOutputAsync(process.StandardError, errorWriter, cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(outputTask, errorTask);
            return process.ExitCode;
        }
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
