using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Aesc.AwesomeKits.Net.WebStorage;
using Aesc.AwesomeKits.Net;
using Aesc.AwesomeKits.ComUtil;
using Aesc.AwesomeKits.Util;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using IWshRuntimeLibrary;
using Aesc.AwesomeUpdater.MessageProvider;
using File = System.IO.File;


namespace Aesc.AwesomeUpdater
{
    public class AescAwesomeUpdater
    {
        public List<UpdateConfig> updateConfigs;
        public AescAwesomeUpdater(UpdateConfig updateConfig)
        {
            updateConfigs = new List<UpdateConfig>() {updateConfig };
        }
        public AescAwesomeUpdater(List<UpdateConfig> updateConfigs)
        {
            this.updateConfigs = updateConfigs;
        }
        /// <summary>
        /// 获取当前应用程序的版本。
        /// </summary>
        /// <returns>当前应用程序的版本</returns>
        public static Version GetCurrentVersion()
            => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// 检查更新，如果有则自动下载并安装，没有则忽略。<br/><br/>
        /// Steps: <see cref="UpdateLaunchConfig.updateConfigs"/> ->
        /// <see cref="UpdateConfig.GetUpdateMessage"/> -><br/>
        /// <see cref="UpdateMessage.DownloadPackage"/> ->
        /// <see cref="UpdatePackage.InstallPackage"/>
        /// </summary>
        public void QuicklyUpdate()
        {
            foreach (var updateConfig in updateConfigs)
            {
                updateConfig.QuicklyUpdate();
            }
        }
    }
    public interface IUpdateMessageProvider
    {
        string Name { get; }
        /// <summary>
        /// 获取更新信息
        /// </summary>
        /// <param name="updateConfig">更新配置信息</param>
        /// <returns>更新信息</returns>
        UpdateMessage GetUpdateMessage(UpdateConfig updateConfig);

        UpdateLaunchConfig GetUpdateLaunchConfig(string data);
    }

    public struct UpdatePackage
    {
        public Version Version => updateMessage.Version;
        public string zipFilePath;
        public string extractFilePath;

        /// <summary>
        /// 关于更新包的更新信息
        /// </summary>
        public UpdateMessage updateMessage;

        /// <summary>
        /// 关于更新包的更新配置信息
        /// </summary>
        public UpdateConfig updateConfig => updateMessage.updateConfig;

        /// <summary>
        /// 安装指定的应用程序包。
        /// </summary>
        /// <param name="extractPath"></param>
        /// <param name="targetPath"></param>
        public void InstallPackage()
        {
            DirectoryInfo directory = new DirectoryInfo(extractFilePath);
            var updateScript = directory.GetFiles("update.bat");
            if (updateScript.Length == 0)
            {
                var process = new Process();
                process.StartInfo.FileName = updateScript[0].FullName;
                process.StartInfo.CreateNoWindow = true;
                process.WaitForExit(30000);
                File.Delete(updateScript[0].FullName);
            }
            foreach (var item in directory.GetFiles())
            {
                item.MoveTo($"{updateConfig.programInstallPath}/{item.Name}", true);
            }
        }
    }

    /// <summary>
    /// 获取到的更新信息
    /// </summary>
    public struct UpdateMessage
    {
        public string packageName;
        public string updatePackageUrl;
        public UpdateConfig updateConfig;
        public string versionCode;
        public Version Version {
            get => new Version(versionCode);
        }

        /// <summary>
        /// 下载更新包并解压。
        /// </summary>
        /// <returns>更新包</returns>
        public UpdatePackage DownloadPackage()
            => DownloadPackage(
                Path.Combine(updateConfig.programInstallPath, $"UpdatedFile_{Version}.zip"),
                Path.Combine(updateConfig.programInstallPath, $"UpdatedFile_{Version}"));

        /// <summary>
        /// 下载更新包到指定位置，并解压到指定位置。
        /// </summary>
        /// <param name="filePath">更新包下载位置</param>
        /// <param name="extractPath">解压位置</param>
        /// <returns>更新包</returns>
        public UpdatePackage DownloadPackage(string filePath, string extractPath)
        {
            WebRequest.CreateHttp(updatePackageUrl).SendGet().WriteToFile(filePath);
            DirectoryInfo directory = new DirectoryInfo(extractPath);
            if (directory.Exists)
            {
                directory.Delete(true);
            }
            ZipFile.ExtractToDirectory(filePath, extractPath, true);
            return new UpdatePackage()
            {
                updateMessage = this,
                extractFilePath = extractPath,
                zipFilePath = filePath
            };
        }
    }

