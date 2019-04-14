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
        private readonly double _percDif; // Percent difference between Large Sample and Small Sammple.
        private readonly double _maxDif; // Maximum set percent difference between Large Sample and Small Sample. 
        private IActorRef _supervisor;
        private ITcpCounter _tCounter; //TCP counter set to be used.
        private double pValueL; // Past Average for Large Sample
        private double cValueL; // Current Average for Large Sample
        private double pValueS; // Past Average for Small Sample
        private double cValueS; // Current Average for Small Sample
        private bool timerFlag = false; //Flag to signal increase in TCP connections

        public PercentDifference(double perDif,double maxDif, int largeSample, int smallSample, ITcpCounter counter, IActorRef Supervisor)
        {
            _supervisor = Supervisor;
            _percDif = perDif;
            _maxDif = maxDif;
            _tCounter = counter;
            alphaL = 2.0 / (largeSample + 1);
            alphaS = 2.0 / (smallSample + 1);
            pValueL = counter.GetTcpCount(); // Set initial value for Large Sample Weighted Average
            pValueS = counter.GetTcpCount(); // Set initial value for Small Sample Weighted Average
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(50), Self, new TcpCount(), ActorRefs.NoSender); //Schedule TCP counts to happen every 500 ms.
            
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
                    dif = (cValueS / cValueL) - 1.0;  // Get percent difference between the two readings.
                    if (dif > _maxDif)  // If difference exceeds max set difference alert supervisor. 
                        _supervisor.Tell(new Stat { CurretStatus = 2 });
                    else if (dif > 0 && dif > _percDif) // If difference is below Max difference but above warning level inform supervisor. 
                    {
                        _supervisor.Tell(new Stat { CurretStatus = 1 });
                        if (!timerFlag)
                        {
                            // Begin timer, if percent difference above warning level start timer to signal if this increase continues
                            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(60), Self, new TimerExpired(), ActorRefs.NoSender);
                        }
                    }
                    else if (timerFlag && dif < _percDif)
                    { //cancel token;
                        timerFlag = false;
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
