using System;
using System.Collections.Generic;
using System.Linq;
using Aquc.AquaUpdater.Pvder;
using Aquc.AquaUpdater.Util;
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
        UpdateLaunchArgs updateLaunchArgs = ArgsParser.Parse<UpdateLaunchArgs>(args);
        if (updateLaunchArgs.update != null)
            Launch.LaunchConfig.subscriptions[updateLaunchArgs.update].GetUpdateMessage().GetUpdatePackageWhenAvailable()?.InstallPackage();
        if (updateLaunchArgs.update_all.isContains)
            foreach (var item in Launch.LaunchConfig.subscriptions.Values)
                item.GetUpdateMessage().GetUpdatePackageWhenAvailable()?.InstallPackage();
        if (updateLaunchArgs.unsubscribe != null)
            Launch.LaunchConfig.subscriptions.Remove(updateLaunchArgs.unsubscribe);
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
public struct UpdateLaunchArgs : IArgsParseResult
{
    public string update;
    public ArgsNamedKey update_all;

    public ArgsNamedKey subscribe;
    public string args;
    public string provider;
    public string version;
    public string directory;
    public string program;
    public string json;

    public string unsubscribe;
}
