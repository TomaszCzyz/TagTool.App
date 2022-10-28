using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using TagTool.App.ViewModels;

namespace TagTool.App;

public class ViewLocator : IDataTemplate
{
    public IControl Build(object? param)
    {
        if (param is null)
        {
            return new TextBlock { Text = "Not Found, because param was null" };
        }

        var name = param.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase or IDockable;
    }
}
