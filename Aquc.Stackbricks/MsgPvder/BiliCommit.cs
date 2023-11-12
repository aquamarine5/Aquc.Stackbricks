using Aquc.BiliCommits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.MsgPvder;

public class BiliCommitMsgPvder : IMessagePvder
{
    public string MsgPvderId => ID;
    public const string ID = "stbks.msgpvder.bilicmts";
    public async Task<UpdateMessage> GetUpdateMessageAsync(StackbricksManifest stackbricksManifest)
    {
        var message=await BiliCommitsClass.GetReply(StackbricksProgram.httpClient,stackbricksManifest.MsgPvderData);
        StackbricksProgram.logger.Information(message);
        var msgData = message.Split(";;");
        if (msgData.Length > 0) {
            if (msgData[0]==MsgPvderId+"@1")
            {
                return ParseToUpdateMessageV1(msgData,stackbricksManifest);
            }
            // ncpe
        }
        throw new NotImplementedException();
    }

    static UpdateMessage ParseToUpdateMessageV1(string[] message, StackbricksManifest manifest)
    {
        // ncpe
        return new UpdateMessage(
            manifest,
            new Version(message[1]),
            message[2],
            message[3]
            );
    }
}
