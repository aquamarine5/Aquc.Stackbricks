using System.IO;
using System;
using Aesc.AwesomeKits.ArgsParser;
using Aesc.AwesomeKits.NetdiskService;
using Aesc.AwesomeKits.TaskScheduler;
using Microsoft.Win32;
using System.Diagnostics;

namespace Aesc.AwesomeProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                FileInfo file = new FileInfo("./AwesomeProgram.launch.json");
                var stream = file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamReader streamReader = new StreamReader(stream);
                string s = streamReader.ReadToEnd();
                if (s == "" || s == "n")
                {
                    RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(@"directory\shell", true);
                    if (registryKey == null) registryKey = Registry.ClassesRoot.CreateSubKey(@"directory\shell");
                    RegistryKey rightCommandKey = registryKey.CreateSubKey("AwesomeCore.JProgram");
                    RegistryKey associatedKey = rightCommandKey.CreateSubKey("command");
                    associatedKey.SetValue("", $"\"{Process.GetCurrentProcess().MainModule.FileName}\" \"%1\" -directory");
                }
            }
            else
            {
                Console.WriteLine(args[1]);
                string c = "D:\\Program Source\\AwesomeJProgram";
                Directory.CreateDirectory(c);
                string k = args[0];
                string p = args[1];
                if (k == "-directory")
                {
                    string f = c + $"\\{DateTime.Now:MMddHHmm}.zip";
                    if (File.Exists(f)) File.Delete(f);
                    Console.WriteLine(f);
                    System.IO.Compression.ZipFile.CreateFromDirectory(p, f);
                    AescNetdiskService.Upload(f);
                }
                else if (k == "-file")
                {
                    AescNetdiskService.Upload(p);
                }
            }
        }
    }
}
