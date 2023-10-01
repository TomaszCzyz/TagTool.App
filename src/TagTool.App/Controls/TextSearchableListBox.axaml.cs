using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using TagTool.App.Core.ViewModels;

namespace TagTool.App.Controls;

public class TextSearchableListBox : ListBox
{
    private string _textSearchTerm = string.Empty;
    private DispatcherTimer? _textSearchTimer;

    /// <inheritdoc />
    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (e.Handled || !IsTextSearchEnabled)
        {
            base.OnTextInput(e);
            return;
        }

        StopTextSearchTimer();

        _textSearchTerm += e.Text;

        var item = Items
            .OfType<ITextSearchable>()
            .FirstOrDefault(searchable => searchable.SearchText.StartsWith(_textSearchTerm, StringComparison.OrdinalIgnoreCase));

        if (item is not null)
        {
            SelectedIndex = Items.IndexOf(item);
        }

        StartTextSearchTimer();

        e.Handled = true;
        base.OnTextInput(e);
    }

    private void StartTextSearchTimer()
    {
        _textSearchTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _textSearchTimer.Tick += TextSearchTimer_Tick;
        _textSearchTimer.Start();
    }

    private void StopTextSearchTimer()
    {
        if (_textSearchTimer == null)
        {
            return;
        }

        _textSearchTimer.Tick -= TextSearchTimer_Tick;
        _textSearchTimer.Stop();

        _textSearchTimer = null;
    }

    private void TextSearchTimer_Tick(object? sender, EventArgs e)
    {
        _textSearchTerm = string.Empty;
        StopTextSearchTimer();
    }
}
