using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Livet;
using RtStorage.Properties;
using RtStorage.ViewModels;
using RtUtility;
using omg.org.RTC;

namespace RtStorage.Models
{
    public class NamingServiceManager : NotificationObject
    {
        // Singletonにすべきか？？
        private static readonly NamingServiceManager _instance = new NamingServiceManager();

        public static NamingServiceManager Default
        {
            get { return _instance; }
        }

        private NamingServiceManager()
        {
            IsUpdating = false;

            NamingServices = new ObservableCollection<NamingService>();

            OutPortTree = new ObservableCollection<TreeViewItemViewModel>();
            InPortTree = new ObservableCollection<TreeViewItemViewModel>();

        }



        public void Initialize()
        {

            // 設定ファイルに保存されているネーミングサービスの情報を読みだす
            foreach (var ns in Settings.Default.NamingServices)
            {
                var items = ns.Split(':');
                if (items.Length != 2)
                {
                    continue;
                }

                var host = items[0];
                int port;
                if (!int.TryParse(items[1], out port))
                {
                    continue;
                }

                AddNamingService(new NamingService(host, port));
            }

            UpdateAsync();
        }

        public ObservableCollection<NamingService> NamingServices { get; private set; }


        public bool AddNamingService(string hostName, int portNumber)
        {
            lock (NamingServices)
            {
                //すでに追加済み
                if (NamingServices.Any(ns => ns.Client.Key == hostName + ":" + portNumber))
                {
                    return false;
                }

                var namingService = new NamingService(hostName, portNumber);

                AddNamingService(namingService);

                return true;
            }
        }
        public bool AddNamingService(NamingService namingService)
        {
            lock (NamingServices)
            {
                //すでに追加済み
                if (NamingServices.Any(ns=>ns.Client.Key == namingService.Client.Key))
                {
                    return false;
                }

                NamingServices.Add(namingService);

                // 設定ファイルに保存されていない情報であれば、設定ファイルに追加して保存する。
                if (!Settings.Default.NamingServices.Cast<string>().Any(x => x == namingService.Client.Key))
                {
                    Settings.Default.NamingServices.Add(namingService.Client.Key);
                    Settings.Default.Save();
                }
                return true;
            }
        }

        public bool RemoveNamingService(string key)
        {
            lock (NamingServices)
            {
                var ns = NamingServices.FirstOrDefault(x=>x.Client.Key==key);
                if (ns != null)
                {
                    // 設定ファイルから削除する
                    if (Settings.Default.NamingServices.Cast<string>().Any(x => x == ns.Client.Key))
                    {
                        Settings.Default.NamingServices.Remove(ns.Client.Key);
                        Settings.Default.Save();
                    }

                    NamingServices.Remove(ns);

                    return true;
                }

                return false;
            }
        }
        
        public RTObject GetComponent(string namingName)
        {
            lock (NamingServices)
            {
                var pos = namingName.IndexOf('/');

                // ネーミングサービスの名前を取得
                var nsName = namingName.Substring(0, pos);

                // コンポーネントの名前を取得
                var compName = namingName.Substring(pos + 1, namingName.Length - (pos + 1));

                //TODO: 見つからなかった場合は？
                var namingService = NamingServices.First(ns => ns.Client.Key == nsName);
                var comp = namingService.Client.GetObject<RTObject>(compName);

                return comp;
            }
        }


        // モデルなのにViewModelをもっている・・・
        // 以下の処理はどこか別のViewModelにもっていきたい
        public ObservableCollection<TreeViewItemViewModel> OutPortTree { get; private set; }
        public ObservableCollection<TreeViewItemViewModel> InPortTree { get; private set; }


        #region IsUpdating変更通知プロパティ
        bool _IsUpdating;

        public bool IsUpdating
        {
            get
            { return _IsUpdating; }
            set
            {
                if (_IsUpdating == value)
                    return;
                _IsUpdating = value;
                RaisePropertyChanged("IsUpdating");
            }
        }
        #endregion


        public void UpdateAsync()
        {
            List<NamingService> namingServices;

            // 更新処理中にネーミングサービスの追加や削除されると困るのでコピーしておく。
            // (更新処理中にずっとロックしておくと、AddやRemoveが止まってしまう)
            lock (NamingServices)
            {
                namingServices = NamingServices.ToList();
                IsUpdating = true;
            }

            Observable.Start(() =>
            {
                lock (OutPortTree)
                lock (InPortTree)
                {
                    OutPortTree.Clear(); //TODO: Clearだと通知されないことがある。RemoveAllだとアクセスエラー。
                    InPortTree.Clear();

                    foreach (var namingService in namingServices)
                    {
                        try
                        {
                            namingService.Client.ClearZombie<RTObject>();
                            OutPortTree.Add(namingService.Factory.CreateOutPortTree());
                            InPortTree.Add(namingService.Factory.CreateInPortTree());
                        }
                        catch (Exception)
                        {
                            namingService.IsAlive = false;
                        }
                    }
                }
            }).Subscribe(_ => IsUpdating = false);

        }

    }
}
