using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.AquaUpdater
{
    public class Logging
    {
        public static ILogger<T> InitLogger<T>()
        {
            return LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<T>();
        }
    }
}
