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
            var Watcher = Sys.ActorOf(Props.Create( ()=> new PercentDifference(0.1,0.2,120,20,new FakeCounter(5,600),TestActor)));
            // AwaitAssert(Assert., TimeSpan.FromMinutes(10))
            var x = Console.ReadLine();
;            
        }
    }

}
