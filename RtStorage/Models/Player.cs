using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Livet;
using OpenRTM;
using RtUtility;
using omg.org.CORBA;
using omg.org.RTC;
using System.Threading.Tasks;

namespace RtStorage.Models
{
    public class ErrorInfoEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    /// <summary>
    /// 保存データを接続先のInPortに送信するクラス
    /// </summary>
    public class Player : NotificationObject
    {
        private readonly string _namingName;
        private readonly string _portName;
        private readonly object _lockObject = new object();

        private PortService _portService;
        private InPortCdr _inPortCdr;

        public Player(RecordDescription desc, string namingName, string portName)
        {
            RecordDescription = desc;
            _namingName = namingName;
            _portName = portName;

            AutoActivate = true;
            EnableLoop = false;
            IsPlaying = false;
            IsAlive = false;
            IsPausing = false;

            Initialize();
        }

        public event EventHandler<ErrorInfoEventArgs> ErrorRaised;
        void NotifyError(string message)
        {
            var arg = new ErrorInfoEventArgs() { Message = message };
            var handler = ErrorRaised;
            if (handler != null)
            {
                handler(this, arg);
            }
        }


        public string Key { get; set; }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        public RecordDescription RecordDescription { get; private set; }

        private BinaryReader _indexFileReader;
        private BinaryReader _dataFileReader;

        private CancellationTokenSource _cancellation;
        public void Start()
        {
            lock (_lockObject)
            {
                ConnectorProfile prof;
                try
                {
                    prof = new ConnectorProfile();
                    prof.ports = new[] { _portService };
                    prof.name = "dummy"; //名前見直し？
                    prof.connector_id = "";
                    ConnectorProfileExtension.AddDataFlowType(ref prof, "push");
                    ConnectorProfileExtension.AddInterfaceType(ref prof, "corba_cdr");
                    ConnectorProfileExtension.AddSubscriptionType(ref prof, "new");
                    //TODO: エンディアンは？

                    var ret = _portService.connect(ref prof);

                    if (ret != ReturnCode_t.RTC_OK)
                    {
                        throw new Exception("ポートの接続に失敗しました。");
                    }

                    

                }
                catch (Exception)
                {
                    IsAlive = false;
                    NotifyError("ポートの接続に失敗しました。");
                    return;
                }

                var ior = prof.GetInPortIor();

                var orb = OrbServices.GetSingleton();
                _inPortCdr = (InPortCdr)orb.string_to_object(ior);

                try
                {
                    _dataFileReader = new BinaryReader(
                        File.OpenRead(SettingHolder.BaseDirectory + RecordDescription.DataFileName));
                    _indexFileReader = new BinaryReader(
                        File.OpenRead(SettingHolder.BaseDirectory + RecordDescription.IndexFileName));
                }
                catch (Exception)
                {
                    IsAlive = false;
                    NotifyError("再生ファイルが開けませんでした。");
                    return;
                }

                try
                {
                    if (AutoActivate)
                    {
                        var comp = NamingServiceManager.Default.GetComponent(_namingName);
                        if (comp.GetState(0) == LifeCycleState.INACTIVE_STATE)
                        {
                            comp.Activate(0);
                        }

                        comp.WaitState(LifeCycleState.ACTIVE_STATE, TimeSpan.FromSeconds(5));
                    }

                }
                catch (Exception)
                {
                    IsAlive = false;
                    NotifyError("RTコンポーネントの活性化に失敗しました。");
                    return;
                }

                _stopWatch.Start();
                CurrentCount = 0;
                IsPlaying = true;

                _cancellation = new CancellationTokenSource();
                _task = Task.Factory.StartNew(Run, _cancellation.Token);

            }
        }

        private Task _task;

