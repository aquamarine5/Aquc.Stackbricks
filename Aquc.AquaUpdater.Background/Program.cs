using System.Diagnostics;

namespace Aquc.AquaUpdater.Background;

internal class Program
{
    static async Task Main(string[] args)
    {
        await Task.Delay(10000);
        DirectoryInfo directory = new(args[0]);
        DirectoryInfo destination=new(args[1]);
        
        var updateScript = directory.GetFiles("do.*");
        if (updateScript.Length != 0)
        {
            foreach (FileInfo f in updateScript)
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = f.FullName,
                            CreateNoWindow = true,
                            UseShellExecute = false
                        },
                        EnableRaisingEvents = true
                    };
                    process.Start();
                    //process.BeginOutputReadLine();
                    process.WaitForExit(30000);
                }
                catch
                {

                }
            }
        }
        CopyDirectory(directory, destination);
        if (File.Exists(args[2])) File.Delete(args[2]);
    }
    private static void CopyDirectory(DirectoryInfo directory, DirectoryInfo dest)
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