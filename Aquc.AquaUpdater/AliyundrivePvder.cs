using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            throw new NotImplementedException();
        }
    }
    public class AliyunpanInteraction
    {
        string exePath;
        public AliyunpanInteraction() =>
            new AliyunpanInteraction(Path.Combine(
                    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), 
                    Launch.LaunchConfig.implementations["aliyunpan"].folder));

        public AliyunpanInteraction(string folder)
        {
            exePath = Path.Combine(folder, "aliyunpan.exe");
            Console.WriteLine(exePath);
            Console.WriteLine(1);
        }
        public string DownloadFile(string drivePath,string downloadPath)
        {
            var task = Task.Run(async () =>
            {
                if (await ProcessLoglist())
                {
                    await ProcessLogin();
                }
                return await ProcessDownload(drivePath, downloadPath);
            });
            Console.WriteLine("Result "+task.IsCompleted+":"+task.Result);
            return task.Result;
        }
        bool wait4loglist = false;
        bool wait4login = false;
        bool wait4download = false;
        bool wait4downloadresult = false;
        bool resultLogin;
        readonly string token="3eebc598a03c40e9a1384e713df3c247";
        private async Task ProcessInvoke(string args,string exePath)
        {
            Console.WriteLine(exePath);
            await Task.Run(() =>
            {
                var process = new Process();
                process.StartInfo.FileName = @"D:\Program Source\v2\Aquc.AquaUpdater\Aquc.AquaUpdater\bin\Debug\netcoreapp3.1\aliyunpan-v0.2.5-windows-x86\aliyunpan.exe";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Arguments = args;
                //process.StartInfo.UseShellExecute = false;
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
            Console.WriteLine("processloglist");
            wait4loglist = true;
            await ProcessInvoke("loglist",exePath);
            return await Task.FromResult(resultLogin);
            
        }
        private async Task<string> ProcessDownload(string drivePath,string downloadPath)
        {
            Console.WriteLine("processdownload");
            wait4download = true;
            await ProcessInvoke("download " + drivePath+" --saveto "+downloadPath, exePath);
            return await Task.FromResult(Path.Combine(downloadPath, Path.GetFileName(drivePath)));

        }
        private async Task ProcessLogin()
        {
            Console.WriteLine("processlogin");
            wait4login = true;
            await ProcessInvoke("login -RefreshToken=" + token, exePath);

        }
        private void ProcessExited(object sender, EventArgs e)
        {
            Console.WriteLine("processexited");
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
                if (e.Data.Contains(token)) resultLogin = true;
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
