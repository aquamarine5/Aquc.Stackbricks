using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Aquc.AquaUpdater.Pvder;
using Microsoft.Extensions.Logging;

namespace Aquc.AquaUpdater;

public struct LaunchConfig:ICloneable
{
    public Dictionary<string,UpdateSubscription> subscriptions;
    public Dictionary<string,Implementation> implementations;
    public string version;
    public object Clone()
    {
        return MemberwiseClone();
    }
}
public struct Implementation
{
    public string folder;
    public string version;
    public string name;
    public string link;
}
public class Launch
{
    public static LaunchConfig LaunchConfig;
    readonly object beforeLaunchConfig;
    readonly ILogger<Launch> logger;
    public Launch()
    {
        logger = Logging.InitLogger<Launch>();
        JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() => {
            var setting = new JsonSerializerSettings();
            setting.Converters.Add(new UpdateSubscriptionConverter());
            return setting;
        });

        LaunchConfig = GetLaunchConfig();
        beforeLaunchConfig = LaunchConfig.Clone();
    }

    static LaunchConfig DefaultLaunchConfig => new()
    {
        implementations = new Dictionary<string,Implementation>()
        {
            { "aliyunpan", new Implementation()
                {
                    link="https://github.com/tickstep/aliyunpan/releases/tag/v0.2.5",
                    folder="",
                    name="aliyunpan",
                    version="0.2.5"
                } 
            }
        },
        subscriptions = new Dictionary<string,UpdateSubscription>()
        {
            {"", new UpdateSubscription()
                {
                    args="748314802178228304",
                    programDirectory=new DirectoryInfo(@"D:\Program Source\v2\Aquc.AquaUpdater\Aquc.AquaUpdater\debug"),
                    programExtrancePath=new FileInfo(@"D:\Program Source\v2\Aquc.AquaUpdater\Aquc.AquaUpdater\bin\Debug\net6.0\aliyunpan.exe"),
                    programKey="test",
                    updateMessageProvider=new BiliCommitMsgPvder(),
                    currentlyVersion=new Version("0.1.0")
                } 
            }
        },
        version = Environment.Version.ToString()
    };
    
    public static string LaunchConfigPath =>
        Path.Combine(Path.GetDirectoryName(Environment.ProcessPath),"Aquc.AquaUpdater.launch.json");
    public LaunchConfig GetLaunchConfig()
    {
        logger.LogInformation("load launch config from: {launchconfigpath}",LaunchConfigPath);
        if (File.Exists(LaunchConfigPath))
        {
            using var sr = new StreamReader(LaunchConfigPath);
            return JsonConvert.DeserializeObject<LaunchConfig>(sr.ReadToEnd());
        }
        else return InitiationLaunchConfig();
    }
    public LaunchConfig InitiationLaunchConfig()
    {
        logger.LogInformation("launch config not found, initation a new launch config on:{launchconfigpath}",LaunchConfigPath);
        var lc = DefaultLaunchConfig;
        using var fs = new FileStream(LaunchConfigPath, FileMode.CreateNew, FileAccess.Write);
        using var sw = new StreamWriter(fs);
        sw.Write(JsonConvert.SerializeObject(lc));
        return lc;
    }
    public void UpdateLaunchConfig()
    {
        logger.LogInformation("launch config updated.");
        using var fs = new FileStream(LaunchConfigPath, FileMode.Truncate, FileAccess.Write);
        using var sw = new StreamWriter(fs);
        sw.Write(JsonConvert.SerializeObject(LaunchConfig));
    }
    public bool UpdateLaunchConfigWhenEdited()
    {
        if (!beforeLaunchConfig.Equals(LaunchConfig))
        {
            UpdateLaunchConfig();
            return true;
        }
        else return false;
    }
}
