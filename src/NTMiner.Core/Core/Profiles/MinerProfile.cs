﻿using NTMiner.Core.Kernels;
using NTMiner.MinerServer;
using NTMiner.Profile;
using NTMiner.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NTMiner.Core.Profiles {
    internal class MinerProfile : IWorkMinerProfile {
        private readonly INTMinerRoot _root;

        private MinerProfileData _data;
        private readonly Guid _workId;
        private readonly CoinKernelProfileSet _coinKernelProfileSet;
        private readonly CoinProfileSet _coinProfileSet;
        private readonly PoolProfileSet _poolProfileSet;
        private readonly WalletSet _walletSet;

        public MinerProfile(INTMinerRoot root, Guid workId) {
            _root = root;
            _workId = workId;
            _coinKernelProfileSet = new CoinKernelProfileSet(root, workId);
            _coinProfileSet = new CoinProfileSet(root, workId);
            _poolProfileSet = new PoolProfileSet(root, workId);
            _walletSet = new WalletSet(root, workId);
            _data = GetMinerProfileData();
            if (_data == null) {
                throw new ValidationException("未获取到MinerProfileData数据，请重试");
            }
        }
        private MinerProfileData GetMinerProfileData() {
            if (_workId != Guid.Empty) {
                return Server.ProfileService.GetMinerProfile(_workId);
            }
            else {
                IRepository<MinerProfileData> repository = NTMinerRoot.CreateLocalRepository<MinerProfileData>();
                var result = repository.GetAll().FirstOrDefault();
                if (result == null) {
                    result = MinerProfileData.CreateDefaultData();
                }
                else if (result.IsAutoThisPCName) {
                    result.MinerName = GetThisPcName();
                }
                return result;
            }
        }

        #region IMinerProfile
        public Guid GetId() {
            return this.Id;
        }

        public Guid Id {
            get { return _data.Id; }
            private set {
                _data.Id = value;
            }
        }

        public string MinerName {
            get => _data.MinerName;
            private set {
                if (string.IsNullOrEmpty(value)) {
                    value = GetThisPcName();
                }
                value = new string(value.ToCharArray().Where(a => !invalidChars.Contains(a)).ToArray());
                if (_data.MinerName != value) {
                    _data.MinerName = value;
                    VirtualRoot.Execute(new RefreshArgsAssemblyCommand());
                }
            }
        }

        private static readonly char[] invalidChars = { '.', ' ', '-', '_' };
        public string GetThisPcName() {
            string value = Environment.MachineName.ToLower();
            value = new string(value.ToCharArray().Where(a => !invalidChars.Contains(a)).ToArray());
            return value;
        }

        public bool IsAutoThisPCName {
            get { return _data.IsAutoThisPCName; }
            private set {
                if (_data.IsAutoThisPCName != value) {
                    _data.IsAutoThisPCName = value;
                    if (value) {
                        this.MinerName = string.Empty;
                    }
                }
            }
        }

        public bool IsShowInTaskbar {
            get { return _data.IsShowInTaskbar; }
            private set {
                if (_data.IsShowInTaskbar != value) {
                    _data.IsShowInTaskbar = value;
                }
            }
        }

        public bool IsAutoBoot {
            get => _data.IsAutoBoot;
            private set {
                if (_data.IsAutoBoot != value) {
                    _data.IsAutoBoot = value;
                    NTMinerRegistry.SetIsAutoBoot(value);
                }
            }
        }

        public bool IsNoShareRestartKernel {
            get => _data.IsNoShareRestartKernel;
            private set {
                if (_data.IsNoShareRestartKernel != value) {
                    _data.IsNoShareRestartKernel = value;
                }
            }
        }
        public int NoShareRestartKernelMinutes {
            get => _data.NoShareRestartKernelMinutes;
            private set {
                if (_data.NoShareRestartKernelMinutes != value) {
                    _data.NoShareRestartKernelMinutes = value;
                }
            }
        }
        public bool IsPeriodicRestartKernel {
            get => _data.IsPeriodicRestartKernel;
            private set {
                if (_data.IsPeriodicRestartKernel != value) {
                    _data.IsPeriodicRestartKernel = value;
                }
            }
        }
        public int PeriodicRestartKernelHours {
            get => _data.PeriodicRestartKernelHours;
            private set {
                if (_data.PeriodicRestartKernelHours != value) {
                    _data.PeriodicRestartKernelHours = value;
                }
            }
        }
        public bool IsPeriodicRestartComputer {
            get => _data.IsPeriodicRestartComputer;
            private set {
                if (_data.IsPeriodicRestartComputer != value) {
                    _data.IsPeriodicRestartComputer = value;
                }
            }
        }
        public int PeriodicRestartComputerHours {
            get => _data.PeriodicRestartComputerHours;
            private set {
                if (_data.PeriodicRestartComputerHours != value) {
                    _data.PeriodicRestartComputerHours = value;
                }
            }
        }

        public bool IsAutoStart {
            get => _data.IsAutoStart;
            private set {
                if (_data.IsAutoStart != value) {
                    _data.IsAutoStart = value;
                }
            }
        }

        public bool IsAutoRestartKernel {
            get {
                return _data.IsAutoRestartKernel;
            }
            private set {
                if (_data.IsAutoRestartKernel != value) {
                    _data.IsAutoRestartKernel = value;
                }
            }
        }

        public bool IsShowCommandLine {
            get { return _data.IsShowCommandLine; }
            set {
                _data.IsShowCommandLine = value;
            }
        }

        public Guid CoinId {
            get => _data.CoinId;
            private set {
                if (_data.CoinId != value) {
                    _data.CoinId = value;
                }
            }
        }

        private static Dictionary<string, PropertyInfo> _properties;
        private static Dictionary<string, PropertyInfo> Properties {
            get {
                if (_properties == null) {
                    _properties = new Dictionary<string, PropertyInfo>();
                    foreach (var item in typeof(MinerProfile).GetProperties()) {
                        _properties.Add(item.Name, item);
                    }
                }
                return _properties;
            }
        }

        public int WalletCount => throw new NotImplementedException();

        public void SetValue(string propertyName, object value) {
            if (Properties.TryGetValue(propertyName, out PropertyInfo propertyInfo)) {
                if (propertyInfo.CanWrite) {
                    if (propertyInfo.PropertyType == typeof(Guid)) {
                        value = DictionaryExtensions.ConvertToGuid(value);
                    }
                    propertyInfo.SetValue(this, value, null);
                    if (_workId != Guid.Empty) {
                        if (CommandLineArgs.IsControlCenter) {
                            Server.ControlCenterService.SetMinerProfilePropertyAsync(_workId, propertyName, value, isSuccess => {
                                VirtualRoot.Happened(new MinerProfilePropertyChangedEvent(propertyName));
                            });
                        }
                    }
                    else {
                        IRepository<MinerProfileData> repository = NTMinerRoot.CreateLocalRepository<MinerProfileData>();
                        repository.Update(_data);
                        VirtualRoot.Happened(new MinerProfilePropertyChangedEvent(propertyName));
                    }
                }
            }
        }

        public object GetValue(string propertyName) {
            if (Properties.TryGetValue(propertyName, out PropertyInfo propertyInfo)) {
                if (propertyInfo.CanRead) {
                    return propertyInfo.GetValue(this, null);
                }
            }
            return null;
        }
        #endregion

        public ICoinKernelProfile GetCoinKernelProfile(Guid coinKernelId) {
            return _coinKernelProfileSet.GetCoinKernelProfile(coinKernelId);
        }

        public void SetCoinKernelProfileProperty(Guid coinKernelId, string propertyName, object value) {
            _coinKernelProfileSet.SetCoinKernelProfileProperty(coinKernelId, propertyName, value);
        }

        public ICoinProfile GetCoinProfile(Guid coinId) {
            return _coinProfileSet.GetCoinProfile(coinId);
        }

        public void SetCoinProfileProperty(Guid coinId, string propertyName, object value) {
            _coinProfileSet.SetCoinProfileProperty(coinId, propertyName, value);
        }

        public IPoolProfile GetPoolProfile(Guid poolId) {
            return _poolProfileSet.GetPoolProfile(poolId);
        }

        public void SetPoolProfileProperty(Guid poolId, string propertyName, object value) {
            _poolProfileSet.SetPoolProfileProperty(poolId, propertyName, value);
        }

        public bool ContainsWallet(Guid walletId) {
            return _walletSet.ContainsWallet(walletId);
        }

        public bool TryGetWallet(Guid walletId, out IWallet wallet) {
            return _walletSet.TryGetWallet(walletId, out wallet);
        }

        public IEnumerable<IWallet> GetAllWallets() {
            return _walletSet.GetAllWallets();
        }

        #region CoinKernelProfileSet
        public class CoinKernelProfileSet {
            private readonly Dictionary<Guid, CoinKernelProfile> _dicById = new Dictionary<Guid, CoinKernelProfile>();

            private readonly INTMinerRoot _root;
            private readonly object _locker = new object();

            private readonly Guid _workId;
            public CoinKernelProfileSet(INTMinerRoot root, Guid workId) {
                _root = root;
                _workId = workId;
            }

            public ICoinKernelProfile GetCoinKernelProfile(Guid coinKernelId) {
                if (_dicById.ContainsKey(coinKernelId)) {
                    return _dicById[coinKernelId];
                }
                lock (_locker) {
                    if (_dicById.ContainsKey(coinKernelId)) {
                        return _dicById[coinKernelId];
                    }
                    CoinKernelProfile coinKernelProfile = CoinKernelProfile.Create(_root, _workId, coinKernelId);
                    _dicById.Add(coinKernelId, coinKernelProfile);

                    return coinKernelProfile;
                }
            }

            public void SetCoinKernelProfileProperty(Guid coinKernelId, string propertyName, object value) {
                CoinKernelProfile coinKernelProfile = (CoinKernelProfile)GetCoinKernelProfile(coinKernelId);
                coinKernelProfile.SetValue(propertyName, value);
            }

            private class CoinKernelProfile : ICoinKernelProfile {
                public static readonly CoinKernelProfile Empty = new CoinKernelProfile(NTMinerRoot.Current, Guid.Empty);

                public static CoinKernelProfile Create(INTMinerRoot root, Guid workId, Guid coinKernelId) {
                    if (root.CoinKernelSet.TryGetCoinKernel(coinKernelId, out ICoinKernel coinKernel)) {
                        CoinKernelProfile coinProfile = new CoinKernelProfile(root, workId, coinKernel);

                        return coinProfile;
                    }
                    else {
                        return Empty;
                    }
                }

                private readonly Guid _workId;
                private CoinKernelProfile(INTMinerRoot root, Guid workId) {
                    _root = root;
                    _workId = workId;
                }

                private CoinKernelProfileData GetCoinKernelProfileData(Guid coinKernelId) {
                    if (_workId != Guid.Empty) {
                        return Server.ProfileService.GetCoinKernelProfile(_workId, coinKernelId);
                    }
                    else {
                        IRepository<CoinKernelProfileData> repository = NTMinerRoot.CreateLocalRepository<CoinKernelProfileData>();
                        var result = repository.GetByKey(coinKernelId);
                        if (result == null) {
                            result = CoinKernelProfileData.CreateDefaultData(coinKernelId);
                        }
                        return result;
                    }
                }

                private readonly INTMinerRoot _root;
                private CoinKernelProfileData _data;
                private CoinKernelProfile(INTMinerRoot root, Guid workId, ICoinKernel coinKernel) {
                    _root = root;
                    _workId = workId;
                    _data = GetCoinKernelProfileData(coinKernel.GetId());
                    if (_data == null) {
                        throw new ValidationException("未获取到CoinKernelProfileData数据，请重试");
                    }
                }

                public Guid CoinKernelId {
                    get => _data.CoinKernelId;
                    private set {
                        if (_data.CoinKernelId != value) {
                            _data.CoinKernelId = value;
                        }
                    }
                }

                public bool IsDualCoinEnabled {
                    get => _data.IsDualCoinEnabled;
                    private set {
                        if (_data.IsDualCoinEnabled != value) {
                            _data.IsDualCoinEnabled = value;
                        }
                    }
                }
                public Guid DualCoinId {
                    get => _data.DualCoinId;
                    private set {
                        if (_data.DualCoinId != value) {
                            _data.DualCoinId = value;
                        }
                    }
                }

                public double DualCoinWeight {
                    get => _data.DualCoinWeight;
                    private set {
                        if (_data.DualCoinWeight != value) {
                            _data.DualCoinWeight = value;
                        }
                    }
                }

                public bool IsAutoDualWeight {
                    get => _data.IsAutoDualWeight;
                    private set {
                        if (_data.IsAutoDualWeight != value) {
                            _data.IsAutoDualWeight = value;
                        }
                    }
                }

                public string CustomArgs {
                    get => _data.CustomArgs;
                    private set {
                        if (_data.CustomArgs != value) {
                            _data.CustomArgs = value;
                        }
                    }
                }

                private static Dictionary<string, PropertyInfo> _properties;
                private static Dictionary<string, PropertyInfo> Properties {
                    get {
                        if (_properties == null) {
                            _properties = new Dictionary<string, PropertyInfo>();
                            foreach (var item in typeof(CoinKernelProfile).GetProperties()) {
                                _properties.Add(item.Name, item);
                            }
                        }
                        return _properties;
                    }
                }

                public void SetValue(string propertyName, object value) {
                    if (Properties.TryGetValue(propertyName, out PropertyInfo propertyInfo)) {
                        if (propertyInfo.CanWrite) {
                            if (propertyInfo.PropertyType == typeof(Guid)) {
                                value = DictionaryExtensions.ConvertToGuid(value);
                            }
                            propertyInfo.SetValue(this, value, null);
                            if (_workId != Guid.Empty) {
                                if (CommandLineArgs.IsControlCenter) {
                                    Server.ControlCenterService.SetCoinKernelProfilePropertyAsync(_workId, CoinKernelId, propertyName, value, isSuccess => {
                                        VirtualRoot.Happened(new CoinKernelProfilePropertyChangedEvent(this.CoinKernelId, propertyName));
                                    });
                                }
                            }
                            else {
                                IRepository<CoinKernelProfileData> repository = NTMinerRoot.CreateLocalRepository<CoinKernelProfileData>();
                                repository.Update(_data);
                                VirtualRoot.Happened(new CoinKernelProfilePropertyChangedEvent(this.CoinKernelId, propertyName));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region CoinProfileSet
        public class CoinProfileSet {
            private readonly Dictionary<Guid, CoinProfile> _dicById = new Dictionary<Guid, CoinProfile>();
            private readonly INTMinerRoot _root;
            private readonly object _locker = new object();
            private readonly Guid _workId;
            public CoinProfileSet(INTMinerRoot root, Guid workId) {
                _root = root;
                _workId = workId;
            }

            public ICoinProfile GetCoinProfile(Guid coinId) {
                if (_dicById.ContainsKey(coinId)) {
                    return _dicById[coinId];
                }
                lock (_locker) {
                    if (_dicById.ContainsKey(coinId)) {
                        return _dicById[coinId];
                    }
                    CoinProfile coinProfile = CoinProfile.Create(_root, _workId, coinId);
                    _dicById.Add(coinId, coinProfile);
                    return coinProfile;
                }
            }

            public void SetCoinProfileProperty(Guid coinId, string propertyName, object value) {
                CoinProfile coinProfile = (CoinProfile)GetCoinProfile(coinId);
                coinProfile.SetValue(propertyName, value);
            }

            private class CoinProfile : ICoinProfile {
                public static readonly CoinProfile Empty = new CoinProfile(NTMinerRoot.Current, Guid.Empty);

                public static CoinProfile Create(INTMinerRoot root, Guid workId, Guid coinId) {
                    if (root.CoinSet.TryGetCoin(coinId, out ICoin coin)) {
                        CoinProfile coinProfile = new CoinProfile(root, workId, coin);

                        return coinProfile;
                    }
                    else {
                        return Empty;
                    }
                }

                private readonly INTMinerRoot _root;
                private readonly Guid _workId;
                private CoinProfileData _data;
                private CoinProfile(INTMinerRoot root, Guid workId) {
                    _root = root;
                    _workId = workId;
                }

                private CoinProfileData GetCoinProfileData(Guid coinId) {
                    if (_workId != Guid.Empty) {
                        return Server.ProfileService.GetCoinProfile(_workId, coinId);
                    }
                    else {
                        IRepository<CoinProfileData> repository = NTMinerRoot.CreateLocalRepository<CoinProfileData>();
                        var result = repository.GetByKey(coinId);
                        if (result == null) {
                            result = CoinProfileData.CreateDefaultData(coinId);
                        }
                        return result;
                    }
                }

                private CoinProfile(INTMinerRoot root, Guid workId, ICoin coin) {
                    _root = root;
                    _workId = workId;
                    _data = GetCoinProfileData(coin.GetId());
                    if (_data == null) {
                        throw new ValidationException("未获取到CoinProfileData数据，请重试");
                    }
                }

                public Guid CoinId {
                    get => _data.CoinId;
                    private set {
                        if (_data.CoinId != value) {
                            _data.CoinId = value;
                        }
                    }
                }

                public Guid PoolId {
                    get => _data.PoolId;
                    private set {
                        if (_data.PoolId != value) {
                            _data.PoolId = value;
                        }
                    }
                }

                public string Wallet {
                    get => _data.Wallet;
                    set {
                        if (_data.Wallet != value) {
                            _data.Wallet = value;
                        }
                    }
                }

                public bool IsHideWallet {
                    get => _data.IsHideWallet;
                    private set {
                        if (_data.IsHideWallet != value) {
                            _data.IsHideWallet = value;
                        }
                    }
                }

                public Guid CoinKernelId {
                    get => _data.CoinKernelId;
                    private set {
                        if (_data.CoinKernelId != value) {
                            _data.CoinKernelId = value;
                        }
                    }
                }
                public Guid DualCoinPoolId {
                    get => _data.DualCoinPoolId;
                    private set {
                        if (_data.DualCoinPoolId != value) {
                            _data.DualCoinPoolId = value;
                        }
                    }
                }

                public string DualCoinWallet {
                    get => _data.DualCoinWallet;
                    set {
                        if (_data.DualCoinWallet != value) {
                            _data.DualCoinWallet = value;
                        }
                    }
                }

                public bool IsDualCoinHideWallet {
                    get => _data.IsDualCoinHideWallet;
                    private set {
                        if (_data.IsDualCoinHideWallet != value) {
                            _data.IsDualCoinHideWallet = value;
                        }
                    }
                }

                private static Dictionary<string, PropertyInfo> _properties;

                private static Dictionary<string, PropertyInfo> Properties {
                    get {
                        if (_properties == null) {
                            _properties = new Dictionary<string, PropertyInfo>();
                            foreach (var item in typeof(CoinProfile).GetProperties()) {
                                _properties.Add(item.Name, item);
                            }
                        }
                        return _properties;
                    }
                }

                public void SetValue(string propertyName, object value) {
                    if (Properties.TryGetValue(propertyName, out PropertyInfo propertyInfo)) {
                        if (propertyInfo.CanWrite) {
                            if (propertyInfo.PropertyType == typeof(Guid)) {
                                value = DictionaryExtensions.ConvertToGuid(value);
                            }
                            propertyInfo.SetValue(this, value, null);
                            if (_workId != Guid.Empty) {
                                if (CommandLineArgs.IsControlCenter) {
                                    Server.ControlCenterService.SetCoinProfilePropertyAsync(_workId, CoinId, propertyName, value, isSuccess => {
                                        VirtualRoot.Happened(new CoinProfilePropertyChangedEvent(this.CoinId, propertyName));
                                    });
                                }
                            }
                            else {
                                IRepository<CoinProfileData> repository = NTMinerRoot.CreateLocalRepository<CoinProfileData>();
                                repository.Update(_data);
                                VirtualRoot.Happened(new CoinProfilePropertyChangedEvent(this.CoinId, propertyName));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region PoolProfileSet
        public class PoolProfileSet {
            private readonly Dictionary<Guid, PoolProfile> _dicById = new Dictionary<Guid, PoolProfile>();
            private readonly INTMinerRoot _root;
            private readonly object _locker = new object();

            private readonly Guid _workId;
            public PoolProfileSet(INTMinerRoot root, Guid workId) {
                _root = root;
                _workId = workId;
            }

            public IPoolProfile GetPoolProfile(Guid poolId) {
                if (_dicById.ContainsKey(poolId)) {
                    return _dicById[poolId];
                }
                lock (_locker) {
                    if (_dicById.ContainsKey(poolId)) {
                        return _dicById[poolId];
                    }
                    PoolProfile coinProfile = PoolProfile.Create(_root, _workId, poolId);
                    _dicById.Add(poolId, coinProfile);
                    return coinProfile;
                }
            }

            public void SetPoolProfileProperty(Guid poolId, string propertyName, object value) {
                PoolProfile coinProfile = (PoolProfile)GetPoolProfile(poolId);
                coinProfile.SetValue(propertyName, value);
            }

            public class PoolProfile : IPoolProfile {
                public static readonly PoolProfile Empty = new PoolProfile(NTMinerRoot.Current, Guid.Empty);

                private readonly Guid _workId;
                public static PoolProfile Create(INTMinerRoot root, Guid workId, Guid poolIdId) {
                    if (root.PoolSet.TryGetPool(poolIdId, out IPool pool)) {
                        PoolProfile coinProfile = new PoolProfile(root, workId, pool);

                        return coinProfile;
                    }
                    else {
                        return Empty;
                    }
                }

                private readonly INTMinerRoot _root;
                private PoolProfileData _data;
                private PoolProfile(INTMinerRoot root, Guid workId) {
                    _root = root;
                    _workId = workId;
                }

                private PoolProfileData GetPoolProfileData(Guid poolId) {
                    if (_workId != Guid.Empty) {
                        return Server.ProfileService.GetPoolProfile(_workId, poolId);
                    }
                    else {
                        IRepository<PoolProfileData> repository = NTMinerRoot.CreateLocalRepository<PoolProfileData>();
                        var result = repository.GetByKey(poolId);
                        if (result == null) {
                            // 如果本地未设置用户名密码则使用默认的测试用户名密码
                            result = PoolProfileData.CreateDefaultData(poolId);
                            if (_root.PoolSet.TryGetPool(poolId, out IPool pool)) {
                                result.UserName = pool.UserName;
                                result.Password = pool.Password;
                            }
                        }
                        return result;
                    }
                }

                private PoolProfile(INTMinerRoot root, Guid workId, IPool pool) {
                    _root = root;
                    _workId = workId;
                    _data = GetPoolProfileData(pool.GetId());
                    if (_data == null) {
                        throw new ValidationException("未获取到PoolProfileData数据，请重试");
                    }
                }

                public Guid PoolId {
                    get => _data.PoolId;
                    private set {
                        if (_data.PoolId != value) {
                            _data.PoolId = value;
                        }
                    }
                }

                public string UserName {
                    get => _data.UserName;
                    private set {
                        if (_data.UserName != value) {
                            _data.UserName = value;
                        }
                    }
                }

                public string Password {
                    get => _data.Password;
                    private set {
                        if (_data.Password != value) {
                            _data.Password = value;
                        }
                    }
                }

                private static Dictionary<string, PropertyInfo> _properties;

                private static Dictionary<string, PropertyInfo> Properties {
                    get {
                        if (_properties == null) {
                            _properties = new Dictionary<string, PropertyInfo>();
                            foreach (var item in typeof(PoolProfile).GetProperties()) {
                                _properties.Add(item.Name, item);
                            }
                        }
                        return _properties;
                    }
                }

                public void SetValue(string propertyName, object value) {
                    if (Properties.TryGetValue(propertyName, out PropertyInfo propertyInfo)) {
                        if (propertyInfo.CanWrite) {
                            if (propertyInfo.PropertyType == typeof(Guid)) {
                                value = DictionaryExtensions.ConvertToGuid(value);
                            }
                            propertyInfo.SetValue(this, value, null);
                            if (_workId != Guid.Empty) {
                                if (CommandLineArgs.IsControlCenter) {
                                    Server.ControlCenterService.SetPoolProfilePropertyAsync(_workId, PoolId, propertyName, value, isSuccess => {
                                        VirtualRoot.Happened(new PoolProfilePropertyChangedEvent(this.PoolId, propertyName));
                                    });
                                }
                            }
                            else {
                                IRepository<PoolProfileData> repository = NTMinerRoot.CreateLocalRepository<PoolProfileData>();
                                repository.Update(_data);
                                VirtualRoot.Happened(new PoolProfilePropertyChangedEvent(this.PoolId, propertyName));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region WalletSet
        internal class WalletSet {
            private readonly INTMinerRoot _root;
            private readonly Dictionary<Guid, WalletData> _dicById = new Dictionary<Guid, WalletData>();

            private bool UseRemoteWalletList {
                get {
                    return _workId != Guid.Empty || CommandLineArgs.IsControlCenter;
                }
            }

            private void AddWallet(WalletData entity) {
                if (UseRemoteWalletList) {
                    Server.ControlCenterService.AddOrUpdateWalletAsync(entity, null);
                }
                else {
                    var repository = NTMinerRoot.CreateLocalRepository<WalletData>();
                    repository.Add(entity);
                }
            }

            private void UpdateWallet(WalletData entity) {
                if (UseRemoteWalletList) {
                    Server.ControlCenterService.AddOrUpdateWalletAsync(entity, null);
                }
                else {
                    var repository = NTMinerRoot.CreateLocalRepository<WalletData>();
                    repository.Update(entity);
                }
            }

            private void RemoveWallet(Guid id) {
                if (UseRemoteWalletList) {
                    Server.ControlCenterService.RemoveWalletAsync(id, null);
                }
                else {
                    var repository = NTMinerRoot.CreateLocalRepository<WalletData>();
                    repository.Remove(id);
                }
            }

            private readonly Guid _workId;
            public WalletSet(INTMinerRoot root, Guid workId) {
                _root = root;
                _workId = workId;
                VirtualRoot.Access<AddWalletCommand>(
                    Guid.Parse("d050de9d-7356-471b-b9c7-19d685aa770a"),
                    "添加钱包",
                    LogEnum.Console,
                    action: message => {
                        InitOnece();
                        if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                            throw new ArgumentNullException();
                        }
                        if (!_root.CoinSet.Contains(message.Input.CoinId)) {
                            throw new ValidationException("there is not coin with id " + message.Input.CoinId);
                        }
                        if (string.IsNullOrEmpty(message.Input.Address)) {
                            throw new ValidationException("wallet code and Address can't be null or empty");
                        }
                        if (_dicById.ContainsKey(message.Input.GetId())) {
                            return;
                        }
                        WalletData entity = new WalletData().Update(message.Input);
                        _dicById.Add(entity.Id, entity);
                        AddWallet(entity);

                        VirtualRoot.Happened(new WalletAddedEvent(entity));
                    });
                VirtualRoot.Access<UpdateWalletCommand>(
                    Guid.Parse("658f0e61-8c86-493f-a147-d66da2ed194d"),
                    "更新钱包",
                    LogEnum.Console,
                    action: message => {
                        InitOnece();
                        if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                            throw new ArgumentNullException();
                        }
                        if (!_root.CoinSet.Contains(message.Input.CoinId)) {
                            throw new ValidationException("there is not coin with id " + message.Input.CoinId);
                        }
                        if (string.IsNullOrEmpty(message.Input.Address)) {
                            throw new ValidationException("wallet Address can't be null or empty");
                        }
                        if (string.IsNullOrEmpty(message.Input.Name)) {
                            throw new ValidationException("wallet name can't be null or empty");
                        }
                        if (!_dicById.ContainsKey(message.Input.GetId())) {
                            return;
                        }
                        WalletData entity = _dicById[message.Input.GetId()];
                        entity.Update(message.Input);
                        UpdateWallet(entity);

                        VirtualRoot.Happened(new WalletUpdatedEvent(entity));
                    });
                VirtualRoot.Access<RemoveWalletCommand>(
                    Guid.Parse("bd70fe34-7575-43d0-a8e5-d8e9566d8d56"),
                    "移除钱包",
                    LogEnum.Console,
                    action: (message) => {
                        InitOnece();
                        if (message == null || message.EntityId == Guid.Empty) {
                            throw new ArgumentNullException();
                        }
                        if (!_dicById.ContainsKey(message.EntityId)) {
                            return;
                        }
                        WalletData entity = _dicById[message.EntityId];
                        _dicById.Remove(entity.GetId());
                        RemoveWallet(entity.Id);

                        VirtualRoot.Happened(new WalletRemovedEvent(entity));
                    });
            }

            private bool _isInited = false;
            private object _locker = new object();

            public int WalletCount {
                get {
                    InitOnece();
                    return _dicById.Count;
                }
            }

            private void InitOnece() {
                if (_isInited) {
                    return;
                }
                Init();
            }

            private void Init() {
                if (!_isInited) {
                    if (UseRemoteWalletList) {
                        lock (_locker) {
                            if (!_isInited) {
                                var response = Server.ControlCenterService.GetWallets();
                                if (response != null) {
                                    foreach (var item in response.Data) {
                                        if (!_dicById.ContainsKey(item.Id)) {
                                            _dicById.Add(item.Id, item);
                                        }
                                    }
                                }
                                _isInited = true;
                            }
                        }
                    }
                    else {
                        var repository = NTMinerRoot.CreateLocalRepository<WalletData>();
                        lock (_locker) {
                            if (!_isInited) {
                                foreach (var item in repository.GetAll()) {
                                    if (!_dicById.ContainsKey(item.Id)) {
                                        _dicById.Add(item.Id, item);
                                    }
                                }
                                _isInited = true;
                            }
                        }
                    }
                }
            }

            public bool ContainsWallet(Guid walletId) {
                InitOnece();
                return _dicById.ContainsKey(walletId);
            }

            public bool TryGetWallet(Guid walletId, out IWallet wallet) {
                InitOnece();
                WalletData wlt;
                bool r = _dicById.TryGetValue(walletId, out wlt);
                wallet = wlt;
                return r;
            }

            public IEnumerable<IWallet> GetAllWallets() {
                InitOnece();
                return _dicById.Values;
            }
        }
        #endregion
    }
}
