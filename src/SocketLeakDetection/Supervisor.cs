using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using System;
using System.IO;
using static SocketLeakDetection.Messages;

namespace SocketLeakDetection
{
    public class Supervisor : ReceiveActor
    {

        protected ILoggingAdapter Log = Context.GetLogger();
        public Supervisor(ActorSystem System, Config config)
        {
            GetConfig(config);
            System.ActorOf(Props.Create(() => new PercentDifference(MaxConnections,PercentDifference, MaxDifference, LargeSample, SmallSample, new TcpCounter(), Self)));
            Receive<Stat>(s =>
            {
                if (s.CurretStatus == 2)
                {
                    Log.Error("ActorSystem Terminated due to increase in TCP connections");
                    System.Terminate();
                }
                else if (s.CurretStatus == 1)
                {
                    Log.Warning("TCP Connection increase Warning");
                }

            });
        }

        private void GetConfig(Config config)
        {
           
            var actorConfig = config.GetConfig("SLD");
            if (actorConfig != null)
            {
                var mx = Convert.ToInt64(actorConfig.GetString("Max-Connections", "16777214"));
                if (mx>0)
                    MaxConnections = mx;
                else
                {
                    Log.Warning("Percent Difference value not between 0 and 1, setting to 0.20");
                    MaxConnections = 16777214;
                }

                var pd = Convert.ToDouble(actorConfig.GetString("Percent-Difference", "0.20"));
                if (pd < 1 && pd > 0)
                    PercentDifference = pd;
                else
                {
                    Log.Warning("Percent Difference value not between 0 and 1, setting to 0.20");
                    PercentDifference = 0.20;
                }

                var md = Convert.ToDouble(actorConfig.GetString("Max-Difference", "0.25"));
                if (md < 1 && md > 0)
                    MaxDifference = md;
                else
                {
                    Log.Warning("Max Difference value not between 0 and 1, setting to 0.25");
                    MaxDifference = 0.25;
                }

                var ls = Convert.ToInt32(actorConfig.GetString("Large-Sample", "120"));
                if (ls > 2)
                    LargeSample = ls;
                else
                {
                    Log.Warning("Large Sample must be greater than 2, setting to 120");
                    LargeSample = 120;
                }

                var ss = Convert.ToInt32(actorConfig.GetString("Small-Sample", "20"));
                if (ss < ls)
                    SmallSample = ss;
                else
                {
                    Log.Warning("Small Sample must be greater than 1 and smaller than Large Sample, setting to 1");
                    SmallSample = 1; //alpha cannot be bigger than 1
                }

            }
        }
        private long MaxConnections { get; set; }
        private double PercentDifference { get; set; }
        private double MaxDifference { get; set; }
        private int LargeSample { get; set; }
        private int SmallSample { get; set; }

    }
}
