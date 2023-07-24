using Aquc.Stackbricks.Actions;
using Aquc.Stackbricks.MsgPvder;

namespace Aquc.Stackbricks;

public class StackbricksConfig
{
    public int ConfigVersion = 1;
    public StackbricksManifest StackbricksManifest;
    public StackbricksManifest ProgramManifest;
    public StackbricksConfig(StackbricksManifest ProgramManifest)
    {
        this.ProgramManifest = ProgramManifest;
        StackbricksManifest = StackbricksManifest.CreateStackbricksManifest();
    }
}
public class StackbricksManifest
{
    public Version Version;
    public DirectoryInfo ProgramDir;
    public string Id;
    public DateTime? LastCheckTime;
    public DateTime? LastUpdateTime;
    public List<IStackbricksAction> UpdateActions;
    public string MsgPvderId;
    public string MsgPvderData;
    public StackbricksManifest(Version version,string id, DateTime? lastCheckTime, DateTime? lastUpdateTime, List<IStackbricksAction> updateActions, string msgPvderId,string msgPvderData, DirectoryInfo programDir)
    {
        Version = version;
        ProgramDir = programDir;
        MsgPvderId = msgPvderId;
        MsgPvderData = msgPvderData;
        Id = id;
        LastCheckTime = lastCheckTime;
        LastUpdateTime = lastUpdateTime;
        UpdateActions = updateActions;
    }
    public IStackbricksMsgPvder GetMsgPvder()
    {
        return StackbricksMsgPvderManager.ParseMsgPvder(MsgPvderId);
    }
    public static StackbricksManifest CreateStackbricksManifest()
    {
        return new StackbricksManifest(
            new Version(1,1),
            "Stackbricks", null, null,
            new List<IStackbricksAction>
                {
                    new ActionReplaceAll(),
                    new ActionRunUpdatePackageActions()
                }, BiliCommitMsgPvder._MsgPvderId,"", new DirectoryInfo("."));
    }
}
