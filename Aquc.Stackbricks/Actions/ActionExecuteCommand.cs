using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.Actions
{
    public class ActionExecuteCommand : IUpdateAction
    {
        public const string ID = "stbks.action.execute";

        public const string FLAG_WAITFOREXIT = "stbks.action.execute.waitforexit";

        public string ActionId => ID;

        public void Execute(UpdateActionData stackbricksAction, UpdatePackage updatePackage)
        {
            StackbricksProgram.logger.Debug($"Apply command line: cmd {stackbricksAction.Args[0]}");
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/C " + stackbricksAction.Args[0],
                    CreateNoWindow = true
                }
            };
            if (stackbricksAction.ContainFlag(FLAG_WAITFOREXIT))
                process.WaitForExit(300000);
            else
                process.Start();
        }
    }
}
