using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aquc.AquaUpdater.Pvder;

namespace Aquc.AquaUpdater
{
    public struct UpdateMessage
    {
        public string fileArgs;
        public IUpdateFilesProvider filesProvider;
        public Version packageVersion;
        public bool NeedUpdate(Version version) => 
            packageVersion > version;
        public bool NeedUpdate() => 
            packageVersion > System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        public UpdatePackage GetUpdatePackage()
        {
            return filesProvider.DownloadPackage(this);
        }
    }
    public struct UpdatePackage
    {

    }
    public struct UpdateSubscription
    {
        public string args;
        
        public IUpdateMessageProvider updateMessageProvider;
        public UpdateMessage GetUpdateMessage()
        {
            return updateMessageProvider.GetUpdateMessage(this);
        }
    }
    public class UpdateSubscriptionConverter : JsonConverter<UpdateSubscription>
    {
        public override UpdateSubscription ReadJson(JsonReader reader, Type objectType, [AllowNull] UpdateSubscription existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = serializer.Deserialize(reader) as JObject;
            return new UpdateSubscription()
            {
                args = jo["args"].ToString(),
                updateMessageProvider = Provider.GetMessageProvider(jo["updateMessageProvider"]["Identity"].ToString())
            };
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] UpdateSubscription value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("args");
            writer.WriteValue(value.args);
            writer.WritePropertyName("updateMessageProvider");
            writer.WriteStartObject();
            writer.WritePropertyName("Identity");
            writer.WriteValue(value.updateMessageProvider.Identity);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
