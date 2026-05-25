using XamlStylerMcp.Formatting;

namespace XamlStylerMcp.Tests.Formatting;

public sealed class XamlFormattingServiceTests
{
    [Fact]
    public void FormatXaml_ReturnsFormattedContent()
    {
        var service = new XamlFormattingService();
        var input = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Button Height=\"20\" Width=\"10\"></Button></Grid>";

        var output = service.FormatXaml(input);

        Assert.Contains("<Button", output, StringComparison.Ordinal);
        Assert.NotEqual(input, output);
    }

    [Fact]
    public void CheckFile_DoesNotWriteChanges()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var filePath = Path.Combine(temporaryDirectory.Path, "Sample.xaml");
        var input = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Button Height=\"20\" Width=\"10\"></Button></Grid>";
        File.WriteAllText(filePath, input);

        var result = new XamlFormattingService().CheckFile(filePath);

        Assert.True(result.WouldChange);
        Assert.Equal(input, File.ReadAllText(filePath));
    }

    [Fact]
    public void FormatDirectory_FormatsOnlySupportedTopLevelFilesByDefault()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var nestedDirectoryPath = Path.Combine(temporaryDirectory.Path, "Nested");
        Directory.CreateDirectory(nestedDirectoryPath);
        var input = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Button Height=\"20\" Width=\"10\"></Button></Grid>";
        var xamlPath = Path.Combine(temporaryDirectory.Path, "Sample.xaml");
        var axamlPath = Path.Combine(temporaryDirectory.Path, "Sample.axaml");
        var textPath = Path.Combine(temporaryDirectory.Path, "Sample.txt");
        var nestedXamlPath = Path.Combine(nestedDirectoryPath, "Nested.xaml");
        File.WriteAllText(xamlPath, input);
        File.WriteAllText(axamlPath, input);
        File.WriteAllText(textPath, "not xaml");
        File.WriteAllText(nestedXamlPath, input);

        var result = new XamlFormattingService().FormatDirectory(temporaryDirectory.Path);

        Assert.False(result.Recursive);
        Assert.Equal(2, result.FileCount);
        Assert.Equal([Path.GetFullPath(axamlPath), Path.GetFullPath(xamlPath)], result.Files.Select(file => file.FilePath));
        Assert.Equal("not xaml", File.ReadAllText(textPath));
        Assert.Equal(input, File.ReadAllText(nestedXamlPath));
    }

    [Fact]
    public void CheckDirectory_WhenRecursiveIncludesNestedSupportedFiles()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var nestedDirectoryPath = Path.Combine(temporaryDirectory.Path, "Nested");
        Directory.CreateDirectory(nestedDirectoryPath);
        var input = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Button Height=\"20\" Width=\"10\"></Button></Grid>";
        var xamlPath = Path.Combine(temporaryDirectory.Path, "Sample.xaml");
        var axamlPath = Path.Combine(nestedDirectoryPath, "Sample.axaml");
        var textPath = Path.Combine(nestedDirectoryPath, "Sample.txt");
        var buildOutputPath = Path.Combine(nestedDirectoryPath, "Sample.dll");
        File.WriteAllText(xamlPath, input);
        File.WriteAllText(axamlPath, input);
        File.WriteAllText(textPath, "not xaml");
        File.WriteAllText(buildOutputPath, "not xaml");

        var result = new XamlFormattingService().CheckDirectory(temporaryDirectory.Path, recursive: true);

        Assert.True(result.Recursive);
        Assert.Equal(2, result.FileCount);
        Assert.Equal([Path.GetFullPath(axamlPath), Path.GetFullPath(xamlPath)], result.Files.Select(file => file.FilePath));
        Assert.Equal(2, result.WouldChangeCount);
    }

    [Fact]
    public void FormatFile_RejectsUnsupportedExtension()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var filePath = Path.Combine(temporaryDirectory.Path, "Sample.txt");
        File.WriteAllText(filePath, "<Grid />");

        Assert.Throws<NotSupportedException>(() => new XamlFormattingService().FormatFile(filePath));
    }
}
