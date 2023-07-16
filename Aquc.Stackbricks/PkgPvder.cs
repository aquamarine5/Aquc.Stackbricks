using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public interface IStackbricksPkgPvder
{
    public string PkgPvderId { get; }
    public Task<StacebricksUpdatePackage> DownloadPackageAsync(string data,string savePosition);
}
