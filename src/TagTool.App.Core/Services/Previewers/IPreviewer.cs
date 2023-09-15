using System.ComponentModel;
using Avalonia;

namespace TagTool.App.Core.Services.Previewers;

public record PreviewSize(Size? MonitorSize, bool UseEffectivePixels = false);

public interface IPreviewer : INotifyPropertyChanged
{
    PreviewState State { get; set; }

    static bool IsFileTypeSupported(string fileExt) => throw new NotImplementedException();

    Task<PreviewSize> GetPreviewSizeAsync(CancellationToken cancellationToken);

    Task LoadPreviewAsync(CancellationToken cancellationToken);
}

public enum PreviewState
{
    Uninitialized = 0,
    Loading = 1,
    Loaded = 2,
    Error = 3
}
