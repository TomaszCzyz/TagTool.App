using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.Services.Previewers;

public class PreviewerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PreviewerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPreviewer Create(TaggableItem item)
    {
        if (item is TaggableFile file && RasterImagePreviewerViewModel.IsFileTypeSupported(Path.GetExtension(file.Path)))
        {
            var rasterImagePreviewerViewModel = _serviceProvider.GetRequiredService<RasterImagePreviewerViewModel>();
            rasterImagePreviewerViewModel.Item = file;
            return rasterImagePreviewerViewModel;
        }

        return new UnsupportedFilePreviewer(item);
    }
}
