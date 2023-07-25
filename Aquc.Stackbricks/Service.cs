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

    [Obsolete]
    public StackbricksService(StackbricksConfig stackbricksConfig)
    {
        this.stackbricksConfig = stackbricksConfig;
        stackbricksManifest = stackbricksConfig.StackbricksManifest;
        programManifest = stackbricksConfig.ProgramManifest;
    }
    public StackbricksService()
    {
        if (File.Exists(StackbricksConfig.CONFIG_FILENAME))
        {
            using var fs = new FileStream(StackbricksConfig.CONFIG_FILENAME, FileMode.Open, FileAccess.Read);
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
    protected void ApplyStackbricksUpdate(FileInfo newFileInfo,Version version)
    {
        var resultFile = ".Aquc.Stackbricks.applyres";
        if (File.Exists(resultFile)) File.Delete(resultFile);
        var command=
            "timeout /t 5 /nobreak && "+
            $"cd /d {newFileInfo.DirectoryName} && "+
            $"rename {newFileInfo.Name} {PROGRAM_NAME} && "+
            $"echo success_executed:{version} > {resultFile}";
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                CreateNoWindow = true
            }
        };
        process.Start();
        Environment.Exit(0);
    }
    public async Task<bool> UpdateWhenAvailable()
    {
        var manifest = programManifest;
        var message = await manifest.GetMsgPvder().GetUpdateMessage(manifest);
        if (message.NeedUpdate())
        {
            var package = await message.GetPkgPvder().DownloadPackageAsync(message, manifest.ProgramDir.FullName);
            package.ExecuteActions();
            await UpdateManifest(message);
            return true;
        }
        else
        {
            await UpdateCheckedManifest();
            return false;
        }
    }
    public async Task<bool> UpdateStackbricksWhenAvailable()
    {
        var manifest = stackbricksManifest;
        var message = await manifest.GetMsgPvder().GetUpdateMessage(manifest);
        if (message.NeedUpdate())
        {
            var file = await message.GetPkgPvder()
                .DownloadFileAsync(message, manifest.ProgramDir.FullName, $".Aquc.Stackbricks.updated_{message.version}.exe");
            using var fs = new FileStream(StackbricksApplyUpdateConfig.APPLYUPDATE_FILENAME, FileMode.Create, FileAccess.Write);
            using var sw = new StreamWriter(fs);
            await sw.WriteAsync(JsonConvert.SerializeObject(new StackbricksApplyUpdateConfig(file.FullName), StackbricksProgram.jsonSerializer));
            await UpdateManifest(message, false);
            ApplyStackbricksUpdate(file, message.version);
            return true;
        }
        else
        {
            await UpdateCheckedManifest(false);
            return false;
        }
    }
    public async Task UpdateCheckedManifest(bool isProgram = true)
    {
        var manifest = isProgram ? stackbricksConfig.ProgramManifest : stackbricksConfig.StackbricksManifest;
        manifest.LastCheckTime = DateTime.Now;
        await WriteConfig();
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
