using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.DataTemplates;

public static class TagsTemplateProvider
{
    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    public static FuncDataTemplate<Tag> TagDataTemplate { get; } = new(_ => true, BuildTagPresenter);

    private static Control BuildTagPresenter(Tag tag)
    {
        var resourceName = $"{tag.GetType().Name}Color";

        var color = Application.Current!.TryGetResource(resourceName, null, out var resource)
            ? (Color)resource!
            : (Color)Application.Current.Resources["DefaultTagColor"]!;

        var textBlock = new TextBlock
        {
            Foreground = new SolidColorBrush(color)
            // [!TextBlock.TextProperty] = new Binding("DisplayText"),
            // [!TextBlock.TextDecorationsProperty] = new Binding("$parent[Grid].DataContext.State") { Converter = new TextDecorationsConverter() }, // todo: get it from resources
        };

        return textBlock;
    }
}
