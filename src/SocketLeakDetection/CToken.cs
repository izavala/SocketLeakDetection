using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketLeakDetection
{
    class CToken : ICancelable
    {
        public bool IsCancellationRequested => throw new NotImplementedException();

        public CancellationToken Token => throw new NotImplementedException();

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void Cancel(bool throwOnFirstException)
        {
            throw new NotImplementedException();
        }

        public void CancelAfter(TimeSpan delay)
        {
            throw new NotImplementedException();
        }

        public void CancelAfter(int millisecondsDelay)
        {
            throw new NotImplementedException();
        }
    }
}
