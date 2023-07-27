using Aquc.Stackbricks.MsgPvder;
using Aquc.Stackbricks.PkgPvder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public interface IStackbricksPkgPvder
{
    public string PkgPvderId { get; }
    public Task<StackbricksUpdatePackage> DownloadPackageAsync(StackbricksUpdateMessage updateMessage, string savePosition);
    public Task<StackbricksUpdatePackage> DownloadPackageAsync(StackbricksUpdateMessage updateMessage, string savePosition, string zipFileName);

    public Task<StackbricksUpdatePackage> DownloadFileAsync(StackbricksUpdateMessage updateMessage, string savePosition, string fileName = "");
}

public class StackbricksPkgPvderManager
{
    readonly static Dictionary<string, IStackbricksPkgPvder> matchDict = new()
    {
        {"stbks.pkgpvder.ghproxy",new GhProxyPkgPvder() }
    };
    public static IStackbricksPkgPvder ParsePkgPvder(string pkgPvderId)
    {
        // ncpe
        return matchDict[pkgPvderId];
    }
}
