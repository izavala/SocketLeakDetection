
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketLeakDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            var Sys = ActorSystem.Create("Test");
            //var watcher = Sys.ActorOf(Props.Create(() => new Watcher()));
            //var sup = Sys.ActorOf(Props.Create(() => new Supervisor(Sys)));
            
            
        }
    }
}
