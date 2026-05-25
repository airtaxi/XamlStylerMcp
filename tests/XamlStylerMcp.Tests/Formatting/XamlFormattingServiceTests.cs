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
    public void FormatFile_RejectsUnsupportedExtension()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var filePath = Path.Combine(temporaryDirectory.Path, "Sample.txt");
        File.WriteAllText(filePath, "<Grid />");

        Assert.Throws<NotSupportedException>(() => new XamlFormattingService().FormatFile(filePath));
    }
}
