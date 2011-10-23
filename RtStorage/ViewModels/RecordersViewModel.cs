using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;

using RtStorage.Models;

namespace RtStorage.ViewModels
{
    public class RecordersViewModel : ViewModel
    {
        private NamingServiceManager _manager;

        public RecordersViewModel(NamingServiceManager manager)
        {
            _manager = manager;
            _recorders = new ObservableCollection<Recorder>();

            RecorderViewModels = ViewModelHelper.CreateReadOnlyDispatcherCollection<RecorderControlViewModel, Recorder>(
                _recorders,
                recorder => new RecorderControlViewModel(this,recorder), 
                DispatcherHelper.UIDispatcher);

            NamingServiceTree = ViewModelHelper.CreateReadOnlyDispatcherCollection<TreeViewItemViewModel,TreeViewItemViewModel>(
                _manager.OutPortTree,
                item => item,
                DispatcherHelper.UIDispatcher);

            // NamingServiceManagerのIsUpdatingの状態が変わったら、UpdateTreeCommandの有効無効を切り替える
            Observable.FromEventPattern<PropertyChangedEventArgs>(_manager, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "IsUpdating")
                .Subscribe(_ => UpdateTreeCommand.RaiseCanExecuteChanged());
        }

        private ObservableCollection<Recorder> _recorders;

        public ReadOnlyDispatcherCollection<RecorderControlViewModel> RecorderViewModels
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
                    _UpdateTreeCommand = new ViewModelCommand(UpdateTree,CanUpdateTreee);
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
                .Where(vm=>vm.IsSelected);

            if (selectedItems.Count() != 1)
            {
                return;
            }

            var selectedItem = selectedItems.First();

            // ポートが選択された場合のみ更新
            if (selectedItem is OutPortItemViewModel)
            {
                SelectedPort = (OutPortItemViewModel)selectedItem;

                _AddRecorderCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region SelectedPort変更通知プロパティ
        private OutPortItemViewModel _SelectedPort;

        public OutPortItemViewModel SelectedPort
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


        #region AddRecorderCommand
        private ViewModelCommand _AddRecorderCommand;

        public ViewModelCommand AddRecorderCommand
        {
            get
            {
                if (_AddRecorderCommand == null)
                    _AddRecorderCommand = new ViewModelCommand(AddRecorder, CanAddRecorder);
                return _AddRecorderCommand;
            }
        }

        private void AddRecorder()
        {
            var recorder = new Recorder(SelectedPort.NamingName, SelectedPort.Name);

            _recorders.Add(recorder);
        }

        private bool CanAddRecorder()
        {
            if (SelectedPort == null) return false;

            var key = SelectedPort.NamingName + ":" + SelectedPort.Name;

            if (_recorders.Any(r => r.Key == key))
            {
                // 既に登録済み
                return false;
            }

            return true;
        }
        #endregion

        public void RemoveRecorder(Recorder recorder)
        {
            _recorders.Remove(recorder);
        }

        public bool HasRecordingRecorder()
        {
            return _recorders.Any(x => x.IsRecording);
        }

        public void StopAllRecorders()
        {
            _recorders.Where(x => x.IsRecording).ForEach(x => x.Stop());
        }
    }
}
