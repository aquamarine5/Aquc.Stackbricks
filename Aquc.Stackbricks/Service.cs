using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public class StackbricksService
{
    public StackbricksConfig stackbricksConfig;
    public StackbricksManifest stackbricksManifest;
    public StackbricksManifest programManifest;

    public StackbricksService(StackbricksConfig stackbricksConfig)
    {
        this.stackbricksConfig= stackbricksConfig;
        stackbricksManifest = stackbricksConfig.StackbricksManifest;
        programManifest = stackbricksConfig.ProgramManifest;
    }
    public StackbricksService()
    {
        if (File.Exists("Aquc.Stackbricks.config.json"))
        {
            using var fs = new FileStream("Aquc.Stackbricks.config.json", FileMode.Open, FileAccess.Read);
            using var sr = new StreamReader(fs);
            stackbricksConfig = JsonConvert.DeserializeObject<StackbricksConfig>(sr.ReadToEnd())!;
            stackbricksManifest = stackbricksConfig.StackbricksManifest;
            programManifest = stackbricksConfig.ProgramManifest;
        }
        else
            throw new FileNotFoundException("Aquc.Stackbricks.config.json");
    }
    public async Task<bool> UpdateWhenAvailable()
    {
        var stackbricksManifest = programManifest;
        var message = await stackbricksManifest.GetMsgPvder().GetUpdateMessage(stackbricksManifest);
        if (message.NeedUpdate())
        {
            var package = await message.GetPkgPvder().DownloadPackageAsync(message, "");
            package.ExecuteActions();
            return true;
        }
        else return false;
    }
    public async Task<bool> UpdateStackbricksWhenAvailable()
    {
        return await Task.FromResult(true);
    }
}
