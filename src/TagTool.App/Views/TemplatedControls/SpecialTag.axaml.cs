using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using JetBrains.Annotations;

namespace TagTool.App.Views.TemplatedControls;

[TemplatePart(Name = DeleteButtonPartName, Type = typeof(Button))]
public class SpecialTag : TemplatedControl
{
    [UsedImplicitly]
    public const string DeleteButtonPartName = "PART_DeleteButton";

    public static readonly StyledProperty<string> SpecialTagNameProperty
        = AvaloniaProperty.Register<SpecialTag, string>(nameof(SpecialTagName));

    public static readonly StyledProperty<string> TextProperty
        = AvaloniaProperty.Register<SpecialTag, string>(nameof(Text));

    public static readonly StyledProperty<IBrush> ClipRectangleFillProperty
        = AvaloniaProperty.Register<SpecialTag, IBrush>(nameof(ClipRectangleFill));

    public static readonly StyledProperty<bool> IsDeletableProperty
        = AvaloniaProperty.Register<TagChip, bool>(nameof(IsDeletable), true);

    public static readonly StyledProperty<ICommand?> DeleteCommandProperty
        = AvaloniaProperty.Register<TagChip, ICommand?>(nameof(DeleteCommand));

    public static readonly StyledProperty<object?> DeleteCommandParameterProperty
        = AvaloniaProperty.Register<TagChip, object?>(nameof(DeleteCommandParameter));

    public static readonly RoutedEvent<RoutedEventArgs> DeleteClickEvent
        = RoutedEvent.Register<TagChip, RoutedEventArgs>(nameof(DeleteClick), RoutingStrategies.Bubble);

    private Button? _deleteButton;

    public IBrush ClipRectangleFill
    {
        get => GetValue(ClipRectangleFillProperty);
        set => SetValue(ClipRectangleFillProperty, value);
    }

    public bool IsDeletable
    {
        get => GetValue(IsDeletableProperty);
        set => SetValue(IsDeletableProperty, value);
    }

    public ICommand? DeleteCommand
    {
        get => GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public object? DeleteCommandParameter
    {
        get => GetValue(DeleteCommandParameterProperty);
        set => SetValue(DeleteCommandParameterProperty, value);
    }

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

    public event EventHandler<RoutedEventArgs> DeleteClick
    {
        add => AddHandler(DeleteClickEvent, value);
        remove => RemoveHandler(DeleteClickEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_deleteButton != null)
        {
            _deleteButton.Click -= DeleteButtonOnClick;
        }

        _deleteButton = this.FindDescendantOfType<Button>();

        if (_deleteButton != null)
        {
            _deleteButton.Click += DeleteButtonOnClick;
        }

        base.OnApplyTemplate(e);
    }

    private void OnDeleteClick()
    {
        RaiseEvent(new RoutedEventArgs(DeleteClickEvent, this));

        if (DeleteCommand?.CanExecute(DeleteCommandParameter) ?? false)
        {
            DeleteCommand.Execute(DeleteCommandParameter);
        }
    }

    private void DeleteButtonOnClick(object? sender, RoutedEventArgs routedEventArgs)
    {
        OnDeleteClick();
        routedEventArgs.Handled = true;
    }
}
