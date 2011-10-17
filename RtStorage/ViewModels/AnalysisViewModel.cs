using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls.DataVisualization.Charting;
using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using Codeplex.Reactive.Notifier;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;

using RtStorage.Models;
using RtUtility;
using RtStorage.Properties;

namespace RtStorage.ViewModels
{
    public class AnalysisViewModel : ViewModel
    {
        public ReactiveCommand UpdateCommand { get; private set; }

        private NamingServiceManager _manager;
        public AnalysisViewModel(NamingServiceManager maanger)
        {
            _manager = _manager;

            var isCreating = new SignalNotifier();

            // nullではないAnalyzerがセットされたら更新実行が可能
            UpdateCommand = Observable
                .FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
                .Where(e => e.EventArgs.PropertyName == "Analyzer")
                .Select(_ => _Analyzer != null)
                .Merge(isCreating.Select(x => x == SignalChangedStatus.Empty))
                .ToReactiveCommand(false);

            Items = UpdateCommand
                .SelectMany(_ => AnalyzeAsync(isCreating))
                .Do(_ => Items.RemoveAll())
                .SelectMany(_ => _)
                .ToReactiveCollection();
        }
        public ReactiveCollection<Histogram> Items { get; private set; }

        public IObservable<IEnumerable<Histogram>> AnalyzeAsync(SignalNotifier isCreating)
        {
            return Observable.Start(() =>
            {
                isCreating.Increment();
                var ret = Analyzer.CreateHistogram();
                var ymax = ret.Max(x => x.Tally);
                if (ymax > 10)
                {
                    YAxisInterval = (int)(ymax/10);
                }
                else
                {
                    // 小数点表示したくないので、Intervalの最小値は1
                    YAxisInterval = 1;
                }

                // グラフ表示に少し余裕を持たせたいので20%分余白をつくる。
                YAxisMaximum = ymax * 1.2;

                return ret;
            })
            .Finally(() => isCreating.Decrement())
            .Catch((Exception ex) => Messenger.Raise(new InformationMessage("データ解析に失敗しました。", "エラー", "ShowError")));
        }

        public void NotifyAdopt(RecordDescription description)
        {
            Analyzer = new Analyzer(description);
            
            UpdateCommand.Execute(null);
        }

        #region SearchRecordCommand
        ViewModelCommand _SearchRecordCommand;

        public ViewModelCommand SearchRecordCommand
        {
            get
            {
                if (_SearchRecordCommand == null)
                    _SearchRecordCommand = new ViewModelCommand(SearchRecord);
                return _SearchRecordCommand;
            }
        }

        private void SearchRecord()
        {
            var vm = new SearchRecordWindowViewModel(NotifyAdopt);
            Messenger.Raise(new TransitionMessage(vm, "OpenSearchRecordWindow"));
        }
        #endregion

        
        #region YAxisInterval変更通知プロパティ
        double _YAxisInterval;

        public double YAxisInterval
        {
            get
            { return _YAxisInterval; }
            set
            {
                if (_YAxisInterval == value)
                    return;

                _YAxisInterval = value;
                RaisePropertyChanged("YAxisInterval");
            }
        }
        #endregion


        #region YAxisMaximum変更通知プロパティ
        double _YAxisMaximum;

        public double YAxisMaximum
        {
            get
            { return _YAxisMaximum; }
            set
            {
                if (_YAxisMaximum == value)
                    return;
                _YAxisMaximum = value;
                RaisePropertyChanged("YAxisMaximum");
            }
        }
        #endregion
      

        
        #region Analyzer変更通知プロパティ
        Analyzer _Analyzer;

        public Analyzer Analyzer
        {
            get
            { return _Analyzer; }
            set
            {
                if (_Analyzer == value)
                    return;
                _Analyzer = value;
                RaisePropertyChanged("Analyzer");
            }
        }
        #endregion
      

        
    }

    
}
