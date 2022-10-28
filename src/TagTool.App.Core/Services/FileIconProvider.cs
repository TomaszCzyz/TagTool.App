using System.Drawing;
using System.Runtime.InteropServices;

namespace TagTool.App.Core.Services;

public interface IFileIconProvider
{
    Icon? GetFileIcon(string filePath);
}

public class DefaultFileIconProvider : IFileIconProvider
{
    public Icon? GetFileIcon(string filePath)
    {
        return !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? null
            : Icon.ExtractAssociatedIcon(filePath);
    }
}
