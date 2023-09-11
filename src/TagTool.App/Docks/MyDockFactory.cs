using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Docks;

public class MyDockFactory : Factory
{
    private readonly IRootDock _rootDock;
    public MyDocumentDock DocumentDock { get; }

    public MyDockFactory(IServiceProvider serviceProvider)
    {
        var taggableItemsSearchViewModel = serviceProvider.GetRequiredService<TaggableItemsSearchViewModel>();
        taggableItemsSearchViewModel.Title = "Tag Search";
        taggableItemsSearchViewModel.CanClose = true;
        var myDocumentDock = new MyDocumentDock(serviceProvider)
        {
            CanCreateDocument = true,
            Proportion = 1.0,
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(taggableItemsSearchViewModel)
        };

        _rootDock = new RootDock
        {
            Title = "Default",
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(myDocumentDock),
            ActiveDockable = DocumentDock,
            DefaultDockable = DocumentDock
        };
        DocumentDock = myDocumentDock;
    }

    public sealed override IList<T> CreateList<T>(params T[] items) => base.CreateList(items);

    public override IRootDock CreateLayout() => _rootDock;

    public override void InitLayout(IDockable layout)
    {
        DockableLocator = new Dictionary<string, Func<IDockable?>> { ["Root"] = () => _rootDock, ["DocumentDock"] = () => DocumentDock };
        // HostWindowLocator = new Dictionary<string, Func<IHostWindow>> { [nameof(IDockWindow)] = () => new HostWindow() };

        base.InitLayout(layout);
    }
}
