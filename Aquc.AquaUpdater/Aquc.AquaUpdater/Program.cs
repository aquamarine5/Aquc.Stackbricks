using System;
using System.Collections.Generic;
using System.Linq;
using Aquc.AquaUpdater.Pvder;
using Aquc.AquaUpdater.Util;
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
    static void Main(string[] args)
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

            })
            .AddHandler<UpdateOption>(option => {
                UpdateWhenAvailable(Launch.LaunchConfig.subscriptions[option.Key]);
            })
            .AddHandler<UnsubscribeOption>(option => { 
            
            })
            .Run();
    }
    public static void UpdateWhenAvailable(UpdateSubscription updateSubscription)
    {
        var msg = updateSubscription.GetUpdateMessage();
        if (msg.NeedUpdate()) msg.GetUpdatePackage().InstallPackage();
    }
    public static void UpdateAllWhenAvailable(List<UpdateSubscription> updateSubscriptions)
    {
        foreach (var item in updateSubscriptions)
            UpdateWhenAvailable(item);
    }

}
[Verb("subscribe")]
public class SubscribeOption
{
    [Option('a',"Args")]public string Args { get; set; }
    [Option('p',"Provider")]public string Provider { get; set; }
    [Option('v',"Version")]public string Version { get; set; }
    [Option('d',"Directory")]public string Directory { get; set; }
    [Option('e',"Program")]public string Program { get; set; }
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
