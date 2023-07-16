using Aquc.BiliCommits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.MsgPvder;

public class BiliCommitMsgPvder : IStackbricksMsgPvder
{
    public string MsgPvderId => "stbks.msgpvder.bilicmts";

    public async Task<StackbricksUpdateMessage> GetUpdateMessage(string data)
    {

        //stbks.msgpvder.bilicmts@1;;0.2.0;;stbks.pkgpvder.ghproxy;;
        var message=await BiliCommitsClass.GetReply(data);
        var msgData = message.Split(";;");
        if (msgData.Length > 0) {
            if (msgData[0]==MsgPvderId+"@1")
            {
                return ParseToUpdateMessageV1(msgData);
            }
            // ncpe
        }
        throw new NotImplementedException();
    }

    static StackbricksUpdateMessage ParseToUpdateMessageV1(string[] message)
    {
        // ncpe
        return new StackbricksUpdateMessage(
            new Version(message[1]),
            message[2],
            message[3]
            );
    }
}
