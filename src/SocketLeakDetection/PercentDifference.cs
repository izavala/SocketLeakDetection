using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static SocketLeakDetection.Messages;

namespace SocketLeakDetection
{
    public class PercentDifference: UntypedActor
    {
        private readonly double alphaL;
        private readonly double alphaS;
        private readonly double _percDif;
        private readonly double _maxDif;
        private IActorRef _supervisor;
        private ITcpCounter _tCounter;
        private double pValueL;
        private double cValueL;
        private double pValueS;
        private double cValueS;
        private bool timerFlag = false;
        public bool primed = false;
        public int c;

        public PercentDifference(double perDif,double maxDif, int largeSample, int smallSample, ITcpCounter counter, IActorRef Supervisor)
        {
            _supervisor = Supervisor;
            _percDif = perDif;
            _maxDif = maxDif;
            _tCounter = counter;
            alphaL = 2.0 / (largeSample + 1);
            alphaS = 2.0 / (smallSample + 1);
            pValueL = counter.GetTcpCount();
            cValueL = EMWA(alphaL, pValueL, counter.GetTcpCount());
            pValueS = counter.GetTcpCount();
            cValueS = EMWA(alphaS, pValueS, counter.GetTcpCount());
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(50), Self, new TcpCount(), ActorRefs.NoSender);
            c = 0;
        }

        protected override void OnReceive(object message)
        {
        
            if (message is TcpCount)
            {
                
                var count = _tCounter.GetTcpCount();
                cValueL = EMWA(alphaL, pValueL, count);
                pValueL = cValueL;
                cValueS = EMWA(alphaS, pValueS, count);
                pValueS = cValueS;
                double dif;

                if (pValueL != 0)
                {
                    dif = (cValueS / cValueL) - 1.0;
                    if (dif > _maxDif)
                        _supervisor.Tell(new Stat { CurretStatus = 2 });
                    else if (dif > 0 && dif > _percDif)
                    {
                        _supervisor.Tell(new Stat { CurretStatus = 1 });
                        if (!timerFlag)
                        {
                            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(60), Self, new TimerExpired(), ActorRefs.NoSender);
                        }
                    }
                    else if (timerFlag && dif < _percDif)
                    { //cancel token;
                    }

                }
            }
            if(message is TimerExpired)
            {
                _supervisor.Tell(new Stat { CurretStatus = 2 });
            }
        }

        public double EMWA(double alpha, double pvalue, int xn )
        {
            return  (alpha * xn) + (1 - alpha) * pvalue;
        }
        
        
    }
}
