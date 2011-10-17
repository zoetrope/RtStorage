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
    public class RecorderControlViewModel : ViewModel
    {
        private RecordersViewModel _parent;
        private Recorder _recorder;

        public RecorderControlViewModel(RecordersViewModel parent, Recorder recorder)
        {
            _parent = parent;
            _recorder = recorder;

            ViewModelHelper.BindNotifyChanged(_recorder.RecordDescription, this,
                (sender, e) => RaisePropertyChanged(e.PropertyName));
            
            ViewModelHelper.BindNotifyChanged(_recorder, this,
                (sender, e) => RaisePropertyChanged(e.PropertyName));

            // IsPlayingの状態が変わったら、PlayCommand/StopCommandの有効無効を切り替える
            Observable.FromEventPattern<PropertyChangedEventArgs>(_recorder, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "IsRecording" || e.EventArgs.PropertyName == "IsPausing")
                .Subscribe(_ =>
                {
                    _PlayCommand.RaiseCanExecuteChanged();
                    _PauseCommand.RaiseCanExecuteChanged();
                    _StopCommand.RaiseCanExecuteChanged();
                });


            Observable.FromEventPattern<PropertyChangedEventArgs>(_recorder, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "IsAlive")
                .Subscribe(_ => _ReinitializeCommand.RaiseCanExecuteChanged());


            Observable.FromEventPattern<ErrorInfoEventArgs>(_recorder, "ErrorRaised")
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
            return !_recorder.IsRecording || _recorder.IsPausing;
        }

        private void Play()
        {
            _recorder.Play();
            _PlayCommand.RaiseCanExecuteChanged();
            _PauseCommand.RaiseCanExecuteChanged();
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
            return _recorder.IsRecording && !_recorder.IsPausing;
        }

        private void Pause()
        {
            _recorder.Pause();
            _PlayCommand.RaiseCanExecuteChanged();
            _PauseCommand.RaiseCanExecuteChanged();
            _StopCommand.RaiseCanExecuteChanged();
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
            return _recorder.IsRecording;
        }

        private void Stop()
        {
            _recorder.Stop();
            _PlayCommand.RaiseCanExecuteChanged();
            _PauseCommand.RaiseCanExecuteChanged();
            _StopCommand.RaiseCanExecuteChanged();

            if (true) //TODO: 成功した場合のみ。
            {
                if (_recorder.RecordDescription.Count > 0)
                {
                    var rep = RepositoryFactory.CreateRepository();
                    rep.Insert(_recorder.RecordDescription);
                }
            }
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
            return !_recorder.IsAlive;
        }

        private void Reinitialize()
        {
            _recorder.Initialize();
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
            _parent.RemoveRecorder(_recorder);

        }
        #endregion

        #region Recorderのプロパティのラップ
        public bool AutoActivate
        {
            get { return _recorder.AutoActivate; }
            set { _recorder.AutoActivate = value; }
        }

        public bool IsAlive
        {
            get { return _recorder.IsAlive; }
        }

        #endregion


        #region RecordDescriptionのプロパティのラップ
        public string ComponentType
        {
            get { return _recorder.RecordDescription.ComponentType; }
        }

        public long Count
        {
            get { return _recorder.RecordDescription.Count; }
        }

        public DateTime CreatedDateTime
        {
            get { return _recorder.RecordDescription.CreatedDateTime; }
        }

        public string DataType
        {
            get { return _recorder.RecordDescription.DataType; }
        }

        public string NamingName
        {
            get { return _recorder.RecordDescription.NamingName; }
        }

        public string PortName
        {
            get { return _recorder.RecordDescription.PortName; }
        }

        public long SumSize
        {
            get { return _recorder.RecordDescription.SumSize; }
        }

        public long TimeSpan
        {
            get { return _recorder.RecordDescription.TimeSpan; }
        }

        public string Title
        {
            get { return _recorder.RecordDescription.Title; }
        }
        #endregion

    }
}
