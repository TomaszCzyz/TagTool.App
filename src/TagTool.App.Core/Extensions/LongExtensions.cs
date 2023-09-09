using System.Globalization;

namespace TagTool.App.Core.Extensions;

public static class LongExtensions
{
    public static string GetBytesReadable(this long i, string format)
    {
        i = i < 0 ? -i : i;

        string suffix;
        double readable;
        switch (i)
        {
            // Exabyte
            case >= 0x1000000000000000:
                suffix = "EB";
                readable = i >> 50;
                break;
            // Petabyte
            case >= 0x4000000000000:
                suffix = "PB";
                readable = i >> 40;
                break;
            // Terabyte
            case >= 0x10000000000:
                suffix = "TB";
                readable = i >> 30;
                break;
            // Gigabyte
            case >= 0x40000000:
                suffix = "GB";
                readable = i >> 20;
                break;
            // Megabyte
            case >= 0x100000:
                suffix = "MB";
                readable = i >> 10;
                break;
            // Kilobyte
            case >= 0x400:
                suffix = "KB";
                readable = i;
                break;
            default:
                return i.ToString("0 B", CultureInfo.CurrentCulture); // Byte
        }

        readable /= 1024;
        return $"{readable.ToString(format, CultureInfo.CurrentCulture)} {suffix}";
    }
}
