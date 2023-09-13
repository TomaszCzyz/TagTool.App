using System.Collections.ObjectModel;
using System.Reflection;
using Dock.Model.Mvvm.Controls;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TagTool.App.Docks;

public class DependencyInjectionContractResolver : DefaultContractResolver
{
    private readonly IServiceProvider _serviceProvider;

    // private readonly Type _type = typeof(AvaloniaList<>);
    private readonly Type _type = typeof(ObservableCollection<>);

    public DependencyInjectionContractResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override JsonContract ResolveContract(Type type)
    {
        if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
        {
            return base.ResolveContract(_type.MakeGenericType(type.GenericTypeArguments[0]));
        }

        return base.ResolveContract(type);
    }

    /// <inheritdoc />
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        => base.CreateProperties(type, memberSerialization).Where(p => p.Writable).ToList();

    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        var service = _serviceProvider.GetService(objectType);

        if (service is Document or MyDocumentDock)
        {
            var contract = ResolveContact(objectType);
            contract.DefaultCreator = () => _serviceProvider.GetRequiredService(objectType);

            return contract;
        }

        return base.CreateObjectContract(objectType);
    }

    private JsonObjectContract ResolveContact(Type objectType)
    {
        var service = _serviceProvider.GetService(objectType);
        if (service == null)
        {
            return base.CreateObjectContract(objectType);
        }

        var concreteType = service.GetType();
        return base.CreateObjectContract(concreteType);
    }
}
// public class DependencyInjectionConverter<T> : CustomCreationConverter<T> where T : class
// {
//     private readonly IServiceProvider _serviceProvider;
//
//     public DependencyInjectionConverter(IServiceProvider serviceProvider)
//     {
//         _serviceProvider = serviceProvider;
//     }
//
//     public override T Create(Type objectType)
//     {
//         return (_serviceProvider.GetService(objectType) as T)!;
//     }
// }
