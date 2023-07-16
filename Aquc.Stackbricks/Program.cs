using Newtonsoft.Json;
using System.CommandLine;

namespace Aquc.Stackbricks;

public class Program
{
    public static void Main(string[] args)
    {
        var root = new RootCommand() { };
        var updateCommand = new Command("update")
        {

        };
        var checkCommand = new Command("check")
        {
            
        };
        var installCommand = new Command("install")
        {

        };
        using var file = new FileStream("Aquc.Stackbricks.config.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using var reader = new StreamWriter(file);
        reader.Write(JsonConvert.SerializeObject(new StackbricksConfig(StackbricksManifest.CreateStackbricksManifest())));
    }
}