        private void Run()
        {
            while (IsPlaying)
            {
                _cancellation.Token.ThrowIfCancellationRequested();
                if (IsPausing)
                {
                    Thread.Sleep(100);
                    continue;
                }
                long time;
                byte[] data;

                lock (_lockObject)
                {
                    try
                    {
                        lock (_indexFileReader)
                        {
                            time = _indexFileReader.ReadInt64();
                            _indexFileReader.ReadInt64();
                            long size = _indexFileReader.ReadInt64();

                            data = _dataFileReader.ReadBytes((int) size);
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        if (EnableLoop)
                        {
                            // いったん停止して最初に戻して再開
                            Pause();
                            CurrentCount = 0;
                            SetPosition();
                            Resume();
                        }
                        else
                        {
                            // すべて再生し終わったらポーズ
                            Pause();
                            CurrentCount = 0;
                        }
                        continue;

                    }
                    catch (IOException)
                    {
                        _dataFileReader.Dispose();
                        _indexFileReader.Dispose();

                        _stopWatch.Reset();
                        IsPlaying = false;
                        IsAlive = false;
                        return;
                    }
                }

                int waittime = (int)(time - (_stopWatch.ElapsedMilliseconds + _timeOffset));
                // waittimeがマイナスのときは再生処理が遅れているということ。waitしない。

                if (waittime > 100)
                {
                    // 100msec秒ごとにキャンセルが発生していないか、ポーズされていないかチェック。

                    for (int i = 0; i < waittime / 100; i++)
                    {
                        Thread.Sleep(100);
                        _cancellation.Token.ThrowIfCancellationRequested();

                        if (IsPausing)
                        {
                            break;
                        }
                    }
                    // 残り
                    Thread.Sleep(waittime % 100);
                }
                else if (waittime > 0)
                {
                    // 100msec以下なら区切らずに待つ
                    Thread.Sleep(waittime);
                }

                _cancellation.Token.ThrowIfCancellationRequested();
                if (IsPausing)
                {
                    continue;
                }
                CurrentCount = CurrentCount + 1;

                try
                {
                    _inPortCdr.put(data);
                }
                catch (Exception)
                {
                    IsAlive = false;

                    _dataFileReader.Dispose();
                    _indexFileReader.Dispose();

                    _stopWatch.Reset();
                    IsPlaying = false;
                    NotifyError("データの送信に失敗しました。");
                }
            }

        }


        public void Stop()
        {
            try
            {
                if (!_task.IsCompleted)
                {
                    _cancellation.Cancel();
                    _task.Wait();
                }
            }
            catch (AggregateException)
            {
            }

            lock (_lockObject)
            {
                _dataFileReader.Dispose();
                _indexFileReader.Dispose();

                _timeOffset = 0;
                _stopWatch.Reset();
                IsPlaying = false;
            }
        }



        public void Play()
        {
            if (IsPausing)
            {
                Resume();
            }
            else
            {
                Start();
            }
        }

        public void Resume()
        {
            IsPausing = false;
            _stopWatch.Start();
        }

        public void Pause()
        {
            IsPausing = true;
            _stopWatch.Stop();
        }

        private long _timeOffset = 0;
        internal void SetPosition()
        {
            if (!IsPlaying)
            {
                // これでいいのか？
                // 再生していないときはファイルにアクセスできない。
                return;
            }

            
            lock (_indexFileReader)
            {
                try
                {

                    // インデックスファイルは1データ24Byte(8Byte×3)
                    var indexPos = CurrentCount * 24;

                    _indexFileReader.BaseStream.Seek(indexPos, SeekOrigin.Begin);

                    _indexFileReader.ReadInt64();
                    long position = _indexFileReader.ReadInt64();
                    _indexFileReader.ReadInt64();

                    long offsetTime;
                    if (CurrentCount > 0)
                    {
                        // オフセット時間は1つ前のもの利用する。
                        _indexFileReader.BaseStream.Seek(-48, SeekOrigin.Current);

                        offsetTime = _indexFileReader.ReadInt64();
                        _indexFileReader.ReadInt64();
                        _indexFileReader.ReadInt64();
                    }
                    else
                    {
                        _indexFileReader.BaseStream.Seek(-24, SeekOrigin.Current);
                        // ↑でReadInt64を3回呼び出したので元に戻す。

                        offsetTime = 0;
                        // 一番最初のデータの場合はオフセットなし
                    }

                    // 再生位置を変更すると計測している時間がずれる。StopWatchをリセットしてオフセットで調整。
                    _stopWatch.Reset();
                    _timeOffset = offsetTime;

                    _dataFileReader.BaseStream.Seek(position, SeekOrigin.Begin);

                }
                catch (Exception)
                {
                    IsAlive = false;
                    NotifyError("再生位置の変更に失敗しました。");
                    return;
                }
            }
        }

        public bool Initialize()
        {
            lock (_lockObject)
            {
                if (IsAlive) return false;

                try
                {
                    var comp = NamingServiceManager.Default.GetComponent(_namingName);
                    _portService = comp.GetPort(_portName);
                    var prof = _portService.get_port_profile();
                    Key = _namingName + ":" + prof.name;

                    IsAlive = true;

                    return true;
                }
                catch (Exception)
                {
                    IsAlive = false;
                    NotifyError("RTコンポーネントの接続に失敗しました。");
                    return false;
                }
            }
        }

        #region CurrentCount変更通知プロパティ
        private long _currentCount;

        public long CurrentCount
        {
            get
            { return _currentCount; }
            set
            {
                if (_currentCount == value)
                    return;
                _currentCount = value;
                RaisePropertyChanged("CurrentCount");
            }
        }
        #endregion

        
        #region AutoActivate変更通知プロパティ
        bool _AutoActivate;

        public bool AutoActivate
        {
            get
            { return _AutoActivate; }
            set
            {
                if (_AutoActivate == value)
                    return;
                _AutoActivate = value;
                RaisePropertyChanged("AutoActivate");
            }
        }
        #endregion


        #region EnableLoop変更通知プロパティ
        bool _EnableLoop;

        public bool EnableLoop
        {
            get
            { return _EnableLoop; }
            set
            {
                if (_EnableLoop == value)
                    return;
                _EnableLoop = value;
                RaisePropertyChanged("EnableLoop");
            }
        }
        #endregion
      


        #region IsAlive変更通知プロパティ
        private bool _IsAlive;

        public bool IsAlive
        {
            get
            { return _IsAlive; }
            set
            {
                if (_IsAlive == value)
                    return;
                _IsAlive = value;
                RaisePropertyChanged("IsAlive");
            }
        }
        #endregion
      

        #region IsPlaying変更通知プロパティ
        bool _IsPlaying;

        public bool IsPlaying
        {
            get
            { return _IsPlaying; }
            set
            {
                if (_IsPlaying == value)
                    return;
                _IsPlaying = value;
                RaisePropertyChanged("IsPlaying");
            }
        }
        #endregion


        #region IsPausing変更通知プロパティ
        bool _IsPausing;

        public bool IsPausing
        {
            get
            { return _IsPausing; }
            set
            {
                if (_IsPausing == value)
                    return;
                _IsPausing = value;
                RaisePropertyChanged("IsPausing");
            }
        }
        #endregion
      

    }
}
