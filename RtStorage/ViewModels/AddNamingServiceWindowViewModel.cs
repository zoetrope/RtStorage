using System;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using Codeplex.Reactive;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.Windows;
using RtStorage.Models;

namespace RtStorage.ViewModels
{
    public class AddNamingServiceWindowViewModel : ViewModel
    {
        [Required]
        [StringLength(256)]
        public ReactiveProperty<string> HostName { get; private set; }

        [Range(1024, 65535)]
        public ReactiveProperty<string> PortNumber { get; private set; }

        public ReactiveCommand AddNamingServiceCommand { get; private set; }
        
        private NamingServiceManager _manager;
        public AddNamingServiceWindowViewModel(NamingServiceManager manager)
        {
            _manager = manager;

            HostName = new ReactiveProperty<string>()
                .SetValidateAttribute(() => HostName);

            PortNumber = new ReactiveProperty<string>()
                .SetValidateAttribute(() => PortNumber);

            var errors = Observable.Merge(
                HostName.ObserveErrorChanged,
                PortNumber.ObserveErrorChanged);

            AddNamingServiceCommand = errors.Select(x => x == null).ToReactiveCommand(initialValue: false);

            // ちょっと固まるので非同期にしたい。
            AddNamingServiceCommand.Subscribe(_ => AddNamingService());
        }

        private void AddNamingService()
        {
            // ポート番号が入力されていない場合はデフォルトで2809
            int port;
            if(!int.TryParse(PortNumber.Value,out port))
            {
                port = 2809;
            }

            bool success = _manager.AddNamingService(HostName.Value, port);

            if(success)
            {
                _manager.UpdateAsync();
                // 追加に成功した場合はウインドウを閉じる
                Messenger.Raise(new WindowActionMessage("CloseWindow", WindowAction.Close));
            }
            else
            {
                 //すでに追加されている場合はエラーメッセージを表示する
                Messenger.Raise(new InformationMessage(
                    HostName.Value + ":" + port + "はすでに登録されています。",
                    "ネーミングサービス追加エラー",
                    "AlreadyRegister"));
            }
            
        }

        #region CancelCommand
        private ViewModelCommand _cancelCommand;

        public ViewModelCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new ViewModelCommand(Cancel);
                return _cancelCommand;
            }
        }

        private void Cancel()
        {
            Messenger.Raise(new WindowActionMessage("CloseWindow", WindowAction.Close)); ;
        }
        #endregion
      

    }
}
