﻿using Aquc.Stackbricks.MsgPvder;
using Aquc.Stackbricks.PkgPvder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public interface IUpdatePkgPvder
{
    public string PkgPvderId { get; }
    public Task<UpdatePackage> DownloadPackageAsync(UpdateMessage updateMessage, string savePosition);
    public Task<UpdatePackage> DownloadPackageAsync(UpdateMessage updateMessage, string savePosition, string zipFileName);
    public Task<UpdatePackage> DownloadFileAsync(UpdateMessage updateMessage, string savePosition, string fileName = "");
}

public class PackagePvderManager
{
    readonly static Dictionary<string, IUpdatePkgPvder> matchDict = new()
    {
        {"stbks.pkgpvder.ghproxy",new GhProxyPkgPvder() }
    };
    public static IUpdatePkgPvder ParsePkgPvder(string pkgPvderId)
    {
        // ncpe
        return matchDict[pkgPvderId];
    }
}
