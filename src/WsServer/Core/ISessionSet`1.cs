﻿using System;
using WebSocketSharp.Server;

namespace NTMiner.Core {
    public interface ISessionSet<TSession> where TSession : ISession {
        WebSocketSessionManager WsSessionManager { get; }
        int Count { get; }
        void Add(TSession ntminerSession);
        TSession RemoveByWsSessionId(string wsSessionId);
        bool TryGetByClientId(Guid clientId, out TSession ntminerSession);
        bool TryGetByWsSessionId(string wsSessionId, out TSession ntminerSession);
        bool ActiveByWsSessionId(string wsSessionId, out TSession ntminerSession);
    }
}
