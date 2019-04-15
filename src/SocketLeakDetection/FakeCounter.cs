using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketLeakDetection
{
    public class FakeCounter : ITcpCounter
    {
        int _currentCount;
        int counter = 0; 

        public FakeCounter(int currentCount)
        {
            _currentCount = currentCount;
            
        }
        public FakeCounter()
        {
            _currentCount = 0;
            
        }
        public int GetTcpCount()
        {
            counter += 1;
            if (counter > 10 && counter < 40 || counter > 70 && counter < 100)
                _currentCount += 10;
            Console.WriteLine("TCP : {0}",_currentCount);
            return _currentCount;
        }
        public void IncreaseCount(int increase)
        {
            _currentCount += increase;
        }
        
        
    }
}

