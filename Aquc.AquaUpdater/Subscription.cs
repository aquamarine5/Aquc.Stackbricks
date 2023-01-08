using System;
using System.Collections.Generic;
using System.Text;

namespace Aquc.AquaUpdater
{
    public class Subscription
    {
        public List<UpdateSubscription> updateSubscriptions;

        public Subscription(List<UpdateSubscription> lups)
        {
            updateSubscriptions = lups;
        }
    }
}
