using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TagTool.App.Lite.ViewModels;

namespace TagTool.App.Lite;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? param)
    {
        if (param is null)
        {
            return new TextBlock { Text = "Not Found, because param was null" };
        }

        var name = param.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            File.WriteAllText(@"C:\Users\tczyz\Documents\TagToolApp\fromViewLocator.txt", "");

            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
