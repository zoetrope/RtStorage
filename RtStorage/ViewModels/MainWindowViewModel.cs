using System.Windows;
using Livet;
using Livet.Behaviors.Messaging;
using Livet.Commands;
using Livet.Messaging;
using RtStorage.Models;

namespace RtStorage.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {

        private NamingServiceManager _manager;
        public MainWindowViewModel()
        {
            _manager = NamingServiceManager.Default;

            RecordersViewModel = new RecordersViewModel(_manager);
            PlayersViewModel = new PlayersViewModel(_manager);
            SettingViewModel = new SettingViewModel(_manager);
            AnalysisViewModel = new AnalysisViewModel(_manager);
        }
        
        public RecordersViewModel RecordersViewModel
        {
            get;
            private set;
        }

        public PlayersViewModel PlayersViewModel
        {
            get;
            private set;
        }
        public AnalysisViewModel AnalysisViewModel
        {
            get;
            private set;
        }
        public SettingViewModel SettingViewModel
        {
            get;
            private set;
        }


        #region CheckAllStopCommand
        ViewModelCommand _CheckAllStopCommand;

        public ViewModelCommand CheckAllStopCommand
        {
            get
            {
                if (_CheckAllStopCommand == null)
                    _CheckAllStopCommand = new ViewModelCommand(CheckAllStop);
                return _CheckAllStopCommand;
            }
        }

        private void CheckAllStop()
        {
            if (RecordersViewModel.HasRecordingRecorder() || PlayersViewModel.HasPlayingPlayers())
            {
                Messenger.Raise(new InteractionMessage("AllStop"));
            }
        }
        #endregion


        #region AllStopCommand
        ListenerCommand<ConfirmationMessage> _AllStopCommand;

        public ListenerCommand<ConfirmationMessage> AllStopCommand
        {
            get
            {
                if (_AllStopCommand == null)
                    _AllStopCommand = new ListenerCommand<ConfirmationMessage>(AllStop);
                return _AllStopCommand;
            }
        }

        private void AllStop(ConfirmationMessage message)
        {
            if (message.Response.HasValue && message.Response.Value)
            {
                RecordersViewModel.StopAllRecorders();
                PlayersViewModel.StopAllPlayers();
            }
        }
        #endregion
      

    }
}
