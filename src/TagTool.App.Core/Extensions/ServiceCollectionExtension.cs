using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.ViewModels;

namespace TagTool.App.Core.Extensions;

public static class ServiceCollectionExtension
{
    private static Func<Type, bool> ViewModelBasePredicate
        => x => typeof(ViewModelBase).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false };

    public static void AddViewModels(this IServiceCollection services, Type scanMarker)
    {
        var viewModelsBases = scanMarker.Assembly.ExportedTypes.Where(ViewModelBasePredicate);

        foreach (var type in viewModelsBases)
        {
            services.AddTransient(type);
        }
    }
}
