using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Microsoft.Win32;
using RtStorage.Models;
using RtStorage.Properties;
using RtStorage.Views.Messages;

namespace RtStorage.ViewModels
{
    public class SettingViewModel : ViewModel
    {
        private NamingServiceManager _manager;

        public SettingViewModel(NamingServiceManager manager)
        {
            _manager = manager;
            NamingServices = ViewModelHelper.CreateReadOnlyNotificationDispatcherCollection(
                _manager.NamingServices,
                ns => new NamingServiceViewModel(ns),
                DispatcherHelper.UIDispatcher);

            DataDirectory = Environment.ExpandEnvironmentVariables(Settings.Default.DataDirectory);
        }

        public ReadOnlyNotificationDispatcherCollection<NamingServiceViewModel> NamingServices { get; private set; }


        #region AddNamingServiceCommand
        ViewModelCommand _AddNamingServiceCommand;

        public ViewModelCommand AddNamingServiceCommand
        {
            get
            {
                if (_AddNamingServiceCommand == null)
                    _AddNamingServiceCommand = new ViewModelCommand(AddNamingService);
                return _AddNamingServiceCommand;
            }
        }

        private void AddNamingService()
        {
            var vm = new AddNamingServiceWindowViewModel(_manager);
            Messenger.Raise(new TransitionMessage(vm, "OpenAddNamingServiceWindow"));
        }
        #endregion


        #region DataDirectory変更通知プロパティ
        string _DataDirectory;

        public string DataDirectory
        {
            get
            { return _DataDirectory; }
            set
            {
                if (_DataDirectory == value)
                    return;
                _DataDirectory = value;
                RaisePropertyChanged("DataDirectory");
                SaveSettingCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region SelectDataDirectoryCommand
        ListenerCommand<DirectorySelectionMessage> _SelectDataDirectoryCommand;

        public ListenerCommand<DirectorySelectionMessage> SelectDataDirectoryCommand
        {
            get
            {
                if (_SelectDataDirectoryCommand == null)
                    _SelectDataDirectoryCommand = new ListenerCommand<DirectorySelectionMessage>(SelectDataDirectory);
                return _SelectDataDirectoryCommand;
            }
        }

        private void SelectDataDirectory(DirectorySelectionMessage message)
        {
            if (!string.IsNullOrEmpty(message.Response))
            {
                DataDirectory = message.Response;
            }
        }
        #endregion


        #region SaveSettingCommand
        ViewModelCommand _SaveSettingCommand;

        public ViewModelCommand SaveSettingCommand
        {
            get
            {
                if (_SaveSettingCommand == null)
                    _SaveSettingCommand = new ViewModelCommand(SaveSetting, CanSaveSetting);
                return _SaveSettingCommand;
            }
        }

        private bool CanSaveSetting()
        {
            return !string.IsNullOrEmpty(DataDirectory) && Settings.Default.DataDirectory != DataDirectory;
        }

        private void SaveSetting()
        {
            Settings.Default.DataDirectory = DataDirectory;
            Settings.Default.Save();
            Messenger.Raise(new InformationMessage("設定を保存しました。この設定は次回起動時に反映されます。", "情報", "ShowInfo"));
        }

        #endregion
      
    }
}
