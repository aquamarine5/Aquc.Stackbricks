using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public class StackbricksService
{
    public StackbricksConfig stackbricksConfig;
    public StackbricksService(StackbricksConfig stackbricksConfig)
    {
        this.stackbricksConfig= stackbricksConfig;
    }
    public IStackbricksMsgPvder GetMsgPvder()
    {
        //return stackbricksConfig.ProgramManifest.
    }
}
