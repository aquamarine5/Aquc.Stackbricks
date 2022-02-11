using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Aquc.AquaKits.Net;
using Aquc.AquaKits.Net.WebStorage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aquc.AquaUpdater.MessageProvider
{
    public struct LuoguUpdateContent
    {
        public string version;
        public List<LuoguUpdateConfigContent> configContents;
    }
    public struct LuoguUpdateConfigContent
    {
        public string messageData;
        public string messageProvider;

    }
    public class LuoguUpdateMsgPvder : IUpdateMessageProvider
    {
        public string Name => nameof(LuoguUpdateMsgPvder);

        public UpdateMessage GetUpdateMessage(UpdateConfig body)
        {
            var pasteContent = JObject.Parse(LuoguMsgPvder.GetMessage(body.messageData));
            return new UpdateMessage()
            {
                updateConfig = body,
                updatePackageUrl = new Huang1111Netdisk().ParseUrl(pasteContent["key"].ToString()),
                packageName = pasteContent["name"].ToString(),
                versionCode = pasteContent["version"].ToString()
            };
        }

        public UpdateLaunchConfig GetUpdateLaunchConfig(string data,LaunchConfig config)
        {
            var pasteContent = JsonConvert.DeserializeObject<LuoguUpdateContent>(LuoguMsgPvder.GetMessage(data));
            var programInstallRootPath = config.programInstallRootPath;
            return new UpdateLaunchConfig();
        }
    }
}
