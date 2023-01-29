using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aquc.AquaUpdater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Aquc.AquaUpdater.Tests
{
    [TestClass()]
    public class UpdaterProgramTests
    {
        [TestMethod()]
        public void MainTest()
        {
            using var host=new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile(UpdaterService.CONFIG_JSON);
                })
                .Build();
            
        }
    }
}