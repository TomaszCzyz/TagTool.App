﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using TagTool.App.Contracts;

namespace TagTool.App.DataTemplates;

public static class TagsTemplateProvider
{
    public static FuncDataTemplate<Tag?> TagDataTemplate { get; } = new(tag => tag.HasValue, BuildTagPresenter);

    private static TextBlock BuildTagPresenter(Tag? tag)
    {
        var resourceName = $"{tag?.GetType().Name}Color";

        var color = Application.Current!.TryGetResource(resourceName, null, out var resource)
            ? (Color)resource!
            : new Color(235, 235, 235, 235);
        // (Color)Application.Current.Resources["DefaultTagColor"]!;

        var textBlock = new TextBlock
        {
            Foreground = new SolidColorBrush(color)
            // [!TextBlock.TextProperty] = new Binding("DisplayText"),
            // [!TextBlock.TextDecorationsProperty] = new Binding("$parent[Grid].DataContext.State") { Converter = new TextDecorationsConverter() }, // todo: get it from resources
        };

        return textBlock;
    }
}
