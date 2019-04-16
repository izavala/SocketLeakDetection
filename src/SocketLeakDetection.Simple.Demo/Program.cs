// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2019 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Configuration;
using System;
using System.Net;
using System.Net.Sockets;

namespace SocketLeakDetection.Simple.Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            /*
             * Simple demo illustrating the monitoring of local TCP endpoints
             * that are currently running on the system somewhere. Shuts itself down
             * if a port leak is detected.
             */
            //var config = ConfigurationFactory.ParseString(@"akka{loglevel = ""DEBUG"" remote{dot-netty.tcp{hostname = ""localhost"" port = 0}}}");
            var actorSystem = ActorSystem.Create("PortDetector", "akka.loglevel = DEBUG");
            var supervisor = actorSystem.ActorOf(Props.Create(() => new TcpPortUseSupervisor()), "tcpPorts");
            
            Console.ReadLine();
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(localAddr,8087);
            TcpClient[] clients = new TcpClient[100];
            Byte[] bytes = new Byte[256];
            String data = null;
            server.Start();

            for (var i = 0; i < 100; i++)
            {

                clients[i] = server.AcceptTcpClient();
                clients[i].Connect(localAddr, 8087);
                NetworkStream stream = clients[i].GetStream();
                int ii;

                // Loop to receive all the data sent by the client.
                while ((ii = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);

                    // Process the data sent by the client.
                    data = data.ToUpper();

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent: {0}", data);

                }
            }
            actorSystem.WhenTerminated.Wait();
            
        }
    }
}