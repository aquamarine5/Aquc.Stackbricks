using Aquc.BiliCommits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.MsgPvder;

public class BiliCommitMsgPvder : IStackbricksMsgPvder
{
    public string Id => "stbks.msgpvder.bilicmts";

    public StackbricksUpdateMessage GetUpdateMessage(string data)
    {

        //stbks.msgpvder.bilicmts@1;;
        var message=BiliCommits
        var data = message.Split(";;");
        throw new NotImplementedException();
    }

    StackbricksUpdateMessage ParseToUpdateMessage(string[] message)
    {
        return new StackbricksUpdateMessage()
    }
}
