using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.PkgPvder;

public class GhProxyPkgPvder : IStackbricksPkgPvder
{
    public string PkgPvderId => "stbks.pkgpvder.ghproxy";
    public StackbricksUpdateMessage updateMessage;
    public async Task<StacebricksUpdatePackage> DownloadPackageAsync(string data,string savePosition)
    {
        // ncpe
        var splitedData = data.Split("]]");
        var downloadFile = $".StackbricksUpdatePackage_{updateMessage.version}.zip";
        var responce = await Program._httpClient.GetAsync(CombineGhproxyUrl(splitedData));
        using var fs = new FileStream(Path.Combine(savePosition, downloadFile), FileMode.Create);
        await responce.Content.CopyToAsync(fs);
        return new StacebricksUpdatePackage(downloadFile, updateMessage, updateMessage.stackbricksManifest.programDir);
    }
    static string CombineGhproxyUrl(string[] data)
    {
        return $"https://ghproxy.com/github.com/{data[0]}/{data[1]}/releases/download/{data[2]}/{data[3]}";
    }
}
