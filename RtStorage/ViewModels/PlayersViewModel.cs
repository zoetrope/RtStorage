using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;

using RtStorage.Models;

namespace RtStorage.ViewModels
{
    public class PlayersViewModel : ViewModel
    {
        private NamingServiceManager _manager;

        public PlayersViewModel(NamingServiceManager manager)
        {
            _manager = manager;

            _players = new ObservableCollection<Player>();


            PlayerViewModels = ViewModelHelper.CreateReadOnlyDispatcherCollection<PlayerControlViewModel,Player>(
                _players,
                player => new PlayerControlViewModel(this,player),
                DispatcherHelper.UIDispatcher);


            NamingServiceTree = ViewModelHelper.CreateReadOnlyDispatcherCollection<TreeViewItemViewModel,TreeViewItemViewModel>(
                _manager.InPortTree,
                item => item,
                DispatcherHelper.UIDispatcher);


            // NamingServiceManagerのIsUpdatingの状態が変わったら、UpdateTreeCommandの有効無効を切り替える
            Observable.FromEventPattern<PropertyChangedEventArgs>(_manager, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "IsUpdating")
                .Subscribe(_ => UpdateTreeCommand.RaiseCanExecuteChanged());
        }

        private ObservableCollection<Player> _players;

        public ReadOnlyDispatcherCollection<PlayerControlViewModel> PlayerViewModels
        {
            get;
            private set;
        }

        public ReadOnlyDispatcherCollection<TreeViewItemViewModel> NamingServiceTree
        {
            get;
            private set;
        }

        #region UpdateTreeCommand
        ViewModelCommand _UpdateTreeCommand;

        public ViewModelCommand UpdateTreeCommand
        {
            get
            {
                if (_UpdateTreeCommand == null)
                    _UpdateTreeCommand = new ViewModelCommand(UpdateTree, CanUpdateTreee);
                return _UpdateTreeCommand;
            }
        }

        private bool CanUpdateTreee()
        {
            return !_manager.IsUpdating;
        }

        private void UpdateTree()
        {
            Observable.Start(() => _manager.UpdateAsync());
        }
        #endregion
      

        #region UpdateItemCommand
        ViewModelCommand _UpdateItemCommand;

        public ViewModelCommand UpdateItemCommand
        {
            get
            {
                if (_UpdateItemCommand == null)
                    _UpdateItemCommand = new ViewModelCommand(UpdateItem);
                return _UpdateItemCommand;
            }
        }

        private void UpdateItem()
        {
            // 選択されているアイテムを探す
            var selectedItems = NamingServiceTree.Cast<TreeViewItemViewModel>()
                .Expand(x => x.Children)
                .Where(vm => vm.IsSelected);

            if (selectedItems.Count() != 1)
            {
                return;
            }

            var selectedItem = selectedItems.First();

            // ポートが選択された場合のみ更新
            if (selectedItem is InPortItemViewModel)
            {
                SelectedPort = (InPortItemViewModel)selectedItem;

                _AddPlayerCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region SelectedPort変更通知プロパティ
        private InPortItemViewModel _SelectedPort;

        public InPortItemViewModel SelectedPort
        {
            get
            { return _SelectedPort; }
            set
            {
                if (_SelectedPort == value)
                    return;
                _SelectedPort = value;
                RaisePropertyChanged("SelectedPort");
            }
        }
        #endregion

        public void NotifyAdopt(RecordDescription description)
        {
            _players.Add(new Player(description, SelectedPort.NamingName, SelectedPort.Name));
        }



        #region AddPlayerCommand
        private ViewModelCommand _AddPlayerCommand;

        public ViewModelCommand AddPlayerCommand
        {
            get
            {
                if (_AddPlayerCommand == null)
                    _AddPlayerCommand = new ViewModelCommand(AddPlayer, CanAddPlayer);
                return _AddPlayerCommand;
            }
        }

        private void AddPlayer()
        {
            // 選択されたポートのデータ型と一致するレコードを検索する画面を開く
            // 選択が完了したらNotifyAdoptが呼ばれる
            var vm = new SearchRecordWindowViewModel(NotifyAdopt, dataType: SelectedPort.DataType);
            Messenger.Raise(new TransitionMessage(vm, "OpenSearchRecordWindow"));
        }


        private bool CanAddPlayer()
        {
            if (SelectedPort == null) return false;

            var key = SelectedPort.NamingName + ":" + SelectedPort.Name;

            if (_players.Any(r => r.Key == key))
            {
                // 既に登録済み
                return false;
            }

            return true;
        }
        #endregion

        public void RemovePlayer(Player player)
        {
            _players.Remove(player);
        }

        public bool HasPlayingPlayers()
        {
            return _players.Any(x => x.IsPlaying);
        }

        public void StopAllPlayers()
        {
            _players.Where(x => x.IsPlaying).ForEach(x => x.Stop());
        }
    }
}
