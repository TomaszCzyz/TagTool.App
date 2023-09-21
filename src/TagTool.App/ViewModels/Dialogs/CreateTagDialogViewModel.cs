using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagTool.App.Core;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.ViewModels.Dialogs;

public partial class CreateTagDialogViewModel : ViewModelBase, IDisposable, INotifyDataErrorInfo
{
    private readonly ILogger<CreateTagDialogViewModel> _logger;

    private readonly AsyncDuplexStreamingCall<CanCreateTagRequest, CanCreateTagReply> _streamingCall;

    [ObservableProperty]
    private string? _tagName;

    private string? _valueError;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    [UsedImplicitly]
    public CreateTagDialogViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _logger = AppTemplate.Current.Services.GetRequiredService<ILogger<CreateTagDialogViewModel>>();
        var tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();
        _streamingCall = tagService.CanCreateTag();
    }

    public CreateTagDialogViewModel(ILogger<CreateTagDialogViewModel> logger, ITagToolBackend tagToolBackend)
    {
        _logger = logger;
        var tagService = tagToolBackend.GetTagService();
        _streamingCall = tagService.CanCreateTag();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _streamingCall.Dispose();
    }

    public IEnumerable GetErrors(string? propertyName)
        => propertyName switch
        {
            nameof(TagName) => new[] { _valueError },
            _ => Array.Empty<object?>()
        };

    public bool HasErrors => _valueError is not null;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    partial void OnTagNameChanging(string? value)
        => Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await _streamingCall.RequestStream.WriteAsync(new CanCreateTagRequest { TagName = value });

            if (await _streamingCall.ResponseStream.MoveNext())
            {
                var reply = _streamingCall.ResponseStream.Current;
                switch (reply.ResultCase)
                {
                    case CanCreateTagReply.ResultOneofCase.None:
                        _valueError = null;
                        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(TagName)));
                        break;
                    case CanCreateTagReply.ResultOneofCase.Error:
                        _logger.LogDebug("Receive error reply: {CanCreateTagReply}", reply);

                        _valueError = reply.Error.Message;
                        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(TagName)));
                        break;
                }
            }
        });
}
