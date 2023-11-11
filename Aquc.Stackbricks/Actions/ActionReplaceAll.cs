using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.Actions;


public class ActionReplaceAll : IStackbricksAction
{
    public string ActionId => ID;

    public const string ID = "stbks.action.replaceall";

    public const string FLAG_KEEPZIPFILE = "stbks.action.replaceall.keepzipfile";

    public void Execute(StackbricksActionData stackbricksAction, StackbricksUpdatePackage updatePackage)
    {
        if (!updatePackage.isZip)
            File.Copy(updatePackage.file, Path.Combine(updatePackage.programDir.FullName, Path.GetFileName(updatePackage.file)), true);
        else
        {

            CopyDirectory(updatePackage.depressedDir, updatePackage.programDir);

            StackbricksProgram.logger.Debug($"{ID}: Copy {updatePackage.depressedDir} to {updatePackage.programDir}");
        }

        if (!stackbricksAction.ContainFlag(FLAG_KEEPZIPFILE))
            File.Delete(updatePackage.file);
    }
    private void CopyDirectory(DirectoryInfo directory, DirectoryInfo dest)
    {
        if (!dest.Exists) dest.Create();
        foreach (FileInfo f in directory.GetFiles())
        {
            f.CopyTo(Path.Combine(dest.FullName, f.Name), true);
        }
        foreach (DirectoryInfo d in directory.GetDirectories())
        {
            CopyDirectory(d, new DirectoryInfo(Path.Combine(dest.FullName, d.Name)));
        }
    }
}
