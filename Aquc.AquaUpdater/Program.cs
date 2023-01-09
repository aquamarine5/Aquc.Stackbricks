using System;
using Aquc.AquaUpdater.Pvder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Aquc.AquaUpdater
{
    public class Program
    {
        static ILogger<Program> logger;
        static void Main(string[] args)
        {
            logger = Logging.InitLogger<Program>();
            Console.WriteLine("Hello World!");
            var lc = new Launch();
            
        }
    }
}
