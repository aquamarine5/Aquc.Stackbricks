using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aquc.AquaUpdater.Pvder;
using dotnetCampus.Cli;
using dotnetCampus.Cli.Standard;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;

namespace Aquc.AquaUpdater;

public class Program
{
    public Launch launch;
    static ILogger<Program> logger;
    string[] args;
    public static void Main(string[] args)
    {
        logger = Logging.InitLogger<Program>();
        JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
        {
            var setting = new JsonSerializerSettings();
            setting.Converters.Add(new UpdateSubscriptionConverter());
            return setting;
        });
        var program = new Program
        {
            launch = new Launch(),
            args = args
        };
        logger.LogInformation("Hello World!");
        program.ParseLaunchArgs();
    }
    void ParseLaunchArgs()
    {
        CommandLine.Parse(args)
            .AddStandardHandlers()
            .AddHandler<SubscribeOption>(option => {
                if (!SubscriptionController.RegisterSubscription(option))
                    logger.LogError("Failed to register a new subscription.");
            })
            .AddHandler<UpdateOption>(option => {
                if (Launch.launchConfig.subscriptions.ContainsKey(option.Key))
                {
                    logger.LogInformation("Start update: {key}", option.Key);
                    UpdateWhenAvailable(Launch.launchConfig.subscriptions[option.Key]);
                }
                else
                    logger.LogError("Update {key} failed. Not found.", option.Key);
            })
            .AddHandler<UnsubscribeOption>(option => {
                if (Launch.launchConfig.subscriptions.Remove(option.Key))
                {
                    logger.LogInformation("Unsubscribe {key} successfully.",option.Key);
                    Launch.UpdateLaunchConfig();
                    
                }
                else
                    logger.LogError("Unsubscribe {key} failed. Not found.", option.Key);
            })
            .Run();
    }
    public static void UpdateWhenAvailable(UpdateSubscription updateSubscription)
    {
        var msg = updateSubscription.GetUpdateMessage();
        if (msg.NeedUpdate()) 
        {
            logger.LogInformation("{key} have new version {version} to use", updateSubscription.programKey, msg.packageVersion);
            msg.GetUpdatePackage().InstallPackage(); 
        }
    }
    public static void UpdateAllWhenAvailable(List<UpdateSubscription> updateSubscriptions)
    {
        logger.LogInformation("Update all subscriptions. Found {length}.", updateSubscriptions.Count);
        foreach (var item in updateSubscriptions)
            UpdateWhenAvailable(item);
    }

}
[Verb("subscribe")]
public class SubscribeOption
{
    [Option('a', "Args"),Value(2),]public string Args { get; set; }
    [Option('p',"Provider"),Value(1),]public string Provider { get; set; }
    [Option('s',"Subprovider")] public string Subprovider { get; set; }
    [Option('v',"Version")]public string Version { get; set; }
    [Option('d',"Directory")]public string Directory { get; set; }
    [Option('e', "Program"), Value(0),]public string Program { get; set; }
    [Option('k',"Key")]public string Key { get; set; }
    [Option('j',"Json")]public string Json { get; set; }
}

[Verb("update")]
public class UpdateOption
{
    [Value(0), Option('k', "Key")]public string Key { get; set; }
}

[Verb("unsubscribe")]
public class UnsubscribeOption
{
    [Value(0), Option('k',"Key")] public string Key { get; set;}
}
