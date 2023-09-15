using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services.Previewers;

namespace TagTool.App.Core.ViewModels;

public partial class TaggableItemPreviewerViewModel : ViewModelBase, IDisposable
{
    private readonly PreviewerFactory _previewerFactory;

    private CancellationTokenSource _cancellationTokenSource = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImagePreviewer))]
    [NotifyPropertyChangedFor(nameof(IsImagePreviewVisible))]
    [NotifyPropertyChangedFor(nameof(IsPreviewLoading))]
    [NotifyPropertyChangedFor(nameof(UnsupportedFilePreviewer))]
    [NotifyPropertyChangedFor(nameof(IsUnsupportedPreviewVisible))]
    private IPreviewer? _previewer;

    [ObservableProperty]
    private TaggableItem? _item;

    [ObservableProperty]
    private double _scalingFactor;

    public IRasterImagePreviewer? ImagePreviewer => Previewer as IRasterImagePreviewer;

    public IUnsupportedFilePreviewer? UnsupportedFilePreviewer => Previewer as IUnsupportedFilePreviewer;

    public bool IsPreviewLoading => Previewer?.State == PreviewState.Loading;

    public bool IsImagePreviewVisible => ImagePreviewer is { Preview: not null, State: PreviewState.Loaded };

    public bool IsUnsupportedPreviewVisible => UnsupportedFilePreviewer is { Preview: not null, State: PreviewState.Loaded or PreviewState.Error };

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public TaggableItemPreviewerViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _previewerFactory = null!;
    }

    [UsedImplicitly]
    public TaggableItemPreviewerViewModel(PreviewerFactory previewerFactory)
    {
        _previewerFactory = previewerFactory;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _cancellationTokenSource.Dispose();
    }

    partial void OnPreviewerChanging(IPreviewer? value)
    {
        // ImagePreview.Source = null;

        if (Previewer != null)
        {
            Previewer.PropertyChanged -= PreviewerErrorState_PropertyChanged;
        }

        if (value != null)
        {
            value.PropertyChanged += PreviewerErrorState_PropertyChanged;
        }
    }

    private async void PreviewerErrorState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Fallback on DefaultPreviewer if we fail to load the correct Preview
        if (e.PropertyName == nameof(IPreviewer.State) && Previewer?.State == PreviewState.Error)
        {
            // Cancel previous loading task
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (Previewer is not IUnsupportedFilePreviewer)
            {
                // Previewer = _previewerFactory.CreateDefaultPreviewer(Item);
                throw new NotImplementedException();
                await UpdatePreviewAsync(_cancellationTokenSource.Token);
            }
        }
    }

    partial void OnScalingFactorChanged(double value)
    {
        if (Previewer is IRasterImagePreviewer imagePreviewer)
        {
            imagePreviewer.ScalingFactor = ScalingFactor;
        }

        // Dispatcher.UIThread.InvokeAsync(async () => await UpdatePreviewSizeAsync(_cancellationTokenSource.Token));
    }

    partial void OnItemChanged(TaggableItem? oldValue, TaggableItem? newValue)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        if (Item == null)
        {
            Previewer = null;
            return;
        }

        Previewer = _previewerFactory.Create(Item);

        Dispatcher.UIThread.InvokeAsync(async () => await UpdatePreviewAsync(_cancellationTokenSource.Token));
    }

    private async Task UpdatePreviewAsync(CancellationToken cancellationToken)
    {
        if (Previewer is null)
        {
            return;
        }

        try
        {
            // cancellationToken.ThrowIfCancellationRequested();
            // await UpdatePreviewSizeAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await Previewer.LoadPreviewAsync(cancellationToken);

            // cancellationToken.ThrowIfCancellationRequested();
            // await UpdateImageTooltipAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // TODO: Log task cancelled exception?
        }
        catch
        {
            // Fall back to Default previewer
            // Logger.LogError("Error in UpdatePreviewAsync, falling back to default previewer: " + ex.Message);
            Previewer.State = PreviewState.Error;
        }
    }
}
