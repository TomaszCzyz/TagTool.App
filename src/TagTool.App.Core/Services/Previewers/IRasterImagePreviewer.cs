using Avalonia.Media.Imaging;

namespace TagTool.App.Core.Services.Previewers;

public interface IRasterImagePreviewer : IPreviewer
{
    public Bitmap? Preview { get; }

    public double ScalingFactor { get; set; }
}
