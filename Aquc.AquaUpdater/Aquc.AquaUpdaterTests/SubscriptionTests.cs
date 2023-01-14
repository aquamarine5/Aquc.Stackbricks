using Aquc.AquaUpdater;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.AquaUpdaterTests
{
    [TestClass()]
    public class SubscriptionTests
    {
        Dictionary<string,UpdateSubscription>? subscription;
        [TestInitialize] public void Init() 
        { 

        }
        [TestMethod()]
        public void ReadSubscription()
        {
            subscription = Launch.GetLaunchConfig().subscriptions;
        }
        [TestMethod()]
        public void AddSubscription()
        {
                Assert.Fail();
        }
        [TestMethod()]
        public void AddSubscriptionByJson()
        {

        }

        
        [TestMethod()]
        public void EditSubscription()
        {

        }
        [TestMethod()]
        public void RemoveSubscription()
        {

        }
    }
}
