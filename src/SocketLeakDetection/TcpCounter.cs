﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace SocketLeakDetection
{
    public class TcpCounter : ITcpCounter
    {
        public int GetTcpCount(IPAddress Address)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] iPEndPoints = ipProperties.GetActiveTcpListeners();

            var ports = from points in iPEndPoints
                        where points.Address.ToString().Contains(Address.ToString())
                        select points.Port;

            return ports.Count();
        }
    }
}
