using Aquc.Stackbricks.MsgPvder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public interface IStackbricksMsgPvder
{
    public string MsgPvderId { get; }
    public Task<StackbricksUpdateMessage> GetUpdateMessage(string data);
}
public class StackbricksUpdateMessage
{
    public Version version;
    public string PkgPvderId;
    public string PkgPvderArgs;
    public StackbricksUpdateMessage(Version version, string pkgPvderId, string pkgPvderArgs)
    {
        this.version = version;
        PkgPvderId = pkgPvderId;
        PkgPvderArgs = pkgPvderArgs;
    }
}
public class StackbricksMsgPvderManager
{
    static Dictionary<string, IStackbricksMsgPvder> matchDict = new Dictionary<string, IStackbricksMsgPvder>
    {
        {"stbks.msgpvder.bilicmts",new BiliCommitMsgPvder() }
    };
    public static IStackbricksMsgPvder ParseMsgPvder(string msgPvderId)
    {
        // ncpe
        return matchDict[msgPvderId];
    }
}
