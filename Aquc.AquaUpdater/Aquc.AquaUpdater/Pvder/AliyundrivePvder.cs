using Aquc.Netdisk.Aliyunpan;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.AquaUpdater.Pvder;

//https://github.com/tickstep/aliyunpan/releases/download/v0.2.5/aliyunpan-v0.2.5-windows-x86.zip
public class AliyundrivePvder : IUpdateFilesProvider
{
    public string Identity => "aliyunfilepvder";
    public UpdatePackage DownloadPackage(UpdateMessage updateMessage)
    {
        var data = updateMessage.fileArgs.Split("]]");
        if (data.Length != 2)
        {
            Logging.UpdatePackageLogger.LogError("Failed to decode received data from {pvd}: {msg}",
                updateMessage.updateSubscription.updateMessageProvider, data);
            throw new ArgumentException("UpdateMessage_ReceivedData");
        }
        string filePath = data[1];
        string version = data[0];
        string zipDirectory = updateMessage.updateSubscription.programDirectory.FullName;
        string extractZipDirectory = Path.Combine(zipDirectory, $"update_{version}");
        var zipPath = ActivatorUtilities.GetServiceOrCreateInstance<AliyunpanNetdisk>(UpdaterProgram.serviceProvider)
            .Download(filePath, new DirectoryInfo(zipDirectory));
        zipPath.Wait();
        if (Directory.Exists(extractZipDirectory))
        {
            Directory.Delete(extractZipDirectory, true);
        }
        ZipFile.ExtractToDirectory(zipPath.Result, extractZipDirectory);
        Logging.UpdatePackageLogger.LogInformation("extract update zip successfully: {ezd}", extractZipDirectory);
        return new UpdatePackage()
        {
            updateMessage = updateMessage,
            updateSubscription = updateMessage.updateSubscription,
            extraceZipDirectory = new DirectoryInfo(extractZipDirectory),
            zipPath = new FileInfo(zipPath.Result)
        };
    }
}