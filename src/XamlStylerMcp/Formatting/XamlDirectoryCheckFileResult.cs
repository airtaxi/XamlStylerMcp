namespace XamlStylerMcp.Formatting;

public sealed record XamlDirectoryCheckFileResult(string FilePath, bool IsFormatted, bool WouldChange);
