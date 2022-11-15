using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace TagTool.App.Views.TemplatedControls;

[TemplatePart(Name = DeleteButtonPartName, Type = typeof(Button))]
public sealed class TagChip : TemplatedControl
{
    public const string DeleteButtonPartName = "PART_DeleteButton";

    private Button? _deleteButton;

    public static readonly StyledProperty<InlineCollection?> InlinesProperty
        = AvaloniaProperty.Register<TagChip, InlineCollection?>(nameof(Inlines));

    public static readonly StyledProperty<string?> TextProperty
        = AvaloniaProperty.Register<TagChip, string?>(nameof(Text), "DefaultTagText");

    /// <summary>
    /// Indicates if the delete button should be visible.
    /// </summary>
    public static readonly StyledProperty<bool> IsDeletableProperty =
        AvaloniaProperty.Register<TagChip, bool>(nameof(IsDeletable), true);

    public static readonly StyledProperty<ICommand?> DeleteCommandProperty =
        AvaloniaProperty.Register<TagChip, ICommand?>(nameof(DeleteCommand));

    public static readonly StyledProperty<object?> DeleteCommandParameterProperty = AvaloniaProperty.Register<TagChip, object?>(
        nameof(DeleteCommandParameter));

    public InlineCollection? Inlines
    {
        get => GetValue(InlinesProperty);
        set => SetValue(InlinesProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    public event EventHandler<RoutedEventArgs> DeleteClick
    {
        add => AddHandler(DeleteClickEvent, value);
        remove => RemoveHandler(DeleteClickEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> DeleteClickEvent
        = RoutedEvent.Register<TagChip, RoutedEventArgs>(nameof(DeleteClick), RoutingStrategies.Bubble);

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
