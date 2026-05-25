namespace XamlStylerMcp.Formatting;

public sealed record XamlDirectoryFormatResult(string DirectoryPath, bool Recursive, int FileCount, int ChangedCount, int WrittenCount, IReadOnlyList<XamlDirectoryFormatFileResult> Files);
