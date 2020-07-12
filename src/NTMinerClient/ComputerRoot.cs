﻿using OpenHardwareMonitor.Hardware;

namespace NTMiner {
    public static class ComputerRoot {
        private static readonly object _locker = new object();
        #region 电脑硬件
        private static Computer _computer = null;
        public static Computer Computer {
            get {
                if (_computer == null) {
                    lock (_locker) {
                        if (_computer == null) {
                            _computer = new Computer();
                            _computer.Open();
                            _computer.CPUEnabled = true;
                            VirtualRoot.AddEventPath<AppExitEvent>($"程序退出时关闭OpenHardwareMonitor", LogEnum.None,
                                message => {
                                    _computer?.Close();
                                }, typeof(VirtualRoot));
                        }
                    }
                }
                return _computer;
            }
        }
        #endregion
    }
}