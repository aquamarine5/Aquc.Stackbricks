using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public class StackbricksConfig
{
    public int ConfigVersion = 1;
    public StackbricksManifest StackbricksManifest;
    public StackbricksManifest ProgramManifest;
    public StackbricksConfig(StackbricksManifest ProgramManifest)
    {
        this.ProgramManifest = ProgramManifest;
        StackbricksManifest=StackbricksManifest.CreateStackbricksManifest();
    }
}
public class StackbricksManifest
{
    public DirectoryInfo programDir;
    public string Id;
    public DateTime? LastCheckTime;
    public DateTime? LastUpdateTime;
    public List<IStackbricksAction> UpdateActions;
    public string MsgPvderId;
    public StackbricksManifest(string id, DateTime? lastCheckTime, DateTime? lastUpdateTime, List<IStackbricksAction> updateActions,string msgPvderId)
    {
        MsgPvderId = msgPvderId;
        Id = id;
        LastCheckTime = lastCheckTime;
        LastUpdateTime = lastUpdateTime;
        UpdateActions = updateActions;
    }
    public static StackbricksManifest CreateStackbricksManifest()
    {
        return new StackbricksManifest("Stackbricks", null, null, new List<IStackbricksAction>
        {
            new ActionReplaceAll(),
            new ActionRunUpdatePackageActions()
        },"");
    }
}
