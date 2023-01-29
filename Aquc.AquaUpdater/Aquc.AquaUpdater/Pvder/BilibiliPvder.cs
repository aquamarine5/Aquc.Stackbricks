using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Aquc.AquaUpdater.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Aquc.Netdisk.Bilibili;

namespace Aquc.AquaUpdater.Pvder
{
    public class BiliCommitMsgPvder : IUpdateMessageProvider
    {
        public BiliCommitMsgPvder() { }
        public string Identity => "bilibilimsgpvder";
        [Obsolete("Use Aquc.Netdisk.Bilibili")]
        public List<BiliReply> GetReplies(string id)
        {
            var commitTextJson = WebRequest.CreateHttp("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id=" + id)
                .SendGet().ReadJsonObject()["data"]["card"]["card"].ToString();
            var commitText = JObject.Parse(commitTextJson)["item"]["content"].ToString();
            var content = WebRequest.CreateHttp($"https://api.bilibili.com/x/v2/reply/main?jsonp=jsonp&next=0&type=11&oid={id}&mode=3&plat=1")
                .SendGet().ReadJsonObject()["data"]["replies"].ToString();
            var replyJsonArray = JArray.Parse(content);
            var biliReplies = new List<BiliReply>();
            foreach (var replyJson in replyJsonArray)
            {
                biliReplies.Add(new BiliReply()
                {
                    text = replyJson["content"]["message"].ToString()
                });
            }
            return biliReplies;
        }
        [Obsolete("Use Aquc.Netdisk.Bilibili")]
        public List<BiliReply> GetReplies(long id) => GetReplies(id.ToString());

        public UpdateMessage GetUpdateMessage(UpdateSubscription updateSubscription)
        {
            var t = BilibiliMsgPvder.Get(updateSubscription.args);
            t.Wait();
            var data = t.Result.Split(";;");
            return new UpdateMessage()
            {
                fileArgs = data[2],
                filesProvider = Provider.GetFilesProvider(data[1]),
                packageVersion = new Version(data[0]),
                updateSubscription=updateSubscription
            };
        }
    }
    public struct BiliReply
    {
        public string text;
    }
}
