using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Aquc.AquaUpdater.Pvder;
using Aquc.Netdisk.Aliyunpan;
using Aquc.Netdisk.Bilibili;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;

namespace Aquc.AquaUpdater;

public class UpdaterProgram
{
    private static ILogger<UpdaterProgram> logger;
    public static UpdaterService CurrentUpdaterService { get; set; }
    public static IHost host;
    public static void Main(string[] args)
    {
        using var host=new HostBuilder()
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSimpleConsole((option) => option.UseUtcTimestamp = true);
                builder.AddFile();
            })
            .ConfigureAppConfiguration((builder) =>
            {
                //builder.AddJsonFile(UpdaterService.CONFIG_JSON,false,true);
            })
            .ConfigureServices(container =>
            {
                container.AddScoped<UpdaterService>();
                container.AddSingleton(container => {
                    var token = BilibiliMsgPvder.Get("221831529");
                    token.Wait();
                    return new AliyunpanNetdisk(
                        new FileInfo(Path.Combine(AppContext.BaseDirectory, "aliyunpan.exe")),
                        token.Result,
                        container.GetRequiredService<ILogger<AliyunpanNetdisk>>());
                });
            })
            .Build();
        var _ = new Launch();
        UpdaterProgram.host = host;
        logger = host.Services.GetRequiredService<ILogger<UpdaterProgram>>();
        CurrentUpdaterService = host.Services.GetRequiredService<UpdaterService>();
        logger.LogInformation("Hello World!");
        ParseLaunchArgs(args);
    }
    static string CheckSubscriptionKey(ArgumentResult result)
    {
        var content = result.Tokens.Single().Value;
        if (Launch.launchConfig.subscriptions.ContainsKey(content))
        {
            result.ErrorMessage = "";
            return null;
        }
        else return result.Tokens.Single().Value;
    }
    static void ParseLaunchArgs(string[] args)
    {
        #region subscribe <json/kvp/unsubscribe/list>
        var jsonSubscribeArgument = new Argument<FileInfo>()
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne
        };

        var kvpSubscribeProvider = new Option<IUpdateMessageProvider>(new string[] { "-pvd", "--provider" }, parseArgument: result =>
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
            Arity = ArgumentArity.ExactlyOne
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

        var unsubscribeKeyArgument = new Argument<string>(parse: CheckSubscriptionKey)
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne
        };

        var listKeyArgument = new Argument<string>(parse: result =>
        {
            if (result.Tokens.Single().Value == "")
                return "%all";
            return CheckSubscriptionKey(result);
        })
        {
            Description = "",
            Arity = ArgumentArity.ZeroOrOne
        };
        #endregion

        #region update
        var updateArgument = new Argument<string>("key", parse: CheckSubscriptionKey)
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
            updateAllCommand,
            subscribeCommand,
            scheduleCommand,
            updateCommand
        };
        kvpSubscribeCommand.SetHandler((args,dir,key,program,pvder,subpvder,ver) => { }, kvpSubscribeArgs, kvpSubscribeDirectory, kvpSubscribeKey, kvpSubscribeProgram, kvpSubscribeProvider, kvpSubscribeSubprovider, kvpSubscribeVersion);
        scheduleInitCommand.SetHandler(new Action(async() =>
            await host.Services.GetRequiredService<UpdaterService>().RegisterScheduleTasks()));
        updateAllCommand.SetHandler(() =>
        {
            CurrentUpdaterService.UpdateAllWhenAvailable(Launch.launchConfig.subscriptions);
        });
        jsonSubscribeCommand.SetHandler((json) => {
            SubscriptionController.RegisterSubscriptionByJson(json);
        }, jsonSubscribeArgument);
        listSubscribeCommand.SetHandler((key) =>
        {
            logger.LogInformation(JsonConvert.SerializeObject(Launch.launchConfig.subscriptions, Formatting.Indented));
        }, listKeyArgument);
        updateCommand.SetHandler((key) =>
        {
            CurrentUpdaterService.UpdateWhenAvailable(Launch.launchConfig.subscriptions[key]);
        }, updateArgument);
        unsubscribeCommand.SetHandler((key) =>
        {
            if (Launch.launchConfig.subscriptions.Remove(key))
            {
                logger.LogInformation("Successfully unsubscribe {key}", key);
            }
            else
            {
                logger.LogWarning("Failed to unsubscribe {key} because not found this value.", key);
            }
        }, unsubscribeKeyArgument);
        if (args.Length == 0) args = new string[] { "updateall"};
        root.Invoke(args);
        
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
