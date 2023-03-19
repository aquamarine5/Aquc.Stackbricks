using System.Diagnostics;

namespace Aquc.AquaUpdater.Background;

internal class Program
{
    static async Task Main(string[] args)
    {
        await Task.Delay(10000);
        DirectoryInfo directory = new(args[0]);
        DirectoryInfo destination=new(args[1]);
        
        var updateScript = directory.GetFiles("dobefore.*");
        if (updateScript.Length != 0)
        {
            foreach (FileInfo f in updateScript)
            {
                if (f.Extension == ".exe" || f.Extension == ".bat")
                {
                    try
                    {
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = f.FullName,
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                WorkingDirectory=destination.FullName
                            },
                            //EnableRaisingEvents = true
                        };
                        process.Start();
                        //process.BeginOutputReadLine();
                        process.WaitForExit(30000);
                    }
                    catch(Exception ex)
                    {
                        var logsDir = directory.GetDirectories("logs");
                        if (logsDir.Length == 1)
                        {
                            var logFile = logsDir[0].GetFiles($"{DateTime.Now:yyyMMdd}.txt");
                            if (logFile.Length == 1)
                            {
                                await File.AppendAllLinesAsync(logFile[0].FullName, 
                                    new string[] { $"[Exception] [Aquc.AquaUpdater.Background.dobefore] [0] [{DateTime.Now:G}]",$"{ex.Message}" });
                            }
                        }
                    }
                }
            }
        }
        CopyDirectory(directory, destination);
        if (File.Exists(args[2])) File.Delete(args[2]);
        var updateAfterScript = directory.GetFiles("doafter.*");
        if (updateAfterScript.Length != 0)
        {
            foreach (FileInfo f in updateAfterScript)
            {
                if (f.Extension == ".exe" || f.Extension == ".bat")
                {
                    try
                    {
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = f.FullName,
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                WorkingDirectory = directory.FullName
                            },
                            //EnableRaisingEvents = true
                        };
                        process.Start();
                        //process.BeginOutputReadLine();
                        process.WaitForExit(30000);
                    }
                    catch(Exception ex)
                    {
                        var logsDir = directory.GetDirectories("logs");
                        if (logsDir.Length == 1)
                        {
                            var logFile = logsDir[0].GetFiles($"{DateTime.Now:yyyMMdd}.txt");
                            if (logFile.Length == 1)
                            {
                                await File.AppendAllLinesAsync(logFile[0].FullName,
                                    new string[] { $"[Exception] [Aquc.AquaUpdater.Background.doafter] [0] [{DateTime.Now:G}]", $"{ex.Message}" });
                            }
                        }
                    }
                }
            }
        }
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