using Aquc.Stackbricks.DataClass;
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

    class StackbricksUpdateResult
    {
        public StackbricksUpdateMessage updateMessage;
        public StackbricksUpdatePackage? updatePackage;
        public StackbricksUpdateResult(StackbricksUpdateMessage updateMessage, StackbricksUpdatePackage? updatePackage)
        {
            this.updateMessage = updateMessage;
            this.updatePackage = updatePackage;
        }
        public StackbricksUpdateResult(StackbricksUpdateMessage updateMessage)
        {
            this.updateMessage = updateMessage;
            updatePackage = null;
        }
    }

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
    public async Task<bool> Update(bool showToast = true)
    {
        var result = await BuiltinUpdate();
        if (showToast)
        {
            if (result.updateMessage.NeedUpdate())
                ShowUpdatedUWPToast(result.updateMessage);
            else
                ShowNewestUWPToast(result.updateMessage);
        }
        return result.updateMessage.NeedUpdate();
    }
    public async Task<UpdateDataClass> UpdateDC(bool showToast = true)
    {
        var result = await BuiltinUpdateStackbricks();
        if (showToast)
        {
            if (result.updateMessage.NeedUpdate())
                ShowUpdatedUWPToast(result.updateMessage);
            else
                ShowNewestUWPToast(result.updateMessage);
        }
        if (result.updateMessage.NeedUpdate())
            return DataClassParser.ParseUpdateDC(result.updatePackage!, true);
        else
            return DataClassParser.ParseUpdateDC(result.updateMessage, true);
    }
    async Task<StackbricksUpdateResult> BuiltinUpdate()
    {
        var manifest = programManifest;
        var message = await manifest.GetMsgPvder().GetUpdateMessageAsync(manifest);
        if (message.NeedUpdate())
        {
            StackbricksProgram.logger.Information($"Got {message.version} to update, currently version is {manifest.Version}");
            var package = await message.GetPkgPvder().DownloadPackageAsync(message, manifest.ProgramDir.FullName);
            package.ExecuteActions();
            await UpdateManifest(message);

            return new StackbricksUpdateResult(message, package);
        }
        else
        {
            StackbricksProgram.logger.Information("No newest version to update");
            await UpdateCheckedManifest();

            return new StackbricksUpdateResult(message);
        }
    }

    async Task<StackbricksUpdateResult> BuiltinUpdateStackbricks()
    {
        var manifest = stackbricksManifest;
        var message = await manifest.GetMsgPvder().GetUpdateMessageAsync(manifest);
        if (message.NeedUpdate())
        {
            StackbricksProgram.logger.Information($"Got {message.version} to update, currently version is {manifest.Version}");
            var file = await message.GetPkgPvder()
                .DownloadFileAsync(message, manifest.ProgramDir.FullName, $".Aquc.Stackbricks.updated_{message.version}.exe");
            file.ExecuteActions();
            await UpdateManifest(message, false);
            return new StackbricksUpdateResult(message, file);
        }
        else
        {
            StackbricksProgram.logger.Information($"Received {message.version}, currently version is {manifest.Version}. No newest version to update.");
            await UpdateCheckedManifest(false);
            return new StackbricksUpdateResult(message);
        }
    }
    public async Task<bool> UpdateStackbricks(bool showToast = true)
    {
        var result = await BuiltinUpdateStackbricks();
        if (showToast)
        {
            if (result.updateMessage.NeedUpdate())
                ShowUpdatedUWPToast(result.updateMessage);
            else
                ShowNewestUWPToast(result.updateMessage);
        }
        return result.updateMessage.NeedUpdate();
    }
    public async Task<UpdateDataClass> UpdateStackbricksDC(bool showToast = true)
    {
        var result = await BuiltinUpdateStackbricks();

        if (showToast)
        {
            if (result.updateMessage.NeedUpdate())
                ShowUpdatedUWPToast(result.updateMessage);
            else
                ShowNewestUWPToast(result.updateMessage);
        }
        if (result.updateMessage.NeedUpdate())
            return DataClassParser.ParseUpdateDC(result.updatePackage!, false);
        else
            return DataClassParser.ParseUpdateDC(result.updateMessage, false);
    }
    public async Task UpdateCheckedManifest(bool isProgram = true)
    {
        var manifest = isProgram ? stackbricksConfig.ProgramManifest : stackbricksConfig.StackbricksManifest;
        manifest.LastCheckTime = DateTime.Now;
        await WriteConfig();
        StackbricksProgram.logger.Debug($"Write config to {StackbricksConfig.CONFIG_FILENAME}");
    }

    private async Task UpdateManifest(StackbricksUpdateMessage msg, bool isProgram = true)
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
