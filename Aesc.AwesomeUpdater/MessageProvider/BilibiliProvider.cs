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
            string packageName = commitContent[0];
            string netdiskProvider = commitContent[1];
            string netdiskUrl = commitContent[2];
            string[] netdiskData = commit.biliReplies[0].text.Split(":");
            return new UpdateMessage()
            {
                packageName = packageName,
                VersionCode = netdiskData[0],
                UpdatePackageUrl = new Huang111Netdisk().ParseUrl(netdiskData[1])
            };
        }
    }
}
