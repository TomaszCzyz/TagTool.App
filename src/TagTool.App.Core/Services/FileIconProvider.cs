using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace TagTool.App.Core.Services;

public interface IFileIconProvider
{
    Bitmap? GetFileIcon(string filePath, int length);
}

public class DefaultFileIconProvider : IFileIconProvider
{
    private readonly string? _themeName;

    public DefaultFileIconProvider()
    {
        // todo: is linux AND is gnome
        if (OperatingSystem.IsLinux())
        {
            using var process = new Process();
            process.StartInfo.FileName = "gsettings";
            process.StartInfo.Arguments = "get org.gnome.desktop.interface icon-theme";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            var reader = process.StandardOutput;
            var output = reader.ReadToEnd();

            _themeName = string.IsNullOrWhiteSpace(output) ? null : _themeName = output.Trim('\'', '\"');

            process.WaitForExit();
        }
    }

    public Bitmap? GetFileIcon(string filePath, int length)
    {
        if (!Path.Exists(filePath))
        {
            return null;
        }

        if (OperatingSystem.IsWindows())
        {
            return GetIconOnWindows(filePath, length);
        }

        return null;
    }

    [SupportedOSPlatform("windows")] // The platform guard attributes used
    private static Bitmap? GetIconOnWindows(string filePath, int length)
    {
        if (Icon.ExtractAssociatedIcon(filePath) is not { } icon)
        {
            return null;
        }

        using var memoryStream = new MemoryStream();
        icon.Save(memoryStream);
        memoryStream.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        return length == default ? new Bitmap(memoryStream) : Bitmap.DecodeToHeight(memoryStream, length);
    }
}
