using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using TagTool.App.Views;

namespace TagTool.App.Extensions;

public static class ServiceCollectionExtension
{
    private static Func<Type, bool> ViewModelBasePredicate
        => x => typeof(ViewModelBase).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false };

    public static void AddViewModelsFromAssembly(this IServiceCollection services, Type scanMarker)
    {
        var viewModelsBases = scanMarker.Assembly.ExportedTypes.Where(ViewModelBasePredicate);

        foreach (var type in viewModelsBases)
        {
            services.AddTransient(type);
        }
    }

    public static void SetupLogging(this IServiceCollection services)
    {
        const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}";
        var logsPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TagTool", "App", "Logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient.IOpenAIService", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .WriteTo.Seq("http://localhost:5341")
            .WriteTo.Debug(outputTemplate: outputTemplate, formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                new CompactJsonFormatter(),
                $"{logsPath}/logs.json",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddSerilog(Log.Logger, true);
        });
    }

    public static void AddConfiguration(this IServiceCollection services, out IConfiguration configuration)
    {
        configuration = new ConfigurationBuilder()
            .AddJsonFile("defaultAppSettings.json", true, true)
            .Build();

        services.AddSingleton(configuration);
    }
}
