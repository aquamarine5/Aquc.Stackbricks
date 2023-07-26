using Aquc.Stackbricks.Actions;
using Aquc.Stackbricks.MsgPvder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Aquc.Stackbricks;

public class StackbricksConfig
{
    public const string CONFIG_FILENAME = "Aquc.Stackbricks.config.json";
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
    public List<StackbricksActionData> UpdateActions;
    public StackbricksManifest(Version version,string id, DateTime? lastCheckTime, DateTime? lastUpdateTime, List<StackbricksActionData> updateActions, string msgPvderId,string msgPvderData, DirectoryInfo programDir)
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
            Assembly.GetExecutingAssembly().GetName().Version??new Version(0,0,0,1),
            "Aquc.Stackbricks", null, null,
            new List<StackbricksActionData>
                {
                    new StackbricksActionData(new ActionApplySelfUpdate()),
                }, WeiboCommitMsgPvder.ID, "4927489886915247", new DirectoryInfo(Directory.GetCurrentDirectory()));
    }
    public static StackbricksManifest CreateBlankManifest()
    {
        return new StackbricksManifest(
            new Version(0, 0, 0, 1), "", null, null, new List<StackbricksActionData>(), "", "", new DirectoryInfo(Directory.GetCurrentDirectory()));
    }
}

#region CustomJsonConverter
public class VersionJsonConverter : JsonConverter<Version>
{
    public override Version? ReadJson(JsonReader reader, Type objectType, Version? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new Version(serializer.Deserialize(reader)!.ToString()!);
    }

    public override void WriteJson(JsonWriter writer, Version? value, JsonSerializer serializer)
    {
        writer.WriteValue(value!.ToString());
    }
}
public class DirectoryInfoJsonConverter : JsonConverter<DirectoryInfo>
{
    public override DirectoryInfo ReadJson(JsonReader reader, Type objectType, DirectoryInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new DirectoryInfo(serializer.Deserialize(reader)!.ToString()!);
    }

    public override void WriteJson(JsonWriter writer, DirectoryInfo? value, JsonSerializer serializer)
    {
        writer.WriteValue(value!.FullName);
    }
}
public class StackbricksActionDataJsonConverter : JsonConverter<StackbricksActionData>
{
    public override StackbricksActionData? ReadJson(JsonReader reader, Type objectType, StackbricksActionData? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var obj=(serializer.Deserialize(reader) as JObject)!;
        return new StackbricksActionData(obj["Id"]!.ToString(), obj["Args"]!.ToObject<List<string>>()!, obj["Flags"]!.ToObject<List<string>>()!);
    }

    public override void WriteJson(JsonWriter writer, StackbricksActionData? value, JsonSerializer serializer)
    {
        new JsonSerializer().Serialize(writer, value);
        /*
        writer.WriteStartObject();
        writer.WriteStartObject();
        writer.WritePropertyName("Id");
        writer.WriteValue(value!.Id);
        writer.WriteEndObject();
        writer.WriteStartObject();
        writer.WritePropertyName("Args");
        writer.WriteStartArray();
        foreach (var arg in value.Args)
        {
            writer.WriteValue(arg);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();

        writer.WriteStartObject();
        writer.WritePropertyName("Flags");
        writer.WriteStartArray();
        foreach (var flag in value.Flags)
        {
            writer.WriteValue(flag);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();*/
    }
}
#endregion