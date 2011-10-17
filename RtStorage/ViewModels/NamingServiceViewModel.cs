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

using RtStorage.Models;

namespace RtStorage.ViewModels
{
    public class NamingServiceViewModel : ViewModel
    {
        private NamingService _namingService;

        public NamingServiceViewModel(NamingService namingService)
        {
            _namingService = namingService;

            ViewModelHelper.BindNotifyChanged(
                _namingService,this,(sender, e) => RaisePropertyChanged(e.PropertyName));

        }

        
        #region NamingServiceのプロパティのラップ
        public string HostName
        {
            get
            { return _namingService.HostName; }
            set
            {
                _namingService.HostName = value;
            }
        }

        public int PortNumber
        {
            get
            { return _namingService.PortNumber; }
            set
            {
                _namingService.PortNumber = value;
            }
        }
        #endregion



        #region RemoveNamingServiceCommand
        ViewModelCommand _RemoveNamingServiceCommand;

        public ViewModelCommand RemoveNamingServiceCommand
        {
            get
            {
                if (_RemoveNamingServiceCommand == null)
                    _RemoveNamingServiceCommand = new ViewModelCommand(RemoveNamingService);
                return _RemoveNamingServiceCommand;
            }
        }

        private void RemoveNamingService()
        {
            NamingServiceManager.Default.RemoveNamingService(_namingService.Client.Key);
            NamingServiceManager.Default.UpdateAsync();
        }
        #endregion
    }
}
