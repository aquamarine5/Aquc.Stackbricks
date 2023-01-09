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
        static ILogger<UpdateMessage> s_UpdateMessageLogger=null;
        public static ILogger<UpdateMessage> UpdateMessageLogger => s_UpdateMessageLogger ??= InitLogger<UpdateMessage>();

        static ILogger<UpdateSubscription> s_UpdateSubscriptionLogger = null;
        public static ILogger<UpdateSubscription> UpdateSubscriptionLogger => s_UpdateSubscriptionLogger ??= InitLogger<UpdateSubscription>();

        static ILogger<UpdatePackage> s_UpdatePackageLogger = null;
        public static ILogger<UpdatePackage> UpdatePackageLogger => s_UpdatePackageLogger ??= InitLogger<UpdatePackage>();
    }
}
