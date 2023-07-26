using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using Sentry;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace Aquc.Stackbricks;

public class StackbricksProgram
{
    public static readonly HttpClient httpClient = new();
    public static readonly Logger logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File($"log/{DateTime.Now:yyyyMMdd}.log")
        .WriteTo.Sentry(o =>
        {
            // Debug and higher are stored as breadcrumbs (default os Information)
            o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
            // Error and higher is sent as event (default is Error)
            o.MinimumEventLevel = LogEventLevel.Error;
            // If DSN is not set, the SDK will look for an environment variable called SENTRY_DSN. If nothing is found, SDK is disabled.
            o.Dsn = "https://92a9029060f841219ef1306de87c345f@o4505418205364224.ingest.sentry.io/4505458345377792";
            o.AttachStacktrace = true;
            // send PII like the username of the user logged in to the device
            o.SendDefaultPii = true;
            // Optional Serilog text formatter used to format LogEvent to string. If TextFormatter is set, FormatProvider is ignored.
            // Other configuration
            o.AutoSessionTracking = true;

            // This option is recommended for client applications only. It ensures all threads use the same global scope.
            // If you're writing a background service of any kind, you should remove this.
            o.IsGlobalModeEnabled = false;

            // This option will enable Sentry's tracing features. You still need to start transactions and spans.
            o.EnableTracing = true;
            o.Debug = true;
        })
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

    public static async Task Main(string[] args)
    {
        var updateCommand = new Command("update");
        var checkCommand = new Command("check");
        var installCommand = new Command("install");
        var downloadCommand = new Command("download");
        var updateallCommand = new Command("updateall");

        var configCreateCommand = new Command("create");
        var configCommand = new Command("config")
        {
            configCreateCommand
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

        var testCommand = new Command("test");
        testCommand.SetHandler(() =>
        {

            new ToastContentBuilder()
                .AddText($"{1} 已成功更新至版本 {2}")
                .Show();
        });
        updateCommand.SetHandler(async () =>
        {
            StackbricksService stackbricksService = new();
            logger.Information("Start to update program if the program has newest version.");
            await stackbricksService.UpdateWhenAvailable();
        });
        selfUpdateCommand.SetHandler(async () =>
        {
            StackbricksService stackbricksService = new();
            logger.Information("Start to update Aquc.Stackbricks if the program has newest version.");
            await stackbricksService.UpdateStackbricksWhenAvailable();
        });
        updateallCommand.SetHandler(async () =>
        {
            StackbricksService stackbricksService = new();
            logger.Information("Start to update program if the program has newest version.");
            await stackbricksService.UpdateWhenAvailable();
            logger.Information("Start to update Aquc.Stackbricks if the program has newest version.");
            await stackbricksService.UpdateStackbricksWhenAvailable();
        });
        configCreateCommand.SetHandler(() =>
        {
            using var file = new FileStream("Aquc.Stackbricks.config.json", FileMode.Create, FileAccess.Write);
            using var reader = new StreamWriter(file);
            reader.Write(JsonConvert.SerializeObject(new StackbricksConfig(StackbricksManifest.CreateBlankManifest()), jsonSerializer));
            logger.Information("Success created default Aquc.Stackbricks.config.json");
        });
        var root = new RootCommand()
        {
            testCommand,
            updateallCommand,
            updateCommand,
            configCommand,
            selfCommand
        };
        await new CommandLineBuilder(root)
           .UseVersionOption()
           .UseHelp()
           .UseEnvironmentVariableDirective()
           .UseParseDirective()
           .RegisterWithDotnetSuggest()
           .UseTypoCorrections()
           .UseParseErrorReporting()
           .CancelOnProcessTermination()
           .Build()
           .InvokeAsync(args);
        httpClient.Dispose();
    }
}