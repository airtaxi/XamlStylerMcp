namespace XamlStylerMcp.Formatting;

public sealed record XamlDirectoryCheckResult(string DirectoryPath, bool Recursive, int FileCount, int FormattedCount, int WouldChangeCount, IReadOnlyList<XamlDirectoryCheckFileResult> Files);
