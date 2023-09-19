using Dock.Model.Controls;
using Dock.Model.Mvvm;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace TagTool.App.Docks;

[UsedImplicitly]
public class DefaultDockFactory : Factory
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultDockFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public override IDocumentDock CreateDocumentDock()
    {
        return _serviceProvider.GetRequiredService<MyDocumentDock>();
    }

    // public override void InitLayout(IDockable layout)
    // {
    //     DockableLocator = new Dictionary<string, Func<IDockable?>> { ["Root"] = () => RootDock, ["DocumentDock"] = () => DocumentDock };
    //     HostWindowLocator = new Dictionary<string, Func<IHostWindow>> { [nameof(IDockWindow)] = () => new HostWindow() };
    //
    //     base.InitLayout(layout);
    // }
}
