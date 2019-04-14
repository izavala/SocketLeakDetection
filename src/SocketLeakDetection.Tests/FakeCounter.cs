using System;
using System.Collections.Generic;
using System.Text;

namespace SocketLeakDetection.Tests
{
    class FakeCounter : ITcpCounter
    {
        int _currentCount;
        int _inc;
        int count; 
        public FakeCounter(int increments, int currentCount)
        {
            _currentCount = currentCount;
            _inc = increments;
            count = 0;
        }
        public int GetTcpCount()
        {
            count += 1;
            if (count < 1000 && count > 950)
            {
                _currentCount = _currentCount + _inc;
                return _currentCount;
            }
            else
            {
                return _currentCount;
            }
        }
    }
}
