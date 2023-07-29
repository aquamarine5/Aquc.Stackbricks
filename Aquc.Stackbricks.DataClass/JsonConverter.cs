using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.DataClass;

public class UpdateDCJsonConverter : JsonConverter<UpdateDataClass>
{
    public override UpdateDataClass? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonNode = JsonNode.Parse(ref reader)!;
        var needUpdate = (bool)jsonNode["needUpdate"]!;
        var isProgram = (bool)jsonNode["IsProgram"]!;
        if (!(bool)jsonNode["isDirectory"]!)
            return new UpdateDataClass(isProgram, needUpdate, jsonNode["version"]!.ToString(), jsonNode["filePath"]!.ToString());
        else
            return new UpdateDataClass(isProgram, needUpdate, jsonNode["version"]!.ToString(), jsonNode["filePath"]!.ToString(), jsonNode["depressedDir"]!.ToString());
    }


    public override void Write(Utf8JsonWriter writer, UpdateDataClass value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("version", value.version);
        writer.WriteString("depressDir", value.depressedDir);
        writer.WriteString("filePath", value.filePath);
        writer.WriteString("DCID", value.DCID);
        writer.WriteBoolean("isProgram", value.IsProgram);
        writer.WriteBoolean("needUpdate", value.needUpdate);
        writer.WriteBoolean("IsDirectory", value.isDirectory);
        writer.WriteEndObject();
    }
}
public class CheckDCJsonConverter : JsonConverter<CheckDataClass>
{
    public override CheckDataClass? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonNode = JsonNode.Parse(ref reader)!;
        return new CheckDataClass((bool)jsonNode["isProgram"]!, (bool)jsonNode["needUpdate"]!, jsonNode["version"]!.ToString());
    }

    public override void Write(Utf8JsonWriter writer, CheckDataClass value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("version", value.version);
        writer.WriteString("DCID", value.DCID);
        writer.WriteBoolean("isProgram", value.IsProgram);
        writer.WriteBoolean("needUpdate", value.needUpdate);
        writer.WriteEndObject();
    }
}

public class ExceptionDCJsonConverter : JsonConverter<ExceptionDataClass>
{
    public override ExceptionDataClass? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {

        var jsonNode = JsonNode.Parse(ref reader)!;
        return new ExceptionDataClass(jsonNode["type"]!.ToString(), jsonNode["message"]!.ToString());
    }

    public override void Write(Utf8JsonWriter writer, ExceptionDataClass value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.type);
        writer.WriteString("message", value.message);
        writer.WriteString("DCID", value.DCID);
        writer.WriteEndObject();
    }
}