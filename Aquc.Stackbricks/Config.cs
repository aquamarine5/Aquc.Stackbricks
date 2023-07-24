using Aquc.Stackbricks.Actions;
using Aquc.Stackbricks.MsgPvder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Aquc.Stackbricks;

public class StackbricksConfig
{
    public int ConfigVersion = 1;
    public StackbricksManifest StackbricksManifest;
    public StackbricksManifest ProgramManifest;
    public StackbricksConfig(StackbricksManifest ProgramManifest)
    {
        this.ProgramManifest = ProgramManifest;
        StackbricksManifest = StackbricksManifest.CreateStackbricksManifest();
    }
}
public class StackbricksManifest
{
    public Version Version;
    public DirectoryInfo ProgramDir;
    public string Id;
    public string MsgPvderId;
    public string MsgPvderData;
    public DateTime? LastCheckTime;
    public DateTime? LastUpdateTime;
    public List<IStackbricksAction> UpdateActions;
    public StackbricksManifest(Version version,string id, DateTime? lastCheckTime, DateTime? lastUpdateTime, List<IStackbricksAction> updateActions, string msgPvderId,string msgPvderData, DirectoryInfo programDir)
    {
        Version = version;
        ProgramDir = programDir;
        MsgPvderId = msgPvderId;
        MsgPvderData = msgPvderData;
        Id = id;
        LastCheckTime = lastCheckTime;
        LastUpdateTime = lastUpdateTime;
        UpdateActions = updateActions;
    }
    public IStackbricksMsgPvder GetMsgPvder()
    {
        return StackbricksMsgPvderManager.ParseMsgPvder(MsgPvderId);
    }
    public static StackbricksManifest CreateStackbricksManifest()
    {
        return new StackbricksManifest(
            Assembly.GetExecutingAssembly().GetName().Version!,
            "Stackbricks", null, null,
            new List<IStackbricksAction>
                {
                    new ActionReplaceAll(),
                    new ActionRunUpdatePackageActions()
                }, BiliCommitMsgPvder._MsgPvderId,"", new DirectoryInfo(Directory.GetCurrentDirectory()));
    }
}
public class VersionJsonConverter : JsonConverter<Version>
{
    public override Version? ReadJson(JsonReader reader, Type objectType, Version? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new Version((serializer.Deserialize(reader) as JObject)!["version"]!.ToString());
    }

    public override void WriteJson(JsonWriter writer, Version? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("version");
        writer.WriteValue(value!.ToString());
        writer.WriteEndObject();
    }
}
public class DirectoryInfoJsonConverter : JsonConverter<DirectoryInfo>
{
    public override DirectoryInfo ReadJson(JsonReader reader, Type objectType, DirectoryInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new DirectoryInfo((serializer.Deserialize(reader) as JObject)!["directory_info"]!.ToString());
    }

    public override void WriteJson(JsonWriter writer, DirectoryInfo? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("directory_info");
        writer.WriteValue(value!.FullName);
        writer.WriteEndObject();
    }
}