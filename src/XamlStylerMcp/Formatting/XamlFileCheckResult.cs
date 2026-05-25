namespace XamlStylerMcp.Formatting;

public sealed record XamlFileCheckResult(string FilePath, bool IsFormatted, bool WouldChange);
