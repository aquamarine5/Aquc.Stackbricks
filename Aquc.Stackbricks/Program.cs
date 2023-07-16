using Newtonsoft.Json;
using System.CommandLine;

namespace Aquc.Stackbricks;

public class Program
{
    public static readonly HttpClient _httpClient = new ();
    public static async void Main(string[] args)
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
        updateCommand.SetHandler(() =>
        {

        });
        var root = new RootCommand() 
        {
            updateCommand,
            checkCommand,
            installCommand,
            downloadCommand,
        };
        await root.InvokeAsync(args);
        using var file = new FileStream("Aquc.Stackbricks.config.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using var reader = new StreamWriter(file);
        reader.Write(JsonConvert.SerializeObject(new StackbricksConfig(StackbricksManifest.CreateStackbricksManifest())));
    }
}