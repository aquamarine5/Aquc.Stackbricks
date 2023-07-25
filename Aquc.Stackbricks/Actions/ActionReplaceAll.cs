using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks.Actions;


public class ActionReplaceAll : IStackbricksAction
{
    public string Id => _ActionId;

    public static readonly string _ActionId = "stbks.action.replaceall";
    public void Execute(StackbricksActionData stackbricksAction, StackbricksUpdatePackage updatePackage)
    {
        CopyDirectory(updatePackage.depressedDir, updatePackage.programDir);
        if (!stackbricksAction.ContainFlag("stbks.action.replaceall.keepzipfile"))
            File.Delete(updatePackage.zipFile);
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
