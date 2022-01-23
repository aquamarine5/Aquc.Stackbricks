using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Aesc.AwesomeKits.Net;
using Aesc.AwesomeKits.Net.WebStorage;
using Newtonsoft.Json.Linq;

namespace Aesc.AwesomeUpdater.MessageProvider
{
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

        public UpdateLaunchConfig GetUpdateLaunchConfig(string data)
        {
            var pasteContent = JObject.Parse(LuoguMsgPvder.GetMessage(data));
            var jsondatas = JArray.Parse(pasteContent["lists"].ToString());
            return new UpdateLaunchConfig();
        }
    }
}
