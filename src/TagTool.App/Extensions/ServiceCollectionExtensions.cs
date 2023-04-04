using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels;

namespace TagTool.App.Extensions;

public static class ServiceCollectionExtensions
{
    private static Func<Type, bool> ViewModelBasePredicate
        => x => typeof(ViewModelBase).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false };

    private static Func<Type, bool> DockablePredicate
        => x => typeof(IDockable).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false };

    public static void AddViewModels(this IServiceCollection services, Type scanMarker)
    {
        var viewModelsBases = scanMarker.Assembly.ExportedTypes.Where(ViewModelBasePredicate);
        var dockables = scanMarker.Assembly.ExportedTypes.Where(DockablePredicate);

        foreach (var type in viewModelsBases.Union(dockables))
        {
            services.AddTransient(type);
        }
    }
}
