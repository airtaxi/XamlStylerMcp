using System.Text;
using Xavalon.XamlStyler;
using Xavalon.XamlStyler.Options;

namespace XamlStylerMcp.Formatting;

public sealed class XamlFormattingService
{
    private static readonly string[] s_supportedExtensions = [".xaml", ".axaml"];

    public string FormatXaml(string content, string? configPath = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        var stylerOptions = CreateStylerOptions(configPath);
        var stylerService = new StylerService(stylerOptions, new XamlLanguageOptions { IsFormatable = true });
        return stylerService.StyleDocument(content);
    }

    public XamlFileFormatResult FormatFile(string filePath, string? configPath = null, bool writeChanges = true)
    {
        var fullPath = ValidateSupportedFile(filePath);
        var (content, encoding) = ReadFile(fullPath);
        var effectiveConfigPath = string.IsNullOrWhiteSpace(configPath) ? FindSettingsFile(fullPath) : configPath;
        var formattedContent = FormatXaml(content, effectiveConfigPath);
        var changed = !string.Equals(content, formattedContent, StringComparison.Ordinal);

        if (writeChanges && changed) File.WriteAllText(fullPath, formattedContent, encoding);

        return new XamlFileFormatResult(fullPath, changed, writeChanges && changed, formattedContent);
    }

    public XamlFileCheckResult CheckFile(string filePath, string? configPath = null)
    {
        var result = FormatFile(filePath, configPath, writeChanges: false);
        return new XamlFileCheckResult(result.FilePath, !result.Changed, result.Changed);
    }

    public XamlDirectoryFormatResult FormatDirectory(string directoryPath, string? configPath = null, bool writeChanges = true, bool recursive = false)
    {
        var fullDirectoryPath = ValidateDirectory(directoryPath);
        var files = new List<XamlDirectoryFormatFileResult>();

        foreach (var filePath in EnumerateSupportedFiles(fullDirectoryPath, recursive))
        {
            var result = FormatFile(filePath, configPath, writeChanges);
            files.Add(new XamlDirectoryFormatFileResult(result.FilePath, result.Changed, result.Written));
        }

        return new XamlDirectoryFormatResult(fullDirectoryPath, recursive, files.Count, files.Count(file => file.Changed), files.Count(file => file.Written), files);
    }

    public XamlDirectoryCheckResult CheckDirectory(string directoryPath, string? configPath = null, bool recursive = false)
    {
        var fullDirectoryPath = ValidateDirectory(directoryPath);
        var files = new List<XamlDirectoryCheckFileResult>();

        foreach (var filePath in EnumerateSupportedFiles(fullDirectoryPath, recursive))
        {
            var result = CheckFile(filePath, configPath);
            files.Add(new XamlDirectoryCheckFileResult(result.FilePath, result.IsFormatted, result.WouldChange));
        }

        return new XamlDirectoryCheckResult(fullDirectoryPath, recursive, files.Count, files.Count(file => file.IsFormatted), files.Count(file => file.WouldChange), files);
    }

    public static string? FindSettingsFile(string filePath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));

        while (!string.IsNullOrWhiteSpace(directory))
        {
            var settingsPath = Path.Combine(directory, "Settings.XamlStyler");
            if (File.Exists(settingsPath)) return settingsPath;
            directory = Directory.GetParent(directory)?.FullName;
        }

        return null;
    }

    private static IStylerOptions CreateStylerOptions(string? configPath)
    {
        return string.IsNullOrWhiteSpace(configPath) ? new StylerOptions() : new StylerOptions(configPath);
    }

    private static string ValidateSupportedFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path is required.", nameof(filePath));

        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("XAML file was not found.", fullPath);

        var extension = Path.GetExtension(fullPath);
        if (!s_supportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) throw new NotSupportedException("Only .xaml and .axaml files are supported.");

        return fullPath;
    }

    private static string ValidateDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentException("Directory path is required.", nameof(directoryPath));

        var fullDirectoryPath = Path.GetFullPath(directoryPath);
        if (!Directory.Exists(fullDirectoryPath)) throw new DirectoryNotFoundException($"XAML directory was not found: {fullDirectoryPath}");

        return fullDirectoryPath;
    }

    private static IEnumerable<string> EnumerateSupportedFiles(string fullDirectoryPath, bool recursive)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return s_supportedExtensions
            .SelectMany(extension => Directory.EnumerateFiles(fullDirectoryPath, $"*{extension}", searchOption))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase);
    }

    private static (string Content, Encoding Encoding) ReadFile(string fullPath)
    {
        using var reader = new StreamReader(fullPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var content = reader.ReadToEnd();
        return (content, reader.CurrentEncoding);
    }
}
