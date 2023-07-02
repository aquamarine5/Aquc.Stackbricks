using Aquc.Configuration.Abstractions;
using Aquc.Netdisk.Aliyunpan;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Aquc.AquaUpdater;
public class UpdaterService
{
    public const string CONFIG_JSON = "Aquc.AquaUpdater.config.json";
    private readonly ILogger _logger;
    public static IConfigurationSource<LaunchConfig> _configuration;
    private readonly AliyunpanNetdisk aliyunpanNetdisk;

    public UpdaterService(ILogger<UpdaterService> logger, AliyunpanNetdisk aliyunpanNetdisk, IConfigurationSource<LaunchConfig> configuration)
    {
        (_logger, this.aliyunpanNetdisk,_configuration) = (logger,aliyunpanNetdisk, configuration);
    }

    public void RegisterSubscription(SubscribeOption option)
    {
        using var flow = _configuration.GetFlow();
        flow.Data.subscriptions.Add(option.Key, SubscriptionController.ParseSubscribeOption(option));
        _logger.LogInformation("Successfully register {key}", option.Key);
    }

    public void RegisterSubscriptionByJson() { }

    public void UpdateWhenAvailable(string key)
    {
        if (_configuration.Data.subscriptions.TryGetValue(key, out UpdateSubscription valve))
        {
            UpdateWhenAvailable(valve);
        }
        else
        {
            throw new KeyNotFoundException(key);
        }
    }
    public void UpdateWhenAvailable(UpdateSubscription updateSubscription)
    {
        var msg = updateSubscription.GetUpdateMessage();
        _logger.LogInformation("{key} currently version is {cv}. Get {nv}.", updateSubscription.programKey, updateSubscription.currentlyVersion, msg.packageVersion);
        if (msg.NeedUpdate())
        {
            _logger.LogInformation("{key} have new version {version} to use", updateSubscription.programKey, msg.packageVersion);
            msg.GetUpdatePackage().InstallPackage();
        }
        else
        {
            //_logger.LogInformation("");
        }
    }
    public void UpdateAllWhenAvailable()
    {
        UpdateAllWhenAvailable(_configuration.Data.subscriptions);
    }
    public void UpdateAllWhenAvailable(Dictionary<string, UpdateSubscription> updateSubscriptions)
    {
        _logger.LogInformation("Update all subscriptions. Found {length}.", updateSubscriptions.Count);
        foreach (var item in updateSubscriptions.Values)
            UpdateWhenAvailable(item);
    }
    public async Task RegisterScheduleTasks()
    {

        _logger.LogInformation("111");
        aliyunpanNetdisk.RegisterUpdateTokenSchtask();
        _logger.LogInformation("111");
        using var process2 = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "schtasks",
                Arguments = $"/Create /F /SC weekly /D MON /TR \"'{Environment.ProcessPath + "' updateall"}\" /TN \"Aquacore\\Aquc.AquaUpdater.CheckSubscrptionsUpdate\"",
                CreateNoWindow = true
            }
        };
        process2.Start();
        await process2.WaitForExitAsync();
        _logger.LogInformation("Success schedule subscriptions-update-all");
    }
}
