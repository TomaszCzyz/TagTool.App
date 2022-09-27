using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TagTool.App.Extensions;

public static class ResourceHostExtensions
{
    public static IServiceProvider GetServiceProvider(this IResourceHost control)
        => (IServiceProvider)control.FindResource(typeof(IServiceProvider))!;

    public static T CreateInstance<T>(this IResourceHost control)
        => ActivatorUtilities.CreateInstance<T>(control.GetServiceProvider());
}
