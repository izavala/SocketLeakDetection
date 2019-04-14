using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Xunit;
using Xunit.Abstractions;
using static SocketLeakDetection.Messages;

namespace SocketLeakDetection.Tests
{
    public class PercentDifferenceWarningSpecs : TestKit 

    {

        [Fact]
        public void MessageShouldBeSentWhenRiseIsHigh()
        {
            var counter = new FakeCounter(600);
            var Watcher = Sys.ActorOf(Props.Create( ()=> new PercentDifference(0.1,0.2,120,20,counter,TestActor)));
            // AwaitAssert(Assert., TimeSpan.FromMinutes(10))
            for(var i = 0; i<1000000; i++)
            {
                if(i%1000==0)
                counter.IncreaseCount(1);
            }
            ExpectMsg<Stat>().CurretStatus.Equals(2);
;            
        }
    }

}
