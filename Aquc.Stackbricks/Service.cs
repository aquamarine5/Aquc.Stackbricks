using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public class StackbricksService
{
    public StackbricksConfig stackbricksConfig;
    public StackbricksManifest stackbricksManifest;
    public StackbricksManifest programManifest;

    public const string PROGRAM_NAME = "Aquc.Stackbricks.exe";

    public StackbricksService()
    {
        if (File.Exists(StackbricksConfig.CONFIG_FILENAME))
        {
            using var fs = new FileStream(StackbricksConfig.CONFIG_FILENAME, FileMode.Open, FileAccess.Read); //?
            using var sr = new StreamReader(fs);
            stackbricksConfig = JsonConvert.DeserializeObject<StackbricksConfig>(sr.ReadToEnd(), StackbricksProgram.jsonSerializer)!;
            stackbricksManifest = stackbricksConfig.StackbricksManifest;
            programManifest = stackbricksConfig.ProgramManifest;
        }
        else
        {
            StackbricksProgram.logger.Error("Aquc.Stackbricks.config.json was not found.");
            throw new FileNotFoundException(StackbricksConfig.CONFIG_FILENAME);
        }
    }

    static void ShowUpdatedUWPToast(StackbricksUpdateMessage message)
    {
        new ToastContentBuilder()
            .AddText($"{message.stackbricksManifest.Id} 已成功更新至版本 {message.version}")
            .Show();
    }
    static void ShowNewestUWPToast(StackbricksUpdateMessage message)
    {
        new ToastContentBuilder()
            .AddText($"{message.stackbricksManifest.Id} 已经是最新版本")
            .Show();
    }
    public async Task<bool> UpdateWhenAvailable(bool showToast = true)
    {
        
        var manifest = programManifest;
        var message = await manifest.GetMsgPvder().GetUpdateMessage(manifest);
        if (message.NeedUpdate())
        {
            StackbricksProgram.logger.Information($"Got {message.version} to update, currently version is {manifest.Version}");
            var package = await message.GetPkgPvder().DownloadPackageAsync(message, manifest.ProgramDir.FullName);
            package.ExecuteActions();
            await UpdateManifest(message);

            if (showToast)
                ShowUpdatedUWPToast(message);
            return true;
        }
        else
        {
            StackbricksProgram.logger.Information("No newest version to update");
            await UpdateCheckedManifest();
            if (showToast)
                ShowNewestUWPToast(message);
            return false;
        }
    }
    public async Task<bool> UpdateStackbricksWhenAvailable(bool showToast=true)
    {
        var manifest = stackbricksManifest;
        var message = await manifest.GetMsgPvder().GetUpdateMessage(manifest);
        if (message.NeedUpdate())
        {
            StackbricksProgram.logger.Information($"Got {message.version} to update, currently version is {manifest.Version}");
            var file = await message.GetPkgPvder()
                .DownloadFileAsync(message, manifest.ProgramDir.FullName, $".Aquc.Stackbricks.updated_{message.version}.exe");
            file.ExecuteActions();
            await UpdateManifest(message, false);
            if (showToast)
                ShowUpdatedUWPToast(message);
            return true;
        }
        else
        {
            StackbricksProgram.logger.Information($"Received {message.version}, currently version is {manifest.Version}. No newest version to update.");
            await UpdateCheckedManifest(false);
            if (showToast)
                ShowNewestUWPToast(message);
            return false;
        }
    }
    public async Task UpdateCheckedManifest(bool isProgram = true)
    {
        var manifest = isProgram ? stackbricksConfig.ProgramManifest : stackbricksConfig.StackbricksManifest;
        manifest.LastCheckTime = DateTime.Now;
        await WriteConfig();
        StackbricksProgram.logger.Debug($"Write config to {StackbricksConfig.CONFIG_FILENAME}");
    }
    public async Task UpdateManifest(StackbricksUpdateMessage msg, bool isProgram = true)
    {
        var manifest = isProgram ? stackbricksConfig.ProgramManifest : stackbricksConfig.StackbricksManifest;
        manifest.LastCheckTime = DateTime.Now;
        manifest.LastUpdateTime = DateTime.Now;
        manifest.Version = msg.version;
        await WriteConfig();
    }
    private async Task WriteConfig()
    {
        using var fs = new FileStream(StackbricksConfig.CONFIG_FILENAME, FileMode.Truncate, FileAccess.ReadWrite);
        using var sw = new StreamWriter(fs);
        await sw.WriteAsync(JsonConvert.SerializeObject(stackbricksConfig, StackbricksProgram.jsonSerializer));
    }
}
