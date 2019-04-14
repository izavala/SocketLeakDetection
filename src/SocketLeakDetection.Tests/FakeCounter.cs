using System;
using System.Collections.Generic;
using System.Text;

namespace SocketLeakDetection.Tests
{
    class FakeCounter : ITcpCounter
    {
        int _currentCount;
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
            return _currentCount;
        }
        public void IncreaseCount(int increase)
        {
            _currentCount += increase;
        }
    }
}
