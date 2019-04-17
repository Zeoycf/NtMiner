﻿using NTMiner.Bus;
using NTMiner.Core;

namespace NTMiner {
    public class StartingMineFailedEvent : EventBase {
        [MessageType(messageType: typeof(StartingMineFailedEvent), description: "开始挖矿不成功")]
        public StartingMineFailedEvent(string message) {
            this.Message = message;
        }

        public string Message { get; private set; }
    }

    [MessageType(messageType: typeof(MineStartedEvent), description: "挖矿开始事件")]
    public class MineStartedEvent : EventBase {
        public MineStartedEvent(IMineContext mineContext) {
            this.MineContext = mineContext;
        }
        public IMineContext MineContext { get; private set; }
    }

    [MessageType(messageType: typeof(MineStartedEvent), description: "挖矿停止事件")]
    public class MineStopedEvent : EventBase {
        public MineStopedEvent(IMineContext mineContext) {
            this.MineContext = mineContext;
        }
        public IMineContext MineContext { get; private set; }
    }

    [MessageType(messageType: typeof(ShowMainWindowCommand), description: "显式主界面")]
    public class ShowMainWindowCommand : Cmd {
    }

    [MessageType(messageType: typeof(CloseNTMinerCommand), description: "关闭NTMiner客户端")]
    // ReSharper disable once InconsistentNaming
    public class CloseNTMinerCommand : Cmd {
    }

    [MessageType(messageType: typeof(RefreshAutoBootStartCommand), description: "刷新开机启动和自动挖矿")]
    public class RefreshAutoBootStartCommand : Cmd {
    }
}
