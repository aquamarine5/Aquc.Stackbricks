using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Aquc.AquaUpdater;

public class SubscriptionController
{
    public const int NEWEST_SUBSCRIPTION_VERSION = 4;
    static bool CheckOption(SubscribeOption option, ILogger logger)
    {
        if (Directory.Exists(option.Program))
        {
            option.Directory = option.Program;
            option.Program = null;
        }
        if (!File.Exists(option.Program))
        {
            logger.LogError("{value} is a invalid file path.", option.Program);
            return false;
        }
        if (!Directory.Exists(option.Directory))
        {
            logger.LogError("{value} is a invalid directory path.", option.Directory);
            return false;
        }
        if (option.Directory == null)
        {
            if (option.Program == null)
            {
                logger.LogError("-Program or -Directory option is necessary.");
                return false;
            }
            else
            {
                option.Directory = Path.GetDirectoryName(option.Program);
            }
        }
        if (option.Version == null)
        {
            if (option.Program == null)
            {
                logger.LogError("-Program option is necessary if -Version value is not given.");
                return false;
            }
            var ver = FileVersionInfo.GetVersionInfo(option.Program).FileVersion;
            if (ver == null)
            {
                logger.LogError("Failed to get program version. Check -Program value is a valid program or set -Version option.");
                return false;
            }
            else
            {
                option.Version = ver;
            }
        }
        if (option.Provider == null)
        {
            if (option.Subprovider == null)
            {
                logger.LogError("-Provider value is necessary.");
                return false;
            }
            else
            {
                option.Provider = option.Subprovider;
                option.Subprovider = null;
                logger.LogWarning("-Provider is not found but -Subprovider is found. Use -Subprovider value as -Provider value and subprovier is now set to null.");
            }
        }
        if (option.Args == null)
        {
            logger.LogWarning("-Args is not found. Please confirm every provider does not need any parameters.");
            option.Args = "";
        }
        option.Key ??= Path.GetFileName(option.Program ?? option.Directory);
        return true;
    }
    static UpdateSubscription ParseSubscribeOption(SubscribeOption option)
    {
        return new UpdateSubscription()
        {
            programDirectory = new DirectoryInfo(option.Directory),
            SubscriptionVersion = NEWEST_SUBSCRIPTION_VERSION,
            args = option.Args,
            programExtrancePath = option.Program == null ? null : new FileInfo(option.Program),
            currentlyVersion = new Version(option.Version),
            lastCheckUpdateTime = new DateTime(0),
            programKey = option.Key,
            updateMessageProvider = Provider.GetMessageProvider(option.Provider),
            secondUpdateMessageProvider = Provider.GetMessageProvider(option.Subprovider)
        };
    }
    public static bool RegisterSubscription(SubscribeOption option)
    {
        var logger = Logging.InitLogger<SubscriptionController>();

        if (option.Json != null)
            option = ParseSubscribeJson(option.Json, logger);
        if (option == null) return false;
        CheckOption(option, logger);
        Launch.launchConfig.subscriptions.Add(option.Key, ParseSubscribeOption(option));
        Launch.UpdateLaunchConfig();
        return true;
    }
    public static SubscribeOption ParseSubscribeJson(string jsonPath, ILogger logger)
    {
        if (!File.Exists(jsonPath))
        {
            logger.LogError("-Json {value} is not a valid file.", jsonPath);
            return null;
        }
        using var file = new FileStream(jsonPath, FileMode.Open, FileAccess.Read);
        using var stream = new StreamReader(file);
        return JsonConvert.DeserializeObject<SubscribeOption>(stream.ReadToEnd());
    }
}
