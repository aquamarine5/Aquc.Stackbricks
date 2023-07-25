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

    public async Task<StackbricksUpdatePackage> DownloadPackageAsync(StackbricksUpdateMessage updateMessage, string savePosition, string zipFileName)
    {
        // ncpe
        var splitedData = updateMessage.PkgPvderArgs.Split("]]");
        var downloadFile = Path.Combine(savePosition, zipFileName);
        await DownloadAsync(CombineGhproxyUrl(splitedData), Path.Combine(savePosition, downloadFile));
        return new StackbricksUpdatePackage(downloadFile, updateMessage, updateMessage.stackbricksManifest.ProgramDir);
    }
    public async Task<StackbricksUpdatePackage> DownloadPackageAsync(StackbricksUpdateMessage updateMessage, string savePosition) =>
        await DownloadPackageAsync(updateMessage, savePosition, $".StackbricksUpdatePackage_{updateMessage.version}.zip");
    async Task DownloadAsync(string url, string savePosition)
    {
        var responce = await StackbricksProgram._httpClient.GetAsync(url);
        using var fs = new FileStream(savePosition, FileMode.Create);
        await responce.Content.CopyToAsync(fs);
    }
    static string CombineGhproxyUrl(string[] data)
    {
        return $"https://ghproxy.com/github.com/{data[0]}/{data[1]}/releases/download/{data[2]}/{data[3]}";
    }

    public async Task<FileInfo> DownloadFileAsync(StackbricksUpdateMessage updateMessage, string savePosition, string fileName = "")
    {
        var splitedData = updateMessage.PkgPvderArgs.Split("]]");
        var downloadFile = Path.Combine(savePosition, string.IsNullOrEmpty(fileName) ? splitedData[3] : fileName);
        await DownloadAsync(CombineGhproxyUrl(splitedData), Path.Combine(savePosition, downloadFile));
        return new FileInfo(downloadFile);
    }
}
