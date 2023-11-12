using Sentry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.Actions;

public class ActionApplySelfUpdate : IUpdateAction
{
    public string ActionId => ID;

    public const string ID = "stbks.action.applyselfupdate";

    public const string FILE_APPLYRESULT = ".Aquc.Stackbricks.applyresult.txt";

    public void Execute(UpdateActionData stackbricksAction, UpdatePackage updatePackage)
    {
        var resultFile = FILE_APPLYRESULT;
        var newFileInfo = new FileInfo(updatePackage.file);
        if (File.Exists(resultFile)) File.Delete(resultFile);
        var command =
            "/C timeout /t 5 /nobreak && " +
            $"cd /d \"{newFileInfo.DirectoryName}\" && " +
            $"del /S \"{StackbricksService.PROGRAM_NAME}\" && " +
            "echo ExecuteDelete && " +
            $"rename \"{newFileInfo.Name}\" \"{StackbricksService.PROGRAM_NAME}\" && " +
            "echo ExecuteRename && "+
            $"echo success_executed:{updatePackage.updateMessage.version} > \"{resultFile}\" && " +
            "exit /B && exit";
        StackbricksProgram.logger.Debug($"Apply update command line: cmd {command}");
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = command,
                CreateNoWindow = true
            }
        };
        process.Start();
        StackbricksProgram.logger.Debug("Executed to apply Aquc.Stackbricks.exe updated actions. Aquc.Stackbricks will exit.");
        Environment.Exit(0);
    }
}
