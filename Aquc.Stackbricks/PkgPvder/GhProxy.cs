using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.PkgPvder;

public class GhProxyPkgPvder : IStackbricksPkgPvder
{

    public string PkgPvderId => _PkgPvderId;

    public static readonly string _PkgPvderId = "stbks.pkgpvder.ghproxy";
    public async Task<StacebricksUpdatePackage> DownloadPackageAsync(StackbricksUpdateMessage updateMessage,string savePosition)
    {
        // ncpe
        var splitedData = updateMessage.PkgPvderArgs.Split("]]");
        var downloadFile = Path.Combine(savePosition, $".StackbricksUpdatePackage_{updateMessage.version}.zip");
        var responce = await StackbricksProgram._httpClient.GetAsync(CombineGhproxyUrl(splitedData));
        using var fs = new FileStream(Path.Combine(savePosition, downloadFile), FileMode.Create);
        
        await responce.Content.CopyToAsync(fs);
        fs.Dispose();
        return new StacebricksUpdatePackage(downloadFile, updateMessage, updateMessage.stackbricksManifest.ProgramDir);
    }
    static string CombineGhproxyUrl(string[] data)
    {
        return $"https://ghproxy.com/github.com/{data[0]}/{data[1]}/releases/download/{data[2]}/{data[3]}";
    }
}
