using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;
using static SocketLeakDetection.Messages;

namespace SocketLeakDetection
{
    class Watcher:ReceiveActor
    {
        public Watcher()
        {
            Receive<Stat>(s =>
            {
                Console.Write(s.CurretStatus);
            });
        }
    }
}
