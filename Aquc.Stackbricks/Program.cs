using Newtonsoft.Json;
using Sentry;
using Serilog;
using Serilog.Core;
using System.CommandLine;

namespace Aquc.Stackbricks;

public class StackbricksProgram
{
    public static readonly HttpClient _httpClient = new();
    public static readonly Logger logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File($"log/{DateTime.Now:yyyyMMdd}.log")
        .MinimumLevel.Verbose()
        .CreateLogger();
    public static readonly JsonSerializerSettings jsonSerializer = new Func<JsonSerializerSettings>(() =>
    {
        JsonSerializerSettings serializerSettings = new()
        {
            Formatting = Formatting.Indented
        };
        serializerSettings.Converters.Add(new DirectoryInfoJsonConverter());
        serializerSettings.Converters.Add(new VersionJsonConverter());
        serializerSettings.Converters.Add(new StackbricksActionDataJsonConverter());
        return serializerSettings;
    }).Invoke();

    public static readonly StackbricksService stackbricksService = new();
    public static void Main(string[] args)
    {
        SentrySdk.Init(options =>
        {
            // A Sentry Data Source Name (DSN) is required.
            // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
            // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
            options.Dsn = "https://92a9029060f841219ef1306de87c345f@o4505418205364224.ingest.sentry.io/4505458345377792";

            // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
            // This might be helpful, or might interfere with the normal operation of your application.
            // We enable it here for demonstration purposes when first trying Sentry.
            // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
            options.Debug = true;

            // This option is recommended. It enables Sentry's "Release Health" feature.
            options.AutoSessionTracking = true;

            // This option is recommended for client applications only. It ensures all threads use the same global scope.
            // If you're writing a background service of any kind, you should remove this.
            options.IsGlobalModeEnabled = true;

            // This option will enable Sentry's tracing features. You still need to start transactions and spans.
            options.EnableTracing = true;
        });

        var updateCommand = new Command("update")
        {

        };
        var checkCommand = new Command("check")
        {

        };
        var installCommand = new Command("install")
        {

        };
        var downloadCommand = new Command("download")
        {

        };
        var configCreateCommand = new Command("create")
        {

        };
        var selfUpdateCommand = new Command("update");
        var selfApplyCommand = new Command("apply");
        var selfCheckCommand = new Command("check")
        {

        };
        var selfCommand = new Command("self")
        {
            selfApplyCommand,
            selfCheckCommand,
            selfUpdateCommand
        };
        var configCommand = new Command("config")
        {
            configCreateCommand
        };
        updateCommand.SetHandler(async () =>
        {
            logger.Information("Start to update program if the program has newest version.");
            await stackbricksService.UpdateWhenAvailable();
        });
        selfUpdateCommand.SetHandler(async () =>
        {

            logger.Information("Start to update Aquc.Stackbricks if the program has newest version.");
            await stackbricksService.UpdateStackbricksWhenAvailable();
        });
        configCreateCommand.SetHandler(() =>
        {

            using var file = new FileStream("Aquc.Stackbricks.config.json", FileMode.Create, FileAccess.Write);
            using var reader = new StreamWriter(file);
            reader.Write(JsonConvert.SerializeObject(new StackbricksConfig(StackbricksManifest.CreateStackbricksManifest()), jsonSerializer));
            logger.Information("Success created default Aquc.Stackbricks.config.json");
        });
        var root = new RootCommand()
        {
            updateCommand,
            checkCommand,
            installCommand,
            downloadCommand,
            configCommand,
            selfCommand
        };
        root.Invoke(args);
    }
}