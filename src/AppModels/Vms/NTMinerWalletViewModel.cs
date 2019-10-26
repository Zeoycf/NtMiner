﻿using NTMiner.Core;
using NTMiner.MinerServer;
using System;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class NTMinerWalletViewModel : ViewModelBase, INTMinerWallet, IEditableViewModel {
        private Guid _id;
        private Guid _coinId;
        private string _wallet;

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand Save { get; private set; }

        public Action CloseWindow { get; set; }

        public NTMinerWalletViewModel() {
            if (!Design.IsInDesignMode) {
                throw new InvalidProgramException();
            }
        }

        public NTMinerWalletViewModel(Guid id) {
            _id = id;
            this.Save = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                if (NTMinerRoot.Instance.NTMinerWalletSet.TryGetNTMinerWallet(Id, out INTMinerWallet _)) {
                    VirtualRoot.Execute(new UpdateNTMinerWalletCommand(this));
                }
                else {
                    VirtualRoot.Execute(new AddNTMinerWalletCommand(this));
                }
                CloseWindow?.Invoke();
            });
            this.Edit = new DelegateCommand<FormType?>((formType) => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                VirtualRoot.Execute(new NTMinerWalletEditCommand(formType ?? FormType.Edit, this));
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                this.ShowDialog(new DialogWindowViewModel(message: $"确定删除吗？", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new RemoveNTMinerWalletCommand(this.Id));
                }));
            });
        }

        public NTMinerWalletViewModel(INTMinerWallet data) : this(data.GetId()) {
            _coinId = data.CoinId;
            _wallet = data.Wallet;
        }

        public Guid GetId() {
            return Id;
        }

        public Guid Id {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public Guid CoinId {
            get => _coinId;
            set {
                _coinId = value;
                OnPropertyChanged(nameof(CoinId));
            }
        }

        public string Wallet {
            get => _wallet;
            set {
                _wallet = value;
                OnPropertyChanged(nameof(Wallet));
            }
        }
    }
}