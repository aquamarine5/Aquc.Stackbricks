using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aquc.AquaUpdater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.AquaUpdater.Tests
{
    [TestClass()]
    public class LaunchTests
    {
        LaunchConfig launchConfig;
        [TestMethod()]
        public void GetLaunchConfigTest()
        {
            var _ = new Launch();
            launchConfig = Launch.launchConfig;
        }
    }
}