using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using TagTool.App.Lite.Models;

namespace TagTool.App.Lite.DataTemplates;

public class TagsTemplateSelector : IDataTemplate
{
    [Content]
    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new();

    public Control? Build(object? param)
    {
        var key = param?.ToString() ?? throw new ArgumentNullException(nameof(param));

        return AvailableTemplates[key].Build(param);
    }

    public bool Match(object? data)
    {
        var key = data?.ToString();

        return data is ITag && !string.IsNullOrEmpty(key) && AvailableTemplates.ContainsKey(key);
    }
}
