using Aquc.Stackbricks.DataClass;
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
    public static readonly Logger logger = new Func<Logger>(() =>
    {
        var args = Environment.GetCommandLineArgs();
        var loggerconfig = new LoggerConfiguration()
            .WriteTo.File($"log/{DateTime.Now:yyyyMMdd}.log");
        if (!args.Contains("--no-log"))
            loggerconfig.WriteTo.Console();
        loggerconfig.WriteTo.Sentry(o =>
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
            o.Debug = args.Contains("--sentrylog");
        });
        loggerconfig.MinimumLevel.Verbose();
        return loggerconfig.CreateLogger();
    }).Invoke();
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
        try
        {
            await BuiltinMain(args);
        }
        catch(Exception ex)
        {
            if (args.Contains("--json"))
                DataClassParser.ParseDataClassPrintin(new ExceptionDataClass(ex));
            throw;
        }
    }

    static async Task BuiltinMain(string[] args)
    {
        var jsonOption = new Option<bool>("--json", () => { return false; });
        var uwpnofOption = new Option<bool>("--no-uwpnof", () => { return false; });
        var nologOption = new Option<bool>("--no-log", () => { return false; });
        var sentrylogOption = new Option<bool>("--sentrylog", () => { return false; });

        var updateCommand = new Command("update") { jsonOption, uwpnofOption };
        var checkCommand = new Command("check") { jsonOption, uwpnofOption };
        var installCommand = new Command("install") { jsonOption, uwpnofOption };
        var checkdlCommand = new Command("checkdl") { jsonOption, uwpnofOption };
        var updateallCommand = new Command("updateall") { jsonOption, uwpnofOption };

        var configCreateCommand = new Command("create");
        var configCommand = new Command("config")
        {
            configCreateCommand
        };

        var selfUpdateCommand = new Command("update") { jsonOption, uwpnofOption };
        var selfInstallCommand = new Command("install") { jsonOption, uwpnofOption };
        var selfCheckCommand = new Command("check") { jsonOption, uwpnofOption };
        var selfCommand = new Command("self")
        {
            selfInstallCommand,
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
        updateCommand.SetHandler(async (isJson, isNoUwpnof) =>
        {
            StackbricksService stackbricksService = new();
            logger.Information("Start to update program if the program has newest version.");
            if (isJson) DataClassParser.ParseDataClassPrintin(await stackbricksService.UpdateDC(isNoUwpnof));
            else await stackbricksService.Update();
        }, jsonOption, uwpnofOption);
        selfUpdateCommand.SetHandler(async (isJson, isNoUwpnof) =>
        {
            StackbricksService stackbricksService = new();
            logger.Information("Start to update Aquc.Stackbricks if the program has newest version.");
            if (isJson) DataClassParser.ParseDataClassPrintin(await stackbricksService.UpdateStackbricksDC(isNoUwpnof));
            else await stackbricksService.UpdateStackbricks();
        }, jsonOption, uwpnofOption);
        updateallCommand.SetHandler(async (isJson, isNoUwpnof) =>
        {
            StackbricksService stackbricksService = new();
            logger.Information("Start to update program if the program has newest version.");
            if (isJson) DataClassParser.ParseDataClassPrintin(await stackbricksService.UpdateDC(isNoUwpnof));
            else await stackbricksService.Update();

            logger.Information("Start to update Aquc.Stackbricks if the program has newest version.");
            if (isJson) DataClassParser.ParseDataClassPrintin(await stackbricksService.UpdateStackbricksDC(isNoUwpnof));
            else await stackbricksService.UpdateStackbricks();
        }, jsonOption, uwpnofOption);
        checkCommand.SetHandler(async (isJson, isNoUwpnof) =>
        {
            StackbricksService stackbricksService = new();
            if (isJson) DataClassParser.ParseDataClassPrintin(await stackbricksService.CheckUpdateDC());
            else await stackbricksService.CheckUpdate(isNoUwpnof);
        }, jsonOption, uwpnofOption);
        selfCheckCommand.SetHandler(async (isJson, isNoUwpnof) =>
        {
            StackbricksService stackbricksService = new();
            if (isJson) DataClassParser.ParseDataClassPrintin(await stackbricksService.CheckStackbricksUpdateDC());
            else await stackbricksService.CheckStackbricksUpdate(isNoUwpnof);
        }, jsonOption, uwpnofOption);
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
            selfCommand,
            checkCommand
        };
        root.AddGlobalOption(sentrylogOption);
        root.AddGlobalOption(nologOption);
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