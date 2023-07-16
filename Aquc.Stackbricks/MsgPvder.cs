using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public interface IStackbricksMsgPvder
{
    public string Id { get; }
    public StackbricksUpdateMessage GetUpdateMessage();
}
public class StackbricksUpdateMessage
{

}
public class StackbricksMsgPvderManager
{

}
