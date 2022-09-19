﻿using System.Text;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using TagTool.App.ViewModels.Docks;

namespace TagTool.App.ViewModels;

public class NotepadFactory : Factory
{
    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    public override IDocumentDock CreateDocumentDock() => new FilesDocumentDock();

    public override IRootDock CreateLayout()
    {
        var untitledFileViewModel = new FileViewModel
        {
            Path = string.Empty,
            Title = "Untitled",
            Text = "",
            Encoding = Encoding.Default.WebName
        };

        var documentDock = new FilesDocumentDock
        {
            Id = "Files",
            Title = "Files",
            IsCollapsable = false,
            Proportion = double.NaN,
            ActiveDockable = untitledFileViewModel,
            VisibleDockables = CreateList<IDockable>
            (
                untitledFileViewModel
            ),
            CanCreateDocument = true
        };

        var tools = new ProportionalDock
        {
            Proportion = 0.2,
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>
            (
            )
        };

        var windowLayout = CreateRootDock();
        windowLayout.Title = "Default";
        var windowLayoutContent = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>
            (
                documentDock,
                new ProportionalDockSplitter(),
                tools
            )
        };
        windowLayout.IsCollapsable = false;
        windowLayout.VisibleDockables = CreateList<IDockable>(windowLayoutContent);
        windowLayout.ActiveDockable = windowLayoutContent;

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.VisibleDockables = CreateList<IDockable>(windowLayout);
        rootDock.ActiveDockable = windowLayout;
        rootDock.DefaultDockable = windowLayout;

        _documentDock = documentDock;
        _rootDock = rootDock;

        return rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object>> { ["Find"] = () => layout, ["Replace"] = () => layout };

        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            ["Root"] = () => _rootDock,
            ["Files"] = () => _documentDock
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow>> { [nameof(IDockWindow)] = () => new HostWindow() };

        base.InitLayout(layout);
    }
}
