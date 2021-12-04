using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Aesc.AwesomeKits;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using IWshRuntimeLibrary;
using TaskScheduler;
using File = System.IO.File;


namespace Aesc.AwesomeUpdater
{
    public class AescAwesomeUpdater
    {
        public UpdateConfig updateConfig;
        public AescAwesomeUpdater(UpdateConfig updateConfig)
        {
            if (!updateConfig.IsAvailable) throw new ArgumentException();
            this.updateConfig = updateConfig;
        }
        public virtual IUpdateMessageProvider GetMessageProvider()
        {
            return (updateConfig.messageProvider.ToLower()) switch
            {
                "bilibiliprovider" => new BilibiliUpdateMessageProvider(),
                _ => null,
            };
        }
        public virtual UpdateMessage GetUpdateMessage()
            => GetMessageProvider().GetUpdateMessage(updateConfig.messageData);
        public virtual bool CheckUpdate()
        {
            return true;
        }
        pubilc virtual void UpdatePackage(UpdateMessage updateMessage)
        {

        }
    }
    public interface IUpdateMessageProvider
    {
        string Name { get; }
        UpdateMessage GetUpdateMessage(string value);
    }
    public struct UpdateMessage
    {
        public string VersionCode;
        public string UpdatePackageUrl;
        public void DownloadPackage(string filePath, string extractPath)
        {
            AescWebRequest.WebRequestDownload(UpdatePackageUrl, filePath);
            DirectoryInfo directory = new DirectoryInfo(extractPath);
            if (directory.Exists)
            {
                directory.Delete(true);
            }
            ZipFile.ExtractToDirectory(filePath, extractPath, true);
        }
    }
    
    }
    public struct UpdateConfig
    {
        public string programInstallPath;
        public string programExe;
        public string programName;
        public int nowVersion;
        public string messageProvider;
        public string messageData;
        public bool IsAvailable
        {
            get => Directory.Exists(programInstallPath) && File.Exists(programExe);
        }
    }
    public struct UpdateLaunchConfig
    {
        public List<UpdateConfig> updateConfigs;
        
    }
    public struct UpdaterArgs : IArgsParseResult
    {
        public ArgsNamedKey Update;
        public ArgsNamedKey Config;
        public ArgsNamedKey updNow;
        public ArgsNamedKey allowUpdateOlder;
        
    }
    public class AescUpdaterProgram
    {
        static void Main(string[] args)
        {
            
        }
        static void Main2(string[] args)
        {
            List<string> argsList = new List<string>(args);
            Console.WriteLine(argsList.Count);
            if (argsList.Count == 0)
            {
                string programPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\AwesomePPTServices";

                DirectoryInfo directory = new DirectoryInfo(programPath);
                if (!directory.Exists) directory.Create();
                string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string programArgs = $"-msgSrc BilibiliProvider -msgId 592590952959206542 -updPath {programPath.Replace(" ", "&nbsp;")} -updNow -updBeforeRun AwesomePPTServices.exe";
                CreateQuickLink("AwesomeUpdater", programArgs);
                // todo:disabled start
                string pp = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\AwesomePPTServices"; // todo:disabled
                CreateQuickLink("Albedov1_1",
                    $"-msgSrc BilibiliProvider -msgId *** -updPath {pp.Replace(" ", "&nbsp;")} -updNow -updBeforeRun do.bat");
                Process processs = new Process();
                processs.StartInfo.FileName = "schtasks";
                processs.StartInfo.Arguments = $"/Create /TR \"'{Process.GetCurrentProcess().MainModule.FileName}' {programArgs}\" /TN AwesomeCore\\AlbedoJoke /SC WEEKLY /D TUE /ST 10:00";
                processs.StartInfo.CreateNoWindow = true;
                processs.Start();
                // todo:disabled end

                Process process = new Process();
                process.StartInfo.FileName = "schtasks";
                process.StartInfo.Arguments = $"/Create /TR \"'{Process.GetCurrentProcess().MainModule.FileName}' {programArgs}\" /TN AwesomeCore\\AwesomeUpdaterTask /SC WEEKLY /D MON /ST 10:00";
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                Environment.Exit(0);
            }
            int length = argsList.Count;
            int msgSrcIndex = argsList.IndexOf("-msgSrc");
            int msgIdIndex = argsList.IndexOf("-msgId");
            int updPathIndex = argsList.IndexOf("-updPath");
            string updPath = argsList[updPathIndex + 1].Replace("&nbsp;", "");
            IUpdateMessageProvider messageProvider = GetMessageProvider(argsList[msgSrcIndex + 1]);
            messageProvider.GetUpdateMessage(argsList[msgIdIndex + 1]).DownloadPackage($"{updPath}/UpdatedFile.zip", $"{updPath}/UpdatedFile");
            if (argsList.Contains("-updNow"))
            {
                UpdatePackage($"{updPath}/UpdatedFile", updPath);
            }
            if (argsList.Contains("-updBeforeRun"))
            {
                int updRunIndex = argsList.FindIndex(arg => arg.Equals("-updBeforeRun"));
                Process.Start(Path.Combine(updPath, argsList[updPathIndex + 1]));
                Environment.Exit(0);
            }
        }
        public static void UpdateSelf()
        {
            const int versionCodeNow = 4;
            string p = Process.GetCurrentProcess().MainModule.FileName;
            var up = new BilibiliUpdateMessageProvider().GetUpdateMessage("595405964716755484");
            if (int.Parse(up.VersionCode) > versionCodeNow)
            {
                up.DownloadPackage(
                    Path.Combine(Path.GetDirectoryName(p), "UpdatedFile\\downloadUpdate.zip"), Path.GetDirectoryName(p) + "\\UpdatedFile");
                Process.Start(Path.GetDirectoryName(p) + "\\UpdatedFile\\updateSelf.bat");
                Environment.Exit(0);
            }
        }
        public static void CreateQuickLink(string fileName, string arguments = "")
        {
            WshShell wshShell = new WshShell();
            string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            DirectoryInfo directory = new DirectoryInfo("D:/AwesomeCore");
            if (!directory.Exists) directory.Create();
            IWshShortcut link = wshShell.CreateShortcut($"{directory.FullName}/{fileName}.lnk") as IWshShortcut;
            link.TargetPath = Process.GetCurrentProcess().MainModule.FileName;
            link.Arguments = arguments;
            link.Description = "AwesomeUpdater";
            link.Save();
        }
        public static void UpdatePackage(string updFilePath, string targetPath)
        {
            DirectoryInfo directory = new DirectoryInfo(updFilePath);
            foreach (var item in directory.GetFiles())
            {
                item.MoveTo($"{targetPath}/{item.Name}", true);
            }
        }
        public static IUpdateMessageProvider GetMessageProvider(string msgSrcKey)
        {
            if (msgSrcKey == "BilibiliProvider")
            {
                return new BilibiliUpdateMessageProvider();
            }
            else return null;
        }

    }
}
