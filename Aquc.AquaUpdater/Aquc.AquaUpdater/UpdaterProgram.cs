using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aquc.AquaUpdater.Pvder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;

namespace Aquc.AquaUpdater;

public class UpdaterProgram
{
    public Launch launch;
    static ILogger<UpdaterProgram> logger=Logging.UpdaterProgramLogger;
    string[] args;
    public static void Main(string[] args)
    {
        logger = Logging.InitLogger<UpdaterProgram>();
        JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
        {
            var setting = new JsonSerializerSettings();
            setting.Converters.Add(new UpdateSubscriptionConverter());
            setting.Converters.Add(new DirectoryInfoConverter());
            return setting;
        });
        var program = new UpdaterProgram
        {
            launch = new Launch(),
            args = args
        };
        logger.LogInformation("Hello World!");
        program.ParseLaunchArgs();
    }
    void ParseLaunchArgs()
    {
        #region subscribe <json/kvp/unsubscribe/list>
        var jsonSubscribeArgument = new Argument<FileInfo>()
        {
            Description = "",
            Arity=ArgumentArity.ExactlyOne
        };
        
        var kvpSubscribeProvider = new Option<IUpdateMessageProvider>(new string[] { "-pvd","--provider"}, parseArgument: result =>
        {
            var content = result.Tokens.Single().Value;
            if (!Provider.ContainInMessageProvider(content))
            {
                result.ErrorMessage = "";
                return null;
            }
            return Provider.GetMessageProvider(content);
        })
        {
            Description="",
            Arity=ArgumentArity.ExactlyOne
        };
        var kvpSubscribeSubprovider = new Option<IUpdateMessageProvider>(new string[] { "-subpvd", "--subprovider" }, parseArgument: result =>
        {
            var content = result.Tokens.Single().Value;
            if (!Provider.ContainInMessageProvider(content))
            {
                result.ErrorMessage = "";
                return null;
            }
            return Provider.GetMessageProvider(content);
        })
        {
            Description = "",
            Arity = ArgumentArity.ZeroOrOne,
        };
        var kvpSubscribeArgs = new Option<string>(new string[] { "-a", "--args" })
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne
        };
        var kvpSubscribeProgram = new Option<FileInfo>(new string[] { "-p", "--program" })
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne
        };
        var kvpSubscribeDirectory = new Option<DirectoryInfo>(new string[] { "-d", "--directory" }, parseArgument: result =>
        {
            if (result.Tokens.Count == 0)
                return result.GetValueForOption(kvpSubscribeProgram).Directory;
            else
            {
                var content = result.Tokens.Single().Value;
                if (!Directory.Exists(content))
                {
                    result.ErrorMessage = "";
                    return null;
                }
                else
                    return new DirectoryInfo(content);
            }
        })
        {
            Description = "",
            Arity = ArgumentArity.ZeroOrOne
        };
        var kvpSubscribeVersion = new Option<Version>(new string[] { "-v", "--version" }, parseArgument: result =>
        {
            if (result.Tokens.Count == 0)
            {
                var fvi = FileVersionInfo.GetVersionInfo(result.GetValueForOption(kvpSubscribeProgram).FullName).FileVersion;
                if (fvi == null)
                {
                    result.ErrorMessage = "";
                    return null;
                }
                else
                    return new Version(fvi);
            }
            else
            {
                if (Version.TryParse(result.Tokens.Single().Value, out var version))
                    return version;
                else
                {
                    result.ErrorMessage = "";
                    return null;
                }
            }
        })
        {
            Description = "",
            Arity = ArgumentArity.ZeroOrOne
        };
        var kvpSubscribeKey = new Option<string>(new string[] { "-k", "--key" }, parseArgument: result =>
        {
            if (result.Tokens.Count == 0)
            {
                return Path.GetFileNameWithoutExtension(result.GetValueForOption(kvpSubscribeProgram).Name);
            }
            else
                return result.Tokens.Single().Value;
        })
        {
            Description = "",
            Arity = ArgumentArity.ZeroOrOne
        };

        var unsubscribeKeyArgument = new Argument<string>(parse: result =>
        {
            var content = result.Tokens.Single().Value;
            if (!Launch.launchConfig.subscriptions.ContainsKey(content))
            {
                result.ErrorMessage = "";
                return null;
            }
            else return result.Tokens.Single().Value;
        })
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne
        };

        var listKeyArgument = new Argument<string>(parse: result =>
        {
            var content = result.Tokens.Single().Value;
            if (content == "")
                return "%all";
            if (!Launch.launchConfig.subscriptions.ContainsKey(content))
            {
                result.ErrorMessage = "";
                return null;
            }
            else return result.Tokens.Single().Value;
        })
        {
            Description = "",
            Arity = ArgumentArity.ZeroOrOne
        };
        #endregion

        #region update
        var updateArgument = new Argument<string>(parse: result =>
        {
            var content = result.Tokens.Single().Value;
            if (!Launch.launchConfig.subscriptions.ContainsKey(content))
            {
                result.ErrorMessage = "";
                return null;
            }
            else return result.Tokens.Single().Value;
        })
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne
        };

        #endregion

        #region schedule
        var scheduleInitCommand = new Command("init");
        #endregion

        // subscribe <json/kvp/unsubscribe/list>
        var jsonSubscribeCommand = new Command("json")
        {
            jsonSubscribeArgument
        };
        var kvpSubscribeCommand = new Command("kvp")
        {
            kvpSubscribeDirectory,
            kvpSubscribeProgram,
            kvpSubscribeProvider,
            kvpSubscribeSubprovider,
            kvpSubscribeVersion,
            kvpSubscribeKey,
            kvpSubscribeArgs
        };
        var listSubscribeCommand = new Command("list")
        {
            listKeyArgument
        };
        var unsubscribeCommand = new Command("unsubscribe")
        {
            unsubscribeKeyArgument
        };
        var subscribeCommand = new Command("subscribe")
        {
            jsonSubscribeCommand,
            kvpSubscribeCommand,
            listSubscribeCommand,
            unsubscribeCommand
        };

        // schedule
        var scheduleCommand = new Command("schedule")
        {
            scheduleInitCommand
        };

        // update
        var updateCommand = new Command("update")
        {
            updateArgument
        };
        var updateAllCommand = new Command("updateall");
        var root = new RootCommand()
        {
            subscribeCommand,
            scheduleCommand,
            updateCommand
        };
        scheduleInitCommand.SetHandler(RegisterScheduleTasks);
        updateAllCommand.SetHandler(() =>
        {
            UpdateAllWhenAvailable(Launch.launchConfig.subscriptions);
        });
        jsonSubscribeCommand.SetHandler((json) => {
            SubscriptionController.RegisterSubscriptionByJson(json);
        },jsonSubscribeArgument);
        listSubscribeCommand.SetHandler((key) =>
        {
            Console.WriteLine(JsonConvert.SerializeObject(Launch.launchConfig.subscriptions, Formatting.Indented));
        }, listKeyArgument);
        updateCommand.SetHandler((key) =>
        {
            UpdateWhenAvailable(Launch.launchConfig.subscriptions[key]);
        }, updateArgument);
        unsubscribeCommand.SetHandler((key) =>
        {
            if (Launch.launchConfig.subscriptions.Remove(key))
            {
                logger.LogInformation("Unsubscribe {key} successfully.", key);
                Launch.UpdateLaunchConfig();
            }
            else
                logger.LogError("Unsubscribe {key} failed. Not found.", key);
        }, unsubscribeKeyArgument);
        root.Invoke(args);
        
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
    public static void UpdateAllWhenAvailable(Dictionary<string,UpdateSubscription> updateSubscriptions)
    {
        logger.LogInformation("Update all subscriptions. Found {length}.", updateSubscriptions.Count);
        foreach (var item in updateSubscriptions.Values)
            UpdateWhenAvailable(item);
    }
    public static void RegisterScheduleTasks()
    {
        
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "schtasks",
                Arguments = $"/Create /F /SC weekly /D MON /TR \"'{Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "aliyunpan.exe") + "' token update -mode 2"}\" /TN \"Aquacore\\Aquc.AquaUpdater.Aliyunpan.UpdateToken\"",
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit(5000);
        logger.LogInformation("Success schedule aliyunpan-token-update");
        process.Dispose();
        var process2 = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "schtasks",
                Arguments = $"/Create /F /SC weekly /D MON /TR \"'{Environment.ProcessPath + "' updateall"}\" /TN \"Aquacore\\Aquc.AquaUpdater.CheckSubscrptionsUpdate\"",
                CreateNoWindow = true
            }
        };
        process2.Start();
        process2.WaitForExit();
        logger.LogInformation("Success schedule subscriptions-update-all");
        process2.Dispose();
    }
}
public class SubscribeOption
{
    public string Args { get; set; }
    public string Provider { get; set; }
    public string Subprovider { get; set; }
    public string Version { get; set; }
    public string Directory { get; set; }
    public string Program { get; set; }
    public string Key { get; set; }
}

public class UpdateOption
{
    public string Key { get; set; }
}

public class UnsubscribeOption
{
    public string Key { get; set;}
}
