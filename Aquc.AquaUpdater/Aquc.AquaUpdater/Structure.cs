using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace Aquc.AquaUpdater;

public class UpdateMessage
{
    public UpdateSubscription updateSubscription;
    public string fileArgs;
    public IUpdateFilesProvider filesProvider;
    public Version packageVersion;
    public bool NeedUpdate(Version version) =>
        packageVersion > version;
    public bool NeedUpdate() =>
        packageVersion > updateSubscription.currentlyVersion;
    public UpdatePackage GetUpdatePackage()
    {
        var package= filesProvider.DownloadPackage(this);
        Logging.UpdateMessageLogger.LogInformation("Download update package successfully.");
        return package;
    }
    public UpdatePackage GetUpdatePackageWhenAvailable()
    {
        return NeedUpdate() ? GetUpdatePackage() : null;
    }
}
public class UpdatePackage
{
    public UpdateMessage updateMessage;
    public UpdateSubscription updateSubscription;
    public FileInfo zipPath;
    public DirectoryInfo extraceZipDirectory;
    public void InstallPackage()
    {
        string Parse(string i)
        {
            if (i.EndsWith("\\"))
                return i[..^1];
            else return i;
        }
        if (extraceZipDirectory.GetFiles(".usebackground").Length == 1)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppContext.BaseDirectory, "Aquc.AquaUpdater.Background", "Aquc.AquaUpdater.Background.exe"),
                    Arguments=$"\"{Parse(extraceZipDirectory.FullName)}\" \"{Parse(updateSubscription.programDirectory.FullName)}\" \"{zipPath.FullName}\"",
                    CreateNoWindow = true,

                }
            };
            Logging.UpdatePackageLogger.LogInformation("run background task: {a}",
                $"\"{Parse(extraceZipDirectory.FullName)}\" \"{Parse(updateSubscription.programDirectory.FullName)}\" \"{zipPath.FullName}\"");
            process.Start();
            
            return;
        }
        var updateScript = extraceZipDirectory.GetFiles("do.*");
        if (updateScript.Length != 0)
        {
            foreach (FileInfo f in updateScript)
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = f.FullName,
                            CreateNoWindow = true,
                            UseShellExecute = false
                        },
                        EnableRaisingEvents = true
                    };
                    process.Start();
                    //process.BeginOutputReadLine();
                    if (!process.WaitForExit(30000))
                        Logging.UpdatePackageLogger.LogWarning("execute update script: {filename} failed within 30000ms", f.Name);
                    else
                        Logging.UpdatePackageLogger.LogInformation("execute update script: {filename} successfully", f.Name);
                }
                catch (Exception ex)
                {
                    Logging.UpdatePackageLogger.LogError("execute update script: {filename} raised a exception: {exception} {track}", f.Name, ex.Message, ex.StackTrace);
                }
            }
        }
        CopyDirectory(extraceZipDirectory, updateSubscription.programDirectory);
        Logging.UpdatePackageLogger.LogInformation("install package {programname}:{programversion} successfully",
            updateSubscription.programKey, updateMessage.packageVersion.ToString());
        updateSubscription.lastCheckUpdateTime = DateTime.Now;
        updateSubscription.currentlyVersion = updateMessage.packageVersion;
        Launch.UpdateLaunchConfig();
        if (extraceZipDirectory.GetFiles(".donotremovezip").Length==0)
            zipPath.Delete();
        if (extraceZipDirectory.GetFiles(".donotremoveextracezip").Length == 0)
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
public class UpdateSubscription
{
    public int SubscriptionVersion { get; init; }
    public DateTime lastCheckUpdateTime;
    public string args;
    public DirectoryInfo programDirectory;
    public FileInfo programExtrancePath;
    public string programKey;
    public Version currentlyVersion;
    public string updateMessageProvider;
    public string secondUpdateMessageProvider;
    public bool NeedUpdate() => GetUpdateMessage().NeedUpdate();
    public UpdateMessage GetUpdateMessage()
    {
        var message=Provider.GetMessageProvider(updateMessageProvider).GetUpdateMessage(this);
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
            updateMessageProvider = jo["updateMessageProvider"]["Identity"].ToString(),
            secondUpdateMessageProvider = jo["secondUpdateMessageProvider"]["Identity"].ToString(),
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
        writer.WriteValue(value.updateMessageProvider);
        writer.WriteEndObject();
        writer.WritePropertyName("secondUpdateMessageProvider");
        writer.WriteStartObject();
        writer.WritePropertyName("Identity");
        writer.WriteValue(value.secondUpdateMessageProvider);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
