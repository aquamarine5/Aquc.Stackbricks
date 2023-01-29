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

namespace Aquc.AquaUpdater.Pvder
{
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
            var zipPath = ActivatorUtilities.GetServiceOrCreateInstance<AliyunpanNetdisk>(UpdaterProgram.host.Services)
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
    [Obsolete("Use Aquc.Netdisk.Aliyunpan")]
    public class AliyunpanInteraction
    {
        public string exePath;
        readonly ILogger<AliyunpanInteraction> logger;
        public AliyunpanInteraction() : this(Path.Combine(
                    Path.GetDirectoryName(Environment.ProcessPath),
                    Launch.launchConfig.implementations["aliyunpan"].folder))
        { }

        public AliyunpanInteraction(string folder)
        {
            exePath = Path.Combine(folder, "aliyunpan.exe");
            logger = Logging.InitLogger<AliyunpanInteraction>();
            logger.LogInformation("use aliyunpan implementation in: {exePath}", exePath);
        }
        public string DownloadFile(string drivePath, string downloadPath, string token)
        {
            this.token = token;
            var task = Task.Run(async () =>
            {
                if (!await ProcessLoglist())
                {
                    await ProcessLogin(token);
                }
                return await ProcessDownload(drivePath, downloadPath);
            });
            return task.Result;
        }
        bool wait4loglist = false;
        bool wait4login = false;
        bool wait4download = false;
        bool wait4downloadresult = false;
        bool resultLogin;
        string token = "";
        private async Task ProcessInvoke(string args)
        {
            await Task.Run(() =>
            {
                Logging.UpdatePackageLogger.LogInformation("run: {args}", args);
                var process = new Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Arguments = args;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.Start();
                process.StandardInput.AutoFlush = true;
                process.EnableRaisingEvents = true;
                process.Exited += ProcessExited;
                process.OutputDataReceived += ProcessOutputDataReceived;
                process.BeginOutputReadLine();
                process.WaitForExit();
            });
        }
        private async Task<bool> ProcessLoglist()
        {
            logger.LogInformation("processloglist");
            wait4loglist = true;
            await ProcessInvoke("loglist");
            return resultLogin;

        }
        private async Task<string> ProcessDownload(string drivePath, string downloadPath)
        {
            logger.LogInformation("processdownload");
            wait4download = true;
            await ProcessInvoke("download " + drivePath + $" --saveto \"{downloadPath}\"");
            return Path.Combine(downloadPath, Path.GetFileName(drivePath));

        }
        private async Task ProcessLogin(string token)
        {
            logger.LogInformation("processlogin");
            wait4login = true;
            await ProcessInvoke("login -RefreshToken=" + token);

        }
        private void ProcessExited(object sender, EventArgs e)
        {
            logger.LogInformation("processexited");
            if (wait4loglist)
            {
                wait4loglist = false;
                resultLogin = false;
            }
        }
        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            Console.WriteLine(e.Data);
            if (wait4loglist)
            {
                if (e.Data.Contains(token))
                {
                    resultLogin = true;
                    wait4loglist = false;
                    (sender as Process).Kill();
                }
            }
            if (wait4login)
            {
                if (e.Data.Contains("阿里云盘登录成功"))
                {
                    wait4login = false;
                }
            }
            if (wait4download)
            {
                if (e.Data.Contains("加入下载队列"))
                {
                    wait4download = false;
                    wait4downloadresult = true;
                }
            }
            if (wait4downloadresult)
            {
                if (e.Data.Contains("下载结束"))
                {
                    wait4downloadresult = false;
                }
            }
        }
    }
}
