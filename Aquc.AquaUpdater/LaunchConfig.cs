using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Aquc.AquaUpdater.Pvder;
using Microsoft.Extensions.Logging;

namespace Aquc.AquaUpdater
{
    public struct LaunchConfig
    {
        public List<UpdateSubscription> subscriptions;
        public Dictionary<string,Implementation> implementations;
        public string version;
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
            
        }

        static LaunchConfig DefaultLaunchConfig => new()
        {
            implementations = new Dictionary<string,Implementation>()
            {
                { "aliyunpan", new Implementation()
                    {
                        link="https://github.com/tickstep/aliyunpan/releases/tag/v0.2.5",
                        folder="aliyunpan-v0.2.5-windows-x86",
                        name="aliyunpan",
                        version="0.2.5"
                    } 
                }
            },
            subscriptions = new List<UpdateSubscription>()
            {
                new UpdateSubscription()
                {
                    args="aja",
                    updateMessageProvider=new BiliCommitMsgPvder()
                }
            },
            version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
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
    }
}
