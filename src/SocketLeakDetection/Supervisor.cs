using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Text;
using static SocketLeakDetection.Messages;

namespace SocketLeakDetection
{
    class Supervisor : ReceiveActor
    {

        protected ILoggingAdapter Log = Context.GetLogger();
        public Supervisor(ActorSystem System)
        {
            System.ActorOf(Props.Create(() => new PercentDifference(0, 0, 120, 20, new TcpCounter(), Self)));

            Receive<Stat>(s =>
            {
                if(s.CurretStatus==2)
                {
                    Log.Error("ActorSystem Terminated due to increase in TCP connections");
                    System.Terminate();
                }
                else if (s.CurretStatus == 2)
                {
                    Log.Warning("TCP Connection increase Warning");
                }

            });
        }
        
            

    }
}
