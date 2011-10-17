using System;
using System.Collections.Generic;
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
    public class PlayerControlViewModel : ViewModel
    {
        private PlayersViewModel _parent;
        private Player _player;

        public PlayerControlViewModel(PlayersViewModel parent, Player player)
        {
            _parent = parent;
            _player = player;

            ViewModelHelper.BindNotifyChanged(_player, this,
                (sender, e) => RaisePropertyChanged(e.PropertyName));

            ViewModelHelper.BindNotifyChanged(_player.RecordDescription, this,
                (sender, e) => RaisePropertyChanged(e.PropertyName));


            // IsPlayingの状態が変わったら、PlayCommand/StopCommandの有効無効を切り替える
            Observable.FromEventPattern<PropertyChangedEventArgs>(_player, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "IsPlaying" || e.EventArgs.PropertyName == "IsPausing")
                .Subscribe(_ => { 
                    _PlayCommand.RaiseCanExecuteChanged();
                    _PauseCommand.RaiseCanExecuteChanged();
                    _StopCommand.RaiseCanExecuteChanged(); 
                });


            Observable.FromEventPattern<PropertyChangedEventArgs>(_player, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "IsAlive")
                .Subscribe(_ => _ReinitializeCommand.RaiseCanExecuteChanged());

            Observable.FromEventPattern<ErrorInfoEventArgs>(_player, "ErrorRaised")
                .Subscribe(e => Messenger.Raise(new InformationMessage(e.EventArgs.Message, "エラー", "ShowError")));

        }

        #region PlayCommand
        ViewModelCommand _PlayCommand;

        public ViewModelCommand PlayCommand
        {
            get
            {
                if (_PlayCommand == null)
                    _PlayCommand = new ViewModelCommand(Play, CanPlay);
                return _PlayCommand;
            }
        }

        private bool CanPlay()
        {
            return !_player.IsPlaying || _player.IsPausing;
        }

        private void Play()
        {
            _player.Play();
            _PlayCommand.RaiseCanExecuteChanged();
            _StopCommand.RaiseCanExecuteChanged();
        }
        #endregion


        #region PauseCommand
        ViewModelCommand _PauseCommand;

        public ViewModelCommand PauseCommand
        {
            get
            {
                if (_PauseCommand == null)
                    _PauseCommand = new ViewModelCommand(Pause, CanPause);
                return _PauseCommand;
            }
        }

        private bool CanPause()
        {
            return _player.IsPlaying && !_player.IsPausing;
        }

        private void Pause()
        {
            _player.Pause();
        }
        #endregion
      

        #region StopCommand
        ViewModelCommand _StopCommand;

        public ViewModelCommand StopCommand
        {
            get
            {
                if (_StopCommand == null)
                    _StopCommand = new ViewModelCommand(Stop, CanStop);
                return _StopCommand;
            }
        }

        private bool CanStop()
        {
            return _player.IsPlaying;
        }

        private void Stop()
        {
            _player.Stop();
            _PlayCommand.RaiseCanExecuteChanged();
            _StopCommand.RaiseCanExecuteChanged();
        }
        #endregion


        #region SkipFirstCommand
        ViewModelCommand _SkipFirstCommand;

        public ViewModelCommand SkipFirstCommand
        {
            get
            {
                if (_SkipFirstCommand == null)
                    _SkipFirstCommand = new ViewModelCommand(SkipFirst);
                return _SkipFirstCommand;
            }
        }

        private void SkipFirst()
        {
            bool isPausing = _player.IsPlaying;

            if (isPausing) _player.Pause();
            
            CurrentCount = 0;
            _player.SetPosition();
            
            if (isPausing) _player.Resume();
        }
        #endregion


        #region ReinitializeCommand
        ViewModelCommand _ReinitializeCommand;

        public ViewModelCommand ReinitializeCommand
        {
            get
            {
                if (_ReinitializeCommand == null)
                    _ReinitializeCommand = new ViewModelCommand(Reinitialize, CanReinitialize);
                return _ReinitializeCommand;
            }
        }

        private bool CanReinitialize()
        {
            return !_player.IsAlive;
        }

        private void Reinitialize()
        {
            _player.Initialize();
        }
        #endregion
      

        #region CloseCommand
        ViewModelCommand _CloseCommand;

        public ViewModelCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                    _CloseCommand = new ViewModelCommand(Close);
                return _CloseCommand;
            }
        }

        private void Close()
        {
            //TODO:確認してから閉じる
            _parent.RemovePlayer(_player);
        }
        #endregion


        #region Playerのプロパティのラップ
        public long CurrentCount
        {
            get { return _player.CurrentCount; }
            set { _player.CurrentCount = value; }
        }

        public bool AutoActivate
        {
            get { return _player.AutoActivate; }
            set { _player.AutoActivate = value; }
        }

        public bool EnableLoop
        {
            get { return _player.EnableLoop; }
            set { _player.EnableLoop = value; }
        }

        public bool IsAlive
        {
            get { return _player.IsAlive; }
        }

        #endregion


        #region RecordDescriptionのプロパティのラップ
        public string ComponentType
        {
            get { return _player.RecordDescription.ComponentType; }
        }

        public long Count
        {
            get { return _player.RecordDescription.Count; }
        }

        public DateTime CreatedDateTime
        {
            get { return _player.RecordDescription.CreatedDateTime; }
        }

        public string DataType
        {
            get { return _player.RecordDescription.DataType; }
        }

        public string NamingName
        {
            get { return _player.RecordDescription.NamingName; }
        }

        public string PortName
        {
            get { return _player.RecordDescription.PortName; }
        }

        public long SumSize
        {
            get { return _player.RecordDescription.SumSize; }
        }

        public long TimeSpan
        {
            get { return _player.RecordDescription.TimeSpan; }
        }

        public string Title
        {
            get { return _player.RecordDescription.Title; }
        }
        #endregion


        private bool isDragging = false;
        private bool draggedDuringPlaying = false;

        #region StartDragCommand
        ViewModelCommand _StartDragCommand;

        public ViewModelCommand StartDragCommand
        {
            get
            {
                if (_StartDragCommand == null)
                    _StartDragCommand = new ViewModelCommand(StartDrag);
                return _StartDragCommand;
            }
        }

        private void StartDrag()
        {
            isDragging = true;

            if (!_player.IsPausing)
            {
                // 再生中にドラッグした場合はいったん停止。
                _player.Pause();
                // ドラッグをやめたときに再開するようにフラグを立てる。
                draggedDuringPlaying = true;
            }
        }
        #endregion


        #region CompleteDragCommand
        ViewModelCommand _CompleteDragCommand;

        public ViewModelCommand CompleteDragCommand
        {
            get
            {
                if (_CompleteDragCommand == null)
                    _CompleteDragCommand = new ViewModelCommand(CompleteDrag);
                return _CompleteDragCommand;
            }
        }

        private void CompleteDrag()
        {
            if (draggedDuringPlaying)
            {
                _player.Resume();
                draggedDuringPlaying = false;
            }
            isDragging = false;
        }
        #endregion


        #region ChangeValueCommand
        ViewModelCommand _ChangeValueCommand;

        public ViewModelCommand ChangeValueCommand
        {
            get
            {
                if (_ChangeValueCommand == null)
                    _ChangeValueCommand = new ViewModelCommand(ChangeValue);
                return _ChangeValueCommand;
            }
        }

        private void ChangeValue()
        {
            if (isDragging)
            {
                _player.SetPosition();
            }
        }
        #endregion
      
      
      

    }
}
