using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Aquc.AquaUpdater.Pvder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Aquc.AquaUpdater;

public struct LaunchConfig : ICloneable
{
    public Dictionary<string, UpdateSubscription> subscriptions;
    public Dictionary<string, Implementation> implementations;
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
    public static LaunchConfig launchConfig;
    static object beforeLaunchConfig;
    static ILogger<Launch> logger;
    public Launch()
    {
        JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
        {
            var setting = new JsonSerializerSettings();
            setting.Converters.Add(new UpdateSubscriptionConverter());
            setting.Converters.Add(new DirectoryInfoConverter());
            return setting;
        });
        logger = Logging.InitLogger<Launch>();
        launchConfig = GetLaunchConfig();
        beforeLaunchConfig = launchConfig.Clone();
    }

    static LaunchConfig DefaultLaunchConfig => new()
    {
        implementations = new Dictionary<string, Implementation>()
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
        subscriptions = new Dictionary<string, UpdateSubscription>()
        {
        },
        version = Environment.Version.ToString()
    };

    public static string LaunchConfigPath =>
        Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "Aquc.AquaUpdater.launch.json");
    public static LaunchConfig GetLaunchConfig()
    {
        logger.LogInformation("load launch config from: {launchconfigpath}", LaunchConfigPath);
        if (File.Exists(LaunchConfigPath))
        {
            using var sr = new StreamReader(LaunchConfigPath);
            return JsonConvert.DeserializeObject<LaunchConfig>(sr.ReadToEnd());
        }
        else return InitiationLaunchConfig();
    }
    public static LaunchConfig InitiationLaunchConfig()
    {
        logger.LogInformation("launch config not found, initation a new launch config on:{launchconfigpath}", LaunchConfigPath);
        var lc = DefaultLaunchConfig;
        using var fs = new FileStream(LaunchConfigPath, FileMode.CreateNew, FileAccess.Write);
        using var sw = new StreamWriter(fs);
        sw.Write(JsonConvert.SerializeObject(lc));
        UpdaterProgram.RegisterScheduleTasks();
        return lc;
    }
    public static void UpdateLaunchConfig()
    {
        using var fs = new FileStream(LaunchConfigPath, FileMode.Truncate, FileAccess.Write);
        using var sw = new StreamWriter(fs);
        sw.Write(JsonConvert.SerializeObject(launchConfig));
        logger.LogInformation("launch config updated.");
        beforeLaunchConfig = launchConfig.Clone();
    }
    public static bool UpdateLaunchConfigWhenEdited()
    {
        if (!beforeLaunchConfig.Equals(launchConfig))
        {
            UpdateLaunchConfig();
            return true;
        }
        else return false;
    }
}
public class DirectoryInfoConverter : JsonConverter<DirectoryInfo>
{
    public override DirectoryInfo ReadJson(JsonReader reader, Type objectType, DirectoryInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new DirectoryInfo((serializer.Deserialize(reader) as JObject)["directory_info"].ToString());
    }

    public override void WriteJson(JsonWriter writer, DirectoryInfo value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("directory_info");
        writer.WriteValue(value.Name);
    }
}