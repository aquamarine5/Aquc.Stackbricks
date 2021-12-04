using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using Aesc.AwesomeKits;

namespace Aesc.AwesomeUpdater.MessageProvider
{
    public class BilibiliUpdateMessageProvider : IUpdateMessageProvider
    {
        public string Name => "BilibiliProvider";
        public readonly string commentsReplyUrl = "https://api.bilibili.com/x/v2/reply/main?jsonp=jsonp&next=0&type=17&mode=2&plat=1&oid";
        public readonly string commentsMainUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id";
        public UpdateMessage GetUpdateMessage(string value)
        {
            BiliCommit commit = new BiliCommit(int.Parse(value));
            var commitContent = commit.commitText.Split("|");
            string netdiskProvider = commit.commitText;
            string resultMain = Bili;//AescWebRequest.WebRequestGet($"{commentsMainUrl}={value}");
            string result = AescWebRequest.WebRequestGet($"{commentsReplyUrl}={value}");  //oid=592534916524618496
            JObject jsonObject = JObject.Parse(result);
            JObject jsonObjectMain = JObject.Parse(resultMain);
            Console.WriteLine(jsonObjectMain);
            JObject jsonObjectMainCard = JObject.Parse(jsonObjectMain["data"]["card"]["card"].ToString());
            string baseDownloadUrl = jsonObjectMainCard["item"]["content"].ToString();
            string realMessage = jsonObject["data"]["replies"][0]["content"]["message"].ToString();
            var list = realMessage.Split(':');
            string resultNetdisk = AescWebRequest.WebRequestPut(baseDownloadUrl.Replace("&downloadCode", list[1]));
            Console.WriteLine(resultNetdisk);
            JObject jsonObjectNetdisk = JObject.Parse(resultNetdisk);
            return new UpdateMessage()
            {
                VersionCode = list[0],
                UpdatePackageUrl = jsonObjectNetdisk["data"].ToString()
            };
        }
    }
