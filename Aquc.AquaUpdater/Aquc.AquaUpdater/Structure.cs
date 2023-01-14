﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aquc.AquaUpdater;

public struct UpdateMessage
{
    public UpdateSubscription updateSubscription;
    public string fileArgs;
    public IUpdateFilesProvider filesProvider;
    public Version packageVersion;
    public bool NeedUpdate(Version version) =>
        packageVersion > version;
    public bool NeedUpdate() =>
        packageVersion > System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    public UpdatePackage GetUpdatePackage()
    {
        var package= filesProvider.DownloadPackage(this);
        Logging.UpdateMessageLogger.LogInformation("Download update package successfully.");
        return package;
    }
    public UpdatePackage? GetUpdatePackageWhenAvailable()
    {
        return NeedUpdate() ? GetUpdatePackage() : null;
    }
}
public struct UpdatePackage
{
    public UpdateMessage updateMessage;
    public UpdateSubscription updateSubscription;
    public FileInfo zipPath;
    public DirectoryInfo extraceZipDirectory;
    public void InstallPackage()
    {
        var updateScript = extraceZipDirectory.GetFiles("do.");
        if (updateScript.Length != 0)
        {
            foreach (FileInfo f in updateScript)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = f.FullName,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8
                    },
                    EnableRaisingEvents = true
                };
                process.Start();
                process.BeginOutputReadLine();
                if (!process.WaitForExit(30000))
                    Logging.UpdatePackageLogger.LogWarning("execute update script: {filename} failed within 30000ms", f.Name);
                else
                    Logging.UpdatePackageLogger.LogInformation("execute update script: {filename} successfully", f.Name);
            }
        }
        CopyDirectory(extraceZipDirectory, updateSubscription.programDirectory);
        Logging.UpdatePackageLogger.LogInformation("install package {programname}:{programversion} successfully",
            updateSubscription.programKey, updateMessage.packageVersion.ToString());
        zipPath.Delete();
        extraceZipDirectory.Delete(true);
    }
    private void CopyDirectory(DirectoryInfo directory, DirectoryInfo dest)
    {
        if (!dest.Exists) dest.Create();
        foreach (FileInfo f in directory.GetFiles())
        {
            f.CopyTo(Path.Combine(dest.FullName, f.Name), true);
        }
        foreach (DirectoryInfo d in directory.GetDirectories())
        {
            CopyDirectory(d, new DirectoryInfo(Path.Combine(dest.FullName, d.Name)));
        }
    }
}
public struct UpdateSubscription
{
    public int SubscriptionVersion { get; init; }
    public DateTime lastCheckUpdateTime;
    public string args;
    public DirectoryInfo programDirectory;
    public FileInfo programExtrancePath;
    public string programKey;
    public Version currentlyVersion;
    public IUpdateMessageProvider updateMessageProvider;
    public IUpdateMessageProvider secondUpdateMessageProvider;
    public bool NeedUpdate() => GetUpdateMessage().NeedUpdate();
    public UpdateMessage GetUpdateMessage()
    {
        var message=updateMessageProvider.GetUpdateMessage(this);
        Logging.UpdateSubscriptionLogger.LogInformation("Get {key}, ver={ver} update message successfully.",
            message.updateSubscription.programKey, message.packageVersion);
        return message;
    }
}
public class UpdateSubscriptionConverter : JsonConverter<UpdateSubscription>
{
    public override UpdateSubscription ReadJson(JsonReader reader, Type objectType, [AllowNull] UpdateSubscription existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jo = serializer.Deserialize(reader) as JObject;
        return new UpdateSubscription()
        {
            SubscriptionVersion = jo["subscriptionVersion"].ToObject<int>(),
            args = jo["args"].ToString(),
            updateMessageProvider = Provider.GetMessageProvider(jo["updateMessageProvider"]["Identity"].ToString()),
            secondUpdateMessageProvider = Provider.GetMessageProvider(jo["secondUpdateMessageProvider"]["Identity"].ToString()),
            currentlyVersion = new Version(jo["version"].ToString()),
            lastCheckUpdateTime = new DateTime(long.Parse(jo["lastCheckUpdateTime"].ToString())),
            programKey = jo["programKey"].ToString(),
            programDirectory = new DirectoryInfo(jo["programDirectory"].ToString()),
            programExtrancePath = new FileInfo(jo["programExtrancePath"].ToString())
        };
    }

    public override void WriteJson(JsonWriter writer, [AllowNull] UpdateSubscription value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("args");
        writer.WriteValue(value.args);
        writer.WritePropertyName("subscriptionVersion");
        writer.WriteValue(SubscriptionController.NEWEST_SUBSCRIPTION_VERSION);
        writer.WritePropertyName("lastCheckUpdateTime");
        writer.WriteValue(value.lastCheckUpdateTime.Ticks.ToString());
        writer.WritePropertyName("version");
        writer.WriteValue(value.currentlyVersion.ToString());
        writer.WritePropertyName("programDirectory");
        writer.WriteValue(value.programDirectory.FullName);
        writer.WritePropertyName("programExtrancePath");
        writer.WriteValue(value.programExtrancePath.FullName);
        writer.WritePropertyName("programKey");
        writer.WriteValue(value.programKey);
        writer.WritePropertyName("updateMessageProvider");
        writer.WriteStartObject();
        writer.WritePropertyName("Identity");
        writer.WriteValue(value.updateMessageProvider.Identity);
        writer.WriteEndObject();
        writer.WritePropertyName("secondUpdateMessageProvider");
        writer.WriteStartObject();
        writer.WritePropertyName("Identity");
        writer.WriteValue(value.secondUpdateMessageProvider.Identity);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
