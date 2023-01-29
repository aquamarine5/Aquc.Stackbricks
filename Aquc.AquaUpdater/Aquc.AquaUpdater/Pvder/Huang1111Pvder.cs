using Aquc.Netdisk.Huang1111;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.AquaUpdater.Pvder;

public class Huang1111Pvder : IUpdateFilesProvider
{
    public string Identity => "huang1111filepvder";

    public UpdatePackage DownloadPackage(UpdateMessage updateMessage)
    {
        var task = Huang1111Netdisk.Download(updateMessage.fileArgs, Directory.GetCurrentDirectory(), $"{updateMessage.packageVersion}.zip");
        task.Wait();

        ZipFile.ExtractToDirectory(task.Result, Path.Combine(Directory.GetCurrentDirectory(),$"update_{updateMessage.packageVersion}"));
        Logging.UpdatePackageLogger.LogInformation("extract update zip successfully: {ezd}", $"update_{updateMessage.packageVersion}");
        return new UpdatePackage
        {
            zipPath = new FileInfo(task.Result),
            updateSubscription = updateMessage.updateSubscription,
            updateMessage = updateMessage,
            extraceZipDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), $"update_{updateMessage.packageVersion}"))
        };
    }
}
