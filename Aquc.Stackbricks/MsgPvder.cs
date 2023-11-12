using Aquc.Stackbricks.MsgPvder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public interface IMessagePvder
{
    public string MsgPvderId { get; }
    public Task<UpdateMessage> GetUpdateMessageAsync(StackbricksManifest stackbricksManifest);
}
public class UpdateMessage
{
    public StackbricksManifest stackbricksManifest;
    public Version version;
    public string PkgPvderId;
    public string PkgPvderArgs;
    public UpdateMessage(StackbricksManifest stackbricksManifest, Version version, string pkgPvderId, string pkgPvderArgs)
    {
        this.stackbricksManifest = stackbricksManifest;
        this.version = version;
        PkgPvderId = pkgPvderId;
        PkgPvderArgs = pkgPvderArgs;
    }
    public IUpdatePkgPvder GetPkgPvder() => PackagePvderManager.ParsePkgPvder(PkgPvderId);
    public bool NeedUpdate() => version > stackbricksManifest.Version;
}
public class MessagePvderManager
{
    static readonly Dictionary<string, IMessagePvder> matchDict = new()
    {
        {BiliCommitMsgPvder.ID,new BiliCommitMsgPvder() },
        {WeiboCommitMsgPvder.ID, new WeiboCommitMsgPvder() }
    };
    public static IMessagePvder ParseMsgPvder(string msgPvderId)
    {
        // ncpe
        return matchDict[msgPvderId];
    }
}
