using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Aesc.AwesomeKits.RxWebRequest;
using Aesc.AwesomeKits.ArgsParser;
using Aesc.AwesomeKits.TaskScheduler;
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
    public class BilibiliUpdateMessageProvider : IUpdateMessageProvider
    {
        public string Name => GetType().Name;
        public readonly string commentsReplyUrl = "https://api.bilibili.com/x/v2/reply/main?jsonp=jsonp&next=0&type=17&mode=2&plat=1&oid";
        public readonly string commentsMainUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id";
        public UpdateMessage GetUpdateMessage(string value)
        {
            string resultMain = AescWebRequest.WebRequestGet($"{commentsMainUrl}={value}");
            string result = AescWebRequest.WebRequestGet($"{commentsReplyUrl}={value}");  //oid=592534916524618496
            JObject jsonObject = JObject.Parse(result);
            JObject jsonObjectMain = JObject.Parse(resultMain);
            Console.WriteLine(jsonObjectMain);
            JObject jsonObjectMainCard = JObject.Parse(jsonObjectMain["data"]["card"]["card"].ToString());
            string baseDownloadUrl = jsonObjectMainCard["item"]["content"].ToString();
            string realMessage = jsonObject["data"]["replies"][0]["content"]["message"].ToString();
            var list = realMessage.Split(':');
            string resultNetdisk = AescWebRequest.WebRequestPut(baseDownloadUrl.Replace("&downloadCode", list[1]));
            Console.WriteLine(resultNetdisk);
            JObject jsonObjectNetdisk = JObject.Parse(resultNetdisk);
            return new UpdateMessage()
            {
                VersionCode = list[0],
                UpdatePackageUrl = jsonObjectNetdisk["data"].ToString()
            };
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
