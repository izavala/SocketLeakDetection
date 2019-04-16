﻿// -----------------------------------------------------------------------
// <copyright file="TcpPortUseSupervisor.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2019 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;

namespace SocketLeakDetection
{
    /// <summary>
    ///     Actor responsible for instantiating the monitoring system.
    /// </summary>
    public class TcpPortUseSupervisor : ReceiveActor
    {
        private readonly Props _childProps;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly SocketLeakDetectorSettings _settings;
        private IActorRef _tcpScanner;

        /// <summary>
        ///     Supervisor actor used to determine if we need to log warning about increase in TCP connections or terminate the
        ///     Actor System.
        ///     Created using the default <see cref="SocketLeakDetectorSettings" />.
        /// </summary>
        public TcpPortUseSupervisor() : this(new SocketLeakDetectorSettings())
        {
        }

        /// <summary>
        ///     Supervisor actor used to determine if we need to log warning about increase in TCP connections or terminate the
        ///     Actor System.
        /// </summary>
        public TcpPortUseSupervisor(SocketLeakDetectorSettings settings)
        {
            _settings = settings;
            _childProps = Props.Create(() => new SocketLeakDetectorActor(_settings, Self));

            Receive<TcpCount>(t =>
            {
                var childName = Uri.EscapeUriString(t.HostInterface.ToString());
                var child = Context.Child(childName).GetOrElse(() =>
                    Context.ActorOf(_childProps, childName));

                child.Forward(t);
            });

            Receive<Shutdown>(_ =>
            {
                _log.Warning("Received shutdown notification from LeakDetector. Terminating ActorSystem");

                // trigger shutdown using custom reason
                CoordinatedShutdown.Get(Context.System).Run(PortLeakReason.Instance);
            });
        }

        protected override void PreStart()
        {
            _tcpScanner = Context.ActorOf(Props.Create(() => new TcpPortMonitoringActor(Self, _settings)),
                "portMonitor");
            Context.Watch(_tcpScanner);
        }

        /// <summary>
        ///     INTERNAL API - signals <see cref="ActorSystem" /> termination.
        /// </summary>
        public sealed class Shutdown
        {
            public static readonly Shutdown Instance = new Shutdown();

            private Shutdown()
            {
            }
        }

        private sealed class PortLeakReason : CoordinatedShutdown.Reason
        {
            public static readonly PortLeakReason Instance = new PortLeakReason();

            private PortLeakReason()
            {
            }

            public override string ToString()
            {
                return "Port exhaustion threshold breached";
            }
        }
    }
}