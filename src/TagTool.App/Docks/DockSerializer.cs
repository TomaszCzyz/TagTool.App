using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace TagTool.App.Docks;

public class DockSerializer
{
    private readonly JsonSerializerSettings _settings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DockSerializer" /> class with the specified list type.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public DockSerializer(IServiceProvider serviceProvider)
    {
        var contractResolver = new DependencyInjectionContractResolver(serviceProvider);

        _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = contractResolver,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new KeyValuePairConverter() }
        };
    }

    public string Serialize<T>(T value) => JsonConvert.SerializeObject(value, _settings);

    public T? Deserialize<T>(string text) => JsonConvert.DeserializeObject<T>(text, _settings);

    public T? Load<T>(Stream stream)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        var text = streamReader.ReadToEnd();
        return Deserialize<T>(text);
    }

    public void Save<T>(Stream stream, T value)
    {
        var text = Serialize(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        streamWriter.Write(text);
    }
}

public class ListContractResolver : DefaultContractResolver
{
    private readonly Type _type;

    public ListContractResolver(Type type)
    {
        _type = type;
    }

    /// <inheritdoc />
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
}
