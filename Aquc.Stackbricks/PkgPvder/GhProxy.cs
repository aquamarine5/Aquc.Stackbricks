using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.PkgPvder;

public class GhProxyPkgPvder : IStackbricksPkgPvder
{

    public string PkgPvderId => ID;
    public const string ID = "stbks.pkgpvder.ghproxy";

    public async Task<UpdatePackage> DownloadPackageAsync(UpdateMessage updateMessage, string savePosition, string zipFileName)
    {
        // ncpe
        var splitedData = updateMessage.PkgPvderArgs.Split("]]");
        var downloadFile = Path.Combine(savePosition, zipFileName);
        await DownloadAsync(CombineGhproxyUrl(splitedData), Path.Combine(savePosition, downloadFile));
        StackbricksProgram.logger.Debug($"{ID}: Download zipPackageFile successfull, file={zipFileName}");
        return new UpdatePackage(downloadFile, updateMessage);
    }
    public async Task<UpdatePackage> DownloadPackageAsync(UpdateMessage updateMessage, string savePosition) =>
        await DownloadPackageAsync(updateMessage, savePosition, $".StackbricksUpdatePackage_{updateMessage.version}.zip");
    static async Task DownloadAsync(string url, string savePosition)
    {
        var dir = Path.GetDirectoryName(savePosition);
        if (!Directory.Exists(dir) && dir != null) Directory.CreateDirectory(dir);

        var responce = await StackbricksProgram.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        using var fs = new FileStream(savePosition, FileMode.Create, FileAccess.ReadWrite);
        using var sr = await responce.Content.ReadAsStreamAsync();
        var totalRead = 0L;
        var buffer = new byte[10240];
        var isMoreToRead = true;
        float percentComplete;
        do
        {
            long? length = responce.Content.Headers.ContentLength;
            string? filename = responce.Content.Headers.ContentDisposition?.Name;
            string lengthText = ConvertByteToText(length);
            if (length == null)
            {
                StackbricksProgram.logger.Warning($"{ID}: Cannot get package content-length.");
            }
            else
            {
                StackbricksProgram.logger.Debug($"{ID}: Get {filename}, {lengthText}.");
            }
            var progressbar = Environment.GetCommandLineArgs().Contains("--log-progress");
            var read = await sr.ReadAsync(buffer);
            if (read == 0)
            {
                isMoreToRead = false;
            }
            else
            {
                await fs.WriteAsync(buffer.AsMemory(0, read));
                totalRead += read;

                if (progressbar && length != null)
                {
                    percentComplete = (float)((float)totalRead / length * 100);
                    int currentLineCursor = Console.CursorTop;
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine($"{ID}: Downloading file: {ConvertByteToText(totalRead)}/{lengthText} ({percentComplete:0.00}%)");
                    Console.SetCursorPosition(0, currentLineCursor);
                }
            }
        }
        while (isMoreToRead);
    }
    static string ConvertByteToText(long? length)
    {
        if (length == null) return "";
        float lengthf=(float)length;
        if (length < 1024)
            return length + "B";
        else if (length < 1048576)
            return (lengthf / 1024).ToString("0.00") + "KB";
        else if (length < 1073741824L)
            return (lengthf / 1048576).ToString("0.00") + "MB";
        else if (length < 1099511627776L)
            return (lengthf / 1073741824L).ToString("0.00") + "GB";
        else
            return (lengthf / 1099511627776L).ToString("0.00") + "TB";
    }
    static string CombineGhproxyUrl(string[] data)
    {
        var result = $"https://mirror.ghproxy.com/github.com/{data[0]}/{data[1]}/releases/download/{data[2]}/{data[3]}";
        StackbricksProgram.logger.Debug($"{ID}: Download link: {result}");
        return result;
    }

    public async Task<UpdatePackage> DownloadFileAsync(UpdateMessage updateMessage, string savePosition, string fileName = "")
    {
        var splitedData = updateMessage.PkgPvderArgs.Split("]]");
        var downloadFile = Path.Combine(savePosition, string.IsNullOrEmpty(fileName) ? splitedData[3] : fileName);
        await DownloadAsync(CombineGhproxyUrl(splitedData), Path.Combine(savePosition, downloadFile));
        StackbricksProgram.logger.Debug($"{ID}: Download file successfull, file={Path.GetFileName(downloadFile)}");
        return new UpdatePackage(downloadFile, updateMessage, false);
    }
}
