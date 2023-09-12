using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Docks;

public class MyDockFactory : Factory
{
    public IRootDock? RootDock { get; set; }
    public MyDocumentDock? DocumentDock { get; set; }

    private void CreateDefaultLayout(IServiceProvider serviceProvider)
    {
        var taggableItemsSearchViewModel = serviceProvider.GetRequiredService<TaggableItemsSearchViewModel>();
        taggableItemsSearchViewModel.Title = "Tag Search";
        // taggableItemsSearchViewModel.CanClose = true;
        var myDocumentDock = new MyDocumentDock(serviceProvider)
        {
            CanCreateDocument = true,
            Proportion = 1.0,
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(taggableItemsSearchViewModel)
        };

        DocumentDock = myDocumentDock;
        RootDock = new RootDock
        {
            Title = "Default",
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(DocumentDock),
            ActiveDockable = DocumentDock,
            DefaultDockable = DocumentDock
        };
    }

    // public sealed override IList<T> CreateList<T>(params T[] items) => base.CreateList(items);

    public override IRootDock CreateLayout() => RootDock ?? throw new ArgumentNullException(nameof(RootDock));

    // public override void InitLayout(IDockable layout)
    // {
    //     DockableLocator = new Dictionary<string, Func<IDockable?>> { ["Root"] = () => RootDock, ["DocumentDock"] = () => DocumentDock };
    //     HostWindowLocator = new Dictionary<string, Func<IHostWindow>> { [nameof(IDockWindow)] = () => new HostWindow() };
    //
    //     base.InitLayout(layout);
    // }
}
