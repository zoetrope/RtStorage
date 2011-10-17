using System;
using System.Collections.Generic;
using System.Linq;
using omg.org.RTC;
using org.omg.SDOPackage;

namespace RtUtility
{
    /// <summary>
    ///   ポートの接続切断を行うユーティリティクラスのインタフェース
    /// </summary>
    public  interface  IConnector
    {
        /// <summary>
        ///   ポート間の接続を行う
        /// </summary>
        /// <returns>実行の成否</returns>
        ReturnCode_t Connect();

        /// <summary>
        ///   ポート間の切断を行う
        /// </summary>
        /// <returns>実行の成否</returns>
        ReturnCode_t Disconnect();

        /// <summary>
        ///   接続済みかどうかを取得するプロパティ
        /// </summary>
        bool IsConnected { get; }
    }

    /// <summary>
    /// データポートやサービスポートの接続を管理するユーティリティクラス
    /// </summary>
    public class PortConnector : IConnector
    {
        private readonly Dictionary<string, List<string>> _propertySettings = new Dictionary<string, List<string>>();

        private ConnectorProfile _profile;
        private readonly List<PortService> _portServices;
        private string _connectorId;

        /// <summary>
        /// <see cref="PortConnector"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="ports">接続対象のポート</param>
        public PortConnector(params PortService[] ports)
        {
            _portServices = new List<PortService>(ports);
            CreatePropertySettings();

            _profile = new ConnectorProfile();
            _profile.ports = ports;

            var props = new List<NameValue>();
            foreach (var setting in _propertySettings)
            {
                if (setting.Value.Count > 0)
                {
                    props.Add(new NameValue(setting.Key, setting.Value[0]));
                }
            }

            _profile.properties = props.ToArray();
        }

        /// <summary>
        /// ConnecotrProfileをもとにPortConnectorを作成する
        /// </summary>
        /// <param name="profile">接続情報</param>
        public PortConnector(ConnectorProfile profile)
        {
            _portServices = profile.ports.ToList();
            CreatePropertySettings();
            _profile = profile;
            _connectorId = _profile.connector_id;
            IsConnected = true;
        }

        private void CreatePropertySettings()
        {
            // 全ポートが持っているプロパティのキーの数を数える
            var keyCount = new Dictionary<string, int>();
            foreach (var port in _portServices)
            {
                foreach (var p in port.get_port_profile().properties)
                {
                    if (keyCount.ContainsKey(p.name))
                    {
                        keyCount[p.name]++;
                    }
                    else
                    {
                        keyCount.Add(p.name, 1);
                    }
                }
            }

            // 全ポートが持っているキーのみを取得する
            foreach (var kc in keyCount)
            {
                if (kc.Value == _portServices.Count)
                {
                    _propertySettings.Add(kc.Key, new List<string>());
                }
            }

            foreach (var key in _propertySettings.Keys)
            {
                int anyCount = 0;
                // 全ポートが持っているプロパティの値の数を数える
                var valueCount = new Dictionary<string, int>();
                foreach (var port in _portServices)
                {
                    string v = port.get_port_profile().properties.GetStringValue(key);
                    if (v == "any")
                    {
                        // "any"はすべてにマッチする値である
                        anyCount++;
                        continue;
                    }

                    // 値はカンマで区切られて持たれている
                    string[] values = v.Split(',');

                    foreach (var val in values)
                    {
                        string value = val.Trim();
                        if (valueCount.ContainsKey(value))
                        {
                            valueCount[value]++;
                        }
                        else
                        {
                            valueCount.Add(value, 1);
                        }
                    }
                }

                // 全ポートが持っている値のみを取得する
                foreach (var vc in valueCount)
                {
                    if (vc.Value == _portServices.Count - anyCount)
                    {
                        _propertySettings[key].Add(vc.Key);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public ReturnCode_t Connect()
        {
            if (IsConnected) return ReturnCode_t.PRECONDITION_NOT_MET;

            ReturnCode_t ret = _portServices[0].connect(ref _profile);
            _connectorId = _profile.connector_id;
            if (ret == ReturnCode_t.RTC_OK) IsConnected = true;
            return ret;
        }

        /// <inheritdoc/>
        public ReturnCode_t Disconnect()
        {
            if (!IsConnected) return ReturnCode_t.PRECONDITION_NOT_MET;

            var ret = _portServices[0].disconnect(_connectorId);
            if (ret == ReturnCode_t.RTC_OK)
            {
                IsConnected = false;
                _connectorId = "";
                _profile.connector_id= "";

                var props = new List<NameValue>();

                foreach (var setting in _propertySettings)
                {
                    if (setting.Value.Count > 0)
                    {
                        props.Add(new NameValue(setting.Key, setting.Value[0]));
                    }
                }

                _profile.properties = props.ToArray();
            }
            return ret;
        }

        /// <inheritdoc/>
        public bool IsConnected
        {
            get;
            private set;
        }

        /// <summary>
        /// 現在の設定を取得します。
        /// </summary>
        public Dictionary<string, string> CurrentProperties
        {
            get
            {
                var ret = new Dictionary<string, string>();
                foreach (var p in _profile.properties)
                {
                    ret.Add(p.name,(string)p.value);
                }
                return ret;
            }
        }

        /// <summary>
        /// 設定可能なプロパティ名の一覧を取得します。
        /// </summary>
        /// <returns>設定可能なプロパティ名一覧</returns>
        public List<string> GetPropertyNames()
        {
            return new List<string>(_propertySettings.Keys);
        }

        /// <summary>
        /// 設定可能なプロパティの値の一覧を取得します。
        /// </summary>
        /// <param name="name">プロパティ名</param>
        /// <returns>設定可能なプロパティの値の一覧</returns>
        public List<string> GetPropertyValues(string name)
        {
            if(!_propertySettings.ContainsKey(name))
            {
                throw new ArgumentException(name + "というパラメータは存在しません。");
            }

            return new List<string>(_propertySettings[name]);
        }

        /// <summary>
        /// プロパティを設定します。
        /// </summary>
        /// <param name="name">プロパティ名</param>
        /// <param name="value">値</param>
        /// <exception cref="ArgumentException">存在しないプロパティ名を指定した</exception>
        public void SetProperty(string name, string value)
        {
            if (!_propertySettings.ContainsKey(name))
            {
                throw new ArgumentException(name + "というパラメータは存在しません。");
            }
            _profile.properties.SetStringValue(name, value);
        }

        /// <summary>
        /// プロパティを追加します。
        /// </summary>
        /// <param name="name">プロパティ名</param>
        /// <param name="value">値</param>
        /// <exception cref="ArgumentException">存在するプロパティ名を指定した</exception>
        public void AddProperty(string name, string value)
        {
            
            if(_profile.properties.Any(x => x.name == name))
            {
                throw new ArgumentException(name + "は既に登録されています。");
            }
            NameValueExtension.AddStringValue(ref _profile.properties, name, value);
        }

        /// <summary>
        /// プロパティを選択します。
        /// </summary>
        /// <param name="name">プロパティ名</param>
        /// <param name="value">値</param>
        /// <returns>存在しない値を指定した場合は<c>false</c>を返します。</returns>
        /// <exception cref="ArgumentException">存在しないプロパティ名を指定した</exception>
        public bool SelectProperty(string name, string value)
        {
            if (!_propertySettings.ContainsKey(name))
            {
                throw new ArgumentException(name + "というパラメータは存在しません。");
            }

            if(!_propertySettings[name].Contains(value))
            {
                return false;
            }

            _profile.properties.SetStringValue(name, value);

            return true;
        }

    }


}
