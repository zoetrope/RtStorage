using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using Livet;
using OpenRTM;
using RtUtility;
using omg.org.CORBA;
using omg.org.RTC;

namespace RtStorage.Models
{
    /// <summary>
    /// 接続先のOutPortから出力されたデータを保存する
    /// </summary>
    public class Recorder : NotificationObject
    {
        private readonly string _namingName;
        private readonly string _portName;
        private readonly object _lockObject = new object();

        private PortService _portService;

        private string _connectorId;
        private readonly Stopwatch _stopWatch = new Stopwatch();

        private BinaryWriter _indexFileWriter;
        private BinaryWriter _dataFileWriter;

        public RecordDescription RecordDescription { get; private set; }


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

        private Logger _logger = LogManager.GetLogger();

        public Recorder(string namingName, string portName)
        {
            _namingName = namingName;
            _portName = portName;

            AutoActivate = true;
            IsRecording = false;
            IsAlive = false;
            IsPausing = false;

            Initialize();
        }


        private void Start()
        {
            lock (_lockObject)
            {
                if (!IsAlive) return;
                if (IsRecording) return;

                IsRecording = true;

                try
                {
                    var adapter = new DelegateInPortCdr(Write);
                    var orb = OrbServices.GetSingleton();
                    var ior = orb.object_to_string(adapter);

                    var prof = new ConnectorProfile();
                    prof.ports = new[] {_portService};
                    prof.name = "dummy"; // 名前見直し？
                    prof.connector_id = "";

                    ConnectorProfileExtension.AddDataFlowType(ref prof, "push");
                    ConnectorProfileExtension.AddInterfaceType(ref prof, "corba_cdr");
                    ConnectorProfileExtension.AddSubscriptionType(ref prof, "new");
                    ConnectorProfileExtension.AddInPortIor(ref prof, ior);

                    var ret = _portService.connect(ref prof);

                    if (ret != ReturnCode_t.RTC_OK)
                    {
                        throw new Exception("接続に失敗しました。");
                    }

                    _connectorId = prof.connector_id;
                    //TODO: エンディアンチェックしたいけど、ミドルウェアが対応してない。
                    //name = "dataport.serializer.cdr.endian"

                }
                catch (Exception)
                {
                    IsAlive = false;
                    NotifyError("ポートの接続に失敗しました。");
                    return;
                }

                var now = DateTime.Now;
                RecordDescription.CreatedDateTime = now;
                RecordDescription.Count = 0;
                RecordDescription.SumSize = 0;
                RecordDescription.IsLittleEndian = 1;

                var fileName = FileUtility.ValidFileName(now.ToString("yyyyMMdd_HHmmss") + "_" + Key);
                RecordDescription.DataFileName = fileName + ".data";
                RecordDescription.IndexFileName = fileName + ".index";
                

                try
                {
                    if (!Directory.Exists(SettingHolder.BaseDirectory))
                    {
                        Directory.CreateDirectory(SettingHolder.BaseDirectory);
                    }
                    _dataFileWriter = new BinaryWriter(
                        File.Create(SettingHolder.BaseDirectory + RecordDescription.DataFileName));
                    _indexFileWriter = new BinaryWriter(
                        File.Create(SettingHolder.BaseDirectory + RecordDescription.IndexFileName));

                }
                catch (Exception)
                {
                    IsAlive = false;
                    NotifyError("ファイルを開くことができませんでした。");
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

                IsRecording = true;
            }
        }

        public void Stop()
        {
            if (!IsAlive) return;
            if (!IsRecording) return;

            try
            {
                var ret = _portService.disconnect(_connectorId);
            }
            catch (Exception)
            {
                //Do nothing.
            }

            lock (_lockObject) // disconnectのコールバックでデータ書き込みをするコンポーネントもあるので、ここでロック
            {
                IsRecording = false;

                _dataFileWriter.Dispose();
                _indexFileWriter.Dispose();

                RecordDescription.TimeSpan = _stopWatch.ElapsedMilliseconds;
                _stopWatch.Reset();

            }
        }


        private PortStatus Write(byte[] data)
        {
            lock (_lockObject)
            {
                if (!IsAlive) return PortStatus.PORT_ERROR;
                if (!IsRecording) return PortStatus.PORT_ERROR;

                if (IsPausing) return PortStatus.PORT_OK;

                try
                {
                    var current = _stopWatch.ElapsedMilliseconds;

                    _dataFileWriter.Write(data, 0, data.Length);


                    _indexFileWriter.Write(current);
                    _indexFileWriter.Write(RecordDescription.SumSize);
                    _indexFileWriter.Write(data.LongLength);

                    RecordDescription.TimeSpan = _stopWatch.ElapsedMilliseconds;
                    RecordDescription.SumSize += data.LongLength;
                    RecordDescription.Count++;
                }
                catch (IOException)
                {
                    IsAlive = false;
                    IsPausing = true;
                    _stopWatch.Stop();
                    NotifyError("ファイルの書き込みに失敗しました。");
                }

                return PortStatus.PORT_OK;
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

                    RecordDescription = new RecordDescription();
                    RecordDescription.NamingName = _namingName;
                    RecordDescription.ComponentType = comp.get_component_profile().type_name;

                    var prof = _portService.get_port_profile();
                    Key = _namingName + ":" + prof.name;

                    RecordDescription.PortName = prof.name;
                    RecordDescription.DataType = prof.GetDataType();

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
      

        #region IsRecording変更通知プロパティ
        bool _IsRecording;

        public bool IsRecording
        {
            get
            { return _IsRecording; }
            set
            {
                if (_IsRecording == value)
                    return;
                _IsRecording = value;
                RaisePropertyChanged("IsRecording");
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
