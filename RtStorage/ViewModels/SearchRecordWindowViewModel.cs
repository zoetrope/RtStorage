using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Codeplex.Reactive;
using Codeplex.Reactive.Notifier;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.Windows;
using RtStorage.Models;
using RtUtility;

namespace RtStorage.ViewModels
{
    public class SearchRecordWindowViewModel : ViewModel
    {
        private IRecordDescriptionRepository _recordDescriptionRepository;
        private Action<RecordDescription> _notifier;

        public ReactiveProperty<bool> IsSearching { get; private set; }
        public ReactiveCommand SearchCommand { get; private set; }


        public SearchRecordWindowViewModel(Action<RecordDescription> notifier, string dataType=null, string componentType=null, string portName=null)
        {
            _recordDescriptionRepository = RepositoryFactory.CreateRepository();

            _notifier = notifier;
            
            // 検索中は検索ボタンが押せないようにする
            var isSearching = new SignalNotifier();
            SearchCommand = isSearching.Select(x=>x == SignalChangedStatus.Empty).ToReactiveCommand();

            // 検索ボタンが押されたら検索処理を非同期で実行し、その結果をSearchResultsに入れる
            SearchResults = SearchCommand.SelectMany(_ => SearchAsync(isSearching))
                .Do(_ => SearchResults.Clear())
                .SelectMany(_ => _)
                .Select(r => new SearchResultViewModel(r, NotifyAdopt))
                .ToReactiveCollection();
            
            Initialize(dataType, componentType, portName);

            SearchCommand.Execute(null);
        }

        private void Initialize(string dataType, string componentType, string portName)
        {
            UpdateDataTypeCommand = new ReactiveCommand();
            UpdatePortNameCommand = new ReactiveCommand();
            UpdateCompoentTypeCommand = new ReactiveCommand();

            DataTypes = UpdateDataTypeCommand
                .SelectMany(_=>Observable.Start(()=>_recordDescriptionRepository.GetDataTypes(componentType,portName))
                    .Catch((Exception ex) => Messenger.Raise(new InformationMessage("データベースアクセスに失敗しました。", "エラー", "ShowError"))))
                .Do(_=>DataTypes.Clear())
                .SelectMany(_=>_)
                .ToReactiveCollection();

            PortNames = UpdatePortNameCommand
                .SelectMany(_=>Observable.Start(()=>_recordDescriptionRepository.GetPortNames(dataType,componentType))
                    .Catch((Exception ex) => Messenger.Raise(new InformationMessage("データベースアクセスに失敗しました。", "エラー", "ShowError"))))
                .Do(_=>PortNames.Clear())
                .SelectMany(_=>_)
                .ToReactiveCollection();
            ComponentTypes = UpdateCompoentTypeCommand
                .SelectMany(_=>Observable.Start(()=>_recordDescriptionRepository.GetComponentTypes(dataType,portName))
                    .Catch((Exception ex) => Messenger.Raise(new InformationMessage("データベースアクセスに失敗しました。", "エラー", "ShowError"))))
                .Do(_=>ComponentTypes.Clear())
                .SelectMany(_=>_)
                .ToReactiveCollection();

            DataType = dataType;
            PortName = portName;
            ComponentType = componentType;

            UpdateDataTypeCommand.Execute(null);
            UpdatePortNameCommand.Execute(null);
            UpdateCompoentTypeCommand.Execute(null);

        }

        public ReactiveCommand UpdateDataTypeCommand { get; private set; }
        public ReactiveCommand UpdatePortNameCommand { get; private set; }
        public ReactiveCommand UpdateCompoentTypeCommand { get; private set; }

        public ReactiveCollection<string> DataTypes { get; private set; }
        public ReactiveCollection<string> PortNames { get; private set; }
        public ReactiveCollection<string> ComponentTypes { get; private set; }

        public ReactiveCollection<SearchResultViewModel> SearchResults { get; private set; }


        public void NotifyAdopt(RecordDescription description) // 検索結果が選択されたら通知される
        {
            _notifier(description);
            Messenger.Raise(new WindowActionMessage("CloseWindow", WindowAction.Close));
        }

        /// <summary>
        /// 非同期でデータベースの検索を行う
        /// </summary>
        private IObservable<IEnumerable<RecordDescription>> SearchAsync(SignalNotifier isSearching)
        {
            isSearching.Increment(); // 検索中

            var condition = new SearchCondition()
            {
                DataType = DataType,
                ComponentType = ComponentType,
                PortName = PortName,
                StartDateTime = StartDate,
                EndDateTime = EndDate
            };

            return Observable.Start(() =>
            {
                //SearchResults.Clear();
                var ret = _recordDescriptionRepository.GetRecordDescriptions(condition);
                //isSearching.Decrement(); //検索完了
                return ret;
            })
            .Finally(() => isSearching.Decrement())
            .Catch((Exception ex) => Messenger.Raise(new InformationMessage("データベースアクセスに失敗しました。", "エラー", "ShowError")));

        }

        #region StartDate変更通知プロパティ
        DateTime? _StartDate;

        public DateTime? StartDate
        {
            get
            { return _StartDate; }
            set
            {
                if (_StartDate == value)
                    return;
                _StartDate = value;
                RaisePropertyChanged("StartDate");
            }
        }
        #endregion


        #region EndDate変更通知プロパティ
        DateTime? _EndDate;

        public DateTime? EndDate
        {
            get
            { return _EndDate; }
            set
            {
                if (_EndDate == value)
                    return;
                _EndDate = value;
                RaisePropertyChanged("EndDate");
            }
        }
        #endregion

        
        #region DataType変更通知プロパティ
        string _DataType;

        public string DataType
        {
            get
            { return _DataType; }
            set
            {
                if (_DataType == value)
                    return;
                _DataType = value;
                RaisePropertyChanged("DataType");
            }
        }
        #endregion


        #region PortName変更通知プロパティ
        string _PortName;

        public string PortName
        {
            get
            { return _PortName; }
            set
            {
                if (_PortName == value)
                    return;
                _PortName = value;
                RaisePropertyChanged("PortName");
            }
        }
        #endregion


        #region ComponentType変更通知プロパティ
        string _ComponentType;

        public string ComponentType
        {
            get
            { return _ComponentType; }
            set
            {
                if (_ComponentType == value)
                    return;
                _ComponentType = value;
                RaisePropertyChanged("ComponentType");
            }
        }
        #endregion
      
      
      
    }
}
