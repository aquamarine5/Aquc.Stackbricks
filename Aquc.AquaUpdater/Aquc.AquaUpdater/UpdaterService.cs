using Aquc.Netdisk.Aliyunpan;
using Huanent.Logging.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aquc.AquaUpdater;
public class UpdaterService : IHostedService
{
    public const string CONFIG_JSON = "Aquc.AquaUpdater.config.json";
    private readonly ILogger _logger;
    private readonly AliyunpanNetdisk aliyunpanNetdisk;
    [Obsolete]
    public readonly LaunchConfig configuration;
    public UpdaterService(ILogger<UpdaterService> logger,AliyunpanNetdisk aliyunpanNetdisk)
    {
        (_logger) = (logger );
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public void RegisterSubscription(SubscribeOption option)
    {
        Launch.launchConfig.subscriptions.Add(option.Key, SubscriptionController.ParseSubscribeOption(option));
        _logger.LogInformation("Successfully register {key}", option.Key);
    }
    public void RegisterSubscriptionByJson() { }
    public void UpdateWhenAvailable(string key)
    {
        if (Launch.launchConfig.subscriptions.TryGetValue(key, out UpdateSubscription valve))
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
        UpdateAllWhenAvailable(Launch.launchConfig.subscriptions);
    }
    public void UpdateAllWhenAvailable(Dictionary<string, UpdateSubscription> updateSubscriptions)
    {
        _logger.LogInformation("Update all subscriptions. Found {length}.", updateSubscriptions.Count);
        foreach (var item in updateSubscriptions.Values)
            UpdateWhenAvailable(item);
    }
    public async Task RegisterScheduleTasks()
    {
        
        await aliyunpanNetdisk.RegisterUpdateTokenSchtask();
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
        _logger.LogInformation("Success schedule subscriptions-update-all");
        process2.Dispose();
    }
}
