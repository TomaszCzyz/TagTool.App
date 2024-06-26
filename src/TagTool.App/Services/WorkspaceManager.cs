﻿using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagTool.App.Docks;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Services;

public interface IWorkspaceManager
{
    (IDock, IFactory) GetLayout();

    Task SaveLayout(RootDock layout);
}

public class WorkspaceManager : IWorkspaceManager
{
    private readonly ILogger<WorkspaceManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly DockSerializer _serializer;

    public WorkspaceManager(ILogger<WorkspaceManager> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        // todo: register DockSerializer and inject it.
        _serializer = new DockSerializer(serviceProvider);
    }

    public (IDock, IFactory) GetLayout()
        => TryGetLayout(out var rootDock, out var dockFactory)
            ? (rootDock, dockFactory)
            : CreateDefaultLayout();

    private bool TryGetLayout([NotNullWhen(true)] out IDock? rootDock, [NotNullWhen(true)] out IFactory? dockFactory)
    {
        _logger.LogDebug("Trying to read layout from file");
        if (!File.Exists(@".\layout.json"))
        {
            (rootDock, dockFactory) = (null, null);
            return false;
        }

        var jsonRootDock = File.ReadAllText(@".\layout.json");

        if (string.IsNullOrEmpty(jsonRootDock))
        {
            (rootDock, dockFactory) = (null, null);
            return false;
        }

        try
        {
            rootDock = _serializer.Deserialize<RootDock>(jsonRootDock);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error during deserialization of dock layout {LayoutString}", jsonRootDock);
            (rootDock, dockFactory) = (null, null);
            return false;
        }

        if (rootDock is null)
        {
            (rootDock, dockFactory) = (null, null);
            return false;
        }

        var defaultDockFactory = _serviceProvider.GetRequiredService<DefaultDockFactory>();
        rootDock.Factory = defaultDockFactory;

        dockFactory = defaultDockFactory;
        return true;
    }

    private (IDock, IFactory) CreateDefaultLayout()
    {
        _logger.LogDebug("Creating default layout");

        var taggableItemsSearch = _serviceProvider.GetRequiredService<TaggableItemsSearchViewModel>();
        var myTags = _serviceProvider.GetRequiredService<TagsLibraryViewModel>();
        taggableItemsSearch.Title = "Tag Search";
        myTags.Title = "Tags Library";

        var documentDock1 = new MyDocumentDock(_serviceProvider)
        {
            CanCreateDocument = true,
            Proportion = 0.25,
            IsCollapsable = false,
            VisibleDockables = new ObservableCollection<IDockable>(new IDockable[] { myTags })
        };

        var documentDock2 = new MyDocumentDock(_serviceProvider)
        {
            CanCreateDocument = true,
            IsCollapsable = false,
            VisibleDockables = new ObservableCollection<IDockable>(new IDockable[] { taggableItemsSearch })
        };

        var proportionalDock = new ProportionalDock
        {
            IsCollapsable = false,
            Orientation = Orientation.Horizontal,
            ActiveDockable = null,
            VisibleDockables
                = new ObservableCollection<IDockable>(new IDockable[] { documentDock1, new ProportionalDockSplitter(), documentDock2 })
        };
        var defaultDockFactory = _serviceProvider.GetRequiredService<DefaultDockFactory>();

        var rootDock = new RootDock
        {
            Title = "Default",
            IsCollapsable = false,
            Factory = defaultDockFactory,
            VisibleDockables = new ObservableCollection<IDockable>(new IDockable[] { proportionalDock }),
            // Without setting ActiveDockable, nothing appears. Maybe it is also important that DocumentDocks are
            // wrapped by ProportionalDock, because it specifies: [DataContract(IsReference = true)]
            ActiveDockable = proportionalDock,
            DefaultDockable = null
        };

        rootDock.Factory = defaultDockFactory;

        return (rootDock, defaultDockFactory);
    }

    public async Task SaveLayout(RootDock layout)
    {
        // todo: get it form settings
        var path = @".\layout.json";

        try
        {
            _logger.LogInformation("Saving dock layout to file {Path}", path);
            await using var fileStream = File.Open(path, FileMode.Create);

            _serializer.Save(fileStream, layout);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to save layout {@Layout}", layout);
            Console.WriteLine(e);
        }
    }
}
