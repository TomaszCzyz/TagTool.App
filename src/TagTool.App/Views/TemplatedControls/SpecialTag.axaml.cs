using Avalonia;
using Avalonia.Controls.Primitives;

namespace TagTool.App.Views.TemplatedControls;

public class SpecialTag : TemplatedControl
{
    public static readonly StyledProperty<string> SpecialTagNameProperty
        = AvaloniaProperty.Register<SpecialTag, string>(nameof(SpecialTagName));

    public static readonly StyledProperty<string> TextProperty
        = AvaloniaProperty.Register<SpecialTag, string>(nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string SpecialTagName
    {
        get => GetValue(SpecialTagNameProperty);
        set => SetValue(SpecialTagNameProperty, value);
    }
}

