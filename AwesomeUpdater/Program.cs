using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AwesomeCore;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using IWshRuntimeLibrary;
using TaskScheduler;
using File = System.IO.File;

namespace AwesomeCore
{
    public class AescWebRequest
    {
        public static string WebRequestGet(string webUrl)
        {
            HttpWebRequest webRequest = WebRequest.CreateHttp(webUrl);
            webRequest.Method = "GET";
            WebResponse webResponse = webRequest.GetResponse();
            StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
            string result = streamReader.ReadToEnd();
            streamReader.Close();
            return result;
        }
        public static string WebRequestPut(string webUrl)
        {
            HttpWebRequest webRequest = WebRequest.CreateHttp(webUrl);
            webRequest.Method = "PUT";
            webRequest.Headers[HttpRequestHeader.Cookie] =
                "Hm_lvt_5583c41c8e3159d9302af01337fb1909=1636808565;" +
                " path_tmp=; Hm_lpvt_5583c41c8e3159d9302af01337fb1909=1636808682;" +
                " cloudreve-session=MTYzNjgwODY3N3xOd3dBTkVOTVExWklOVmhOVjBZeU5GZENVRFZQTmpaT1JrSlJTRWt6TTBkSlJqTkJTVVpOTkU5QlNGTlRUbGhEUms1Sk0wcEJUa0U9fKJqkrr6JiA-2auw6dbHvy4amMFvjYqvPfKpNin9WuVF";
            WebResponse webResponse = webRequest.GetResponse();
            StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
            string result = streamReader.ReadToEnd();
            streamReader.Close();
            return result;
        }
        public static void WebRequestDownload(string webUrl, string filePath)
        {
            HttpWebRequest webRequest = WebRequest.CreateHttp(webUrl);
            webRequest.Method = "GET";
            WebResponse webResponce = webRequest.GetResponse();
            Stream stream = webResponce.GetResponseStream();
            if (File.Exists(filePath))
                File.Delete(filePath);
            Console.WriteLine(filePath);
            FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            byte[] byteSuffer = new byte[1024];
            int totalSize = 0;
            int size = stream.Read(byteSuffer, 0, byteSuffer.Length);
            while (size > 0)
            {
                totalSize += size;
                fileStream.Write(byteSuffer, 0, size);
                size = stream.Read(byteSuffer, 0, byteSuffer.Length);
            }
            fileStream.Flush();
            fileStream.Close();
        }
    }
}

namespace AwesomeUpdater
{
    class Program
    {
        public interface IUpdateMessageProvider
        {
            string Name { get; }
            UpdateMessage GetUpdateMessage(string value);
        }
        public struct UpdateMessage
        {
            public UpdateMessage(string versionCode, string updatePackageUrl)
            {
                VersionCode = versionCode;
                UpdatePackageUrl = updatePackageUrl;
            }
            public string VersionCode { get; }
            public string UpdatePackageUrl { get; }
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
                return new UpdateMessage(list[0], jsonObjectNetdisk["data"].ToString());
            }
        }
        public static void CreateQuickLink(string arguments = "")
        {
            WshShell wshShell = new WshShell();
            string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            IWshShortcut link = wshShell.CreateShortcut($"{startMenuPath}/AwesomeUpdater.lnk") as IWshShortcut;
            link.TargetPath = Process.GetCurrentProcess().MainModule.FileName;
            link.Arguments = arguments;
            link.Description = "AwesomeUpdater";
            link.Save();
        }
        static void Main(string[] args)
        {
            List<string> argsList = new List<string>(args);
            Console.WriteLine(argsList.Count);
            if (argsList.Count == 0)
            {
                string programPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\AwesomePPTServices";
                DirectoryInfo directory = new DirectoryInfo(programPath);
                if (!directory.Exists) directory.Create();
                string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                CreateQuickLink($"-msgSrc BilibiliProvider -msgId 592590952959206542 -updPath {programPath.Replace(" ", "&nbsp;")} -updNow -updBeforeRun AwesomePPTServices.exe");
                Process process = new Process();
                process.StartInfo.FileName = "schtasks";
                process.StartInfo.Arguments = $"/Create /TR \"{startMenuPath}/AwesomeUpdater.lnk\" /TN AwesomeCore\\AwesomeUpdaterTask /SC DAILY /ST 10:00";
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                Environment.Exit(0);
            }
            int length = argsList.Count;
            int msgSrcIndex = argsList.FindIndex(arg => arg.Equals("-msgSrc"));
            int msgIdIndex = argsList.FindIndex(arg => arg.Equals("-msgId"));
            int updPathIndex = argsList.FindIndex(arg => arg.Equals("-updPath"));
            string updPath = argsList[updPathIndex + 1].Replace("&nbsp;","");
            IUpdateMessageProvider messageProvider = GetMessageProvider(argsList[msgSrcIndex + 1]);
            messageProvider.GetUpdateMessage(argsList[msgIdIndex + 1]).DownloadPackage($"{updPath}/UpdatedFile.zip", $"{updPath}/UpdatedFile");
            if (argsList.Contains("-updNow"))
            {
                UpdatePackage($"{updPath}/UpdatedFile", updPath);
            }
            if (argsList.Contains("-updBeforeRun"))
            {
                int updRunIndex = argsList.FindIndex(arg => arg.Equals("-updBeforeRun"));
                Process.Start(Path.Combine(updPath,argsList[updPathIndex + 1]));
                Environment.Exit(0);
            }
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
