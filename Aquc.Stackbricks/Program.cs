using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using System.CommandLine;

namespace Aquc.Stackbricks;

public class StackbricksProgram
{
    public static readonly HttpClient _httpClient = new ();
    public static StackbricksService stackbricksService = null;
    public static readonly Logger logger=new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File($"log/{DateTime.Now:yyyyMMdd}.log")
        .CreateLogger();
    public static readonly JsonSerializerSettings jsonSerializer = new Func<JsonSerializerSettings>(() =>
    {
        JsonSerializerSettings serializerSettings = new();
        serializerSettings.Converters.Add(new DirectoryInfoJsonConverter());
        serializerSettings.Converters.Add(new VersionJsonConverter());
        return serializerSettings;
    }).Invoke();

    public static void Main(string[] args)
    {
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
        var configCommand = new Command("config")
        {
            configCreateCommand
        };
        updateCommand.SetHandler(async () =>
        {
            await stackbricksService.UpdateWhenAvailable();
        });
        configCreateCommand.SetHandler(() =>
        {

            using var file = new FileStream("Aquc.Stackbricks.config.json", FileMode.Create, FileAccess.Write);
            using var reader = new StreamWriter(file);
            reader.Write(JsonConvert.SerializeObject(new StackbricksConfig(StackbricksManifest.CreateStackbricksManifest()),jsonSerializer));
            //Console.WriteLine(JsonConvert.SerializeObject(new StackbricksConfig(StackbricksManifest.CreateStackbricksManifest()), jsonSerializer));
        });
        var root = new RootCommand() 
        {
            updateCommand,
            checkCommand,
            installCommand,
            downloadCommand,
            configCommand
        };
        root.Invoke(args);
    }
}