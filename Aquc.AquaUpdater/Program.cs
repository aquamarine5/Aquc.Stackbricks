using System;
using Aquc.AquaUpdater.Pvder;

namespace Aquc.AquaUpdater
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var lc = new Launch();
            new AliyunpanInteraction().DownloadFile("sc.psd", @"D:\Program Source\v2\Aquc.AquaUpdater\Aquc.AquaUpdater");
        }
    }
}