    /// <summary>
    /// 关于更新包的配置信息
    /// </summary>
    public struct UpdateConfig
    {
        public string programInstallPath;
        public string programExe;
        public string programName;
        public int nowVersion;
        public string messageProvider;
        public string messageData;
        public bool isInstalled;
        /// <summary>
        /// 检查当前是否需要更新。
        /// </summary>
        /// <returns>当前是否需要更新</returns>
        public bool IsNeedToUpdated()
            => AescAwesomeUpdater.GetCurrentVersion() < GetUpdateMessage().Version;

        /// <summary>
        /// 获取更新信息的提供服务。
        /// </summary>
        /// <returns>更新信息的提供服务</returns>
        public IUpdateMessageProvider GetMessageProvider()
        {
            return (messageProvider.ToLower()) switch
            {
                "bilibiliprovider" => new BilibiliUpdateMessageProvider(),
                _ => null,
            };
        }

        /// <summary>
        /// 获取当前最新的更新消息。
        /// </summary>
        /// <returns>当前最新的更新消息</returns>
        public UpdateMessage GetUpdateMessage() =>
            GetMessageProvider().GetUpdateMessage(this);

        /// <summary>
        /// 下载最新的应用程序包并解压在指定位置。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="extractPath"></param>
        public void DownloadPackage(string filePath, string extractPath) =>
            GetUpdateMessage().DownloadPackage(filePath, extractPath);

        /// <summary>
        /// 检查更新，如果有则自动下载并安装，没有则忽略。<br/><br/>
        /// Steps: <see cref="GetUpdateMessage"/> ->
        /// <see cref="UpdateMessage.DownloadPackage"/><br/> ->
        /// <see cref="UpdatePackage.InstallPackage"/>
        /// </summary>
        public void QuicklyUpdate()
        {
            if (IsNeedToUpdated()) 
                GetUpdateMessage().DownloadPackage().InstallPackage();
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
        public UpdaterArgs updaterArgs;
        public AescUpdaterProgram(string[] args)
        {
            updaterArgs = AescArgsParser.Parse<UpdaterArgs>(args);
        }
        public static void Main(string[] args)
        {
            new AescUpdaterProgram(args);
        }
        public void QuicklyUpdate()
        {
            var updateConfig = GetUpdateLaunchConfigOnline(1);
            new AescAwesomeUpdater(updateConfig.updateConfigs).QuicklyUpdate();
        }

        /// <summary>
        /// 默认使用<see cref="BiliCommitMsgPvder"/>和<see cref="BiliReply"/>获取更新信息，此方法可以在子类重写。<br/><br/>
        /// 关于<see cref="BiliReply"/>的格式规定：<br/>
        /// packageName|messageData
        /// <br/><br/>其余<see cref="UpdateConfig"/>的值会自动填充。
        /// </summary>
        /// <param name="informationId"></param>
        /// <returns>启动更新程序的更新配置信息</returns>
        public virtual UpdateLaunchConfig GetUpdateLaunchConfigOnline(int informationId)
        {
            BiliCommitMsgPvder commit = new BiliCommitMsgPvder(informationId);
            var replies = commit.biliReplies;
            var launchConfigMaster = new UpdateLaunchConfig();
            var launchConfig = launchConfigMaster.updateConfigs;
            string packageName;
            string messageData;
            string[] textSplitResult;
            string awesomeCorePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            foreach (var reply in replies)
            {
                textSplitResult = reply.text.Split("|");
                packageName = textSplitResult[0];
                messageData = textSplitResult[1];
                launchConfig.Add(new UpdateConfig()
                {
                    programExe = $"{packageName}.exe",
                    programName = packageName,
                    programInstallPath = $"{awesomeCorePath}\\{packageName}",
                    messageProvider = "BilibiliProvider",
                    isInstalled = File.Exists($"{awesomeCorePath}\\{packageName}\\{packageName}.exe"),
                    messageData = messageData,
                    nowVersion = 0
                });
            }
            return launchConfigMaster;
        }
    }
}
