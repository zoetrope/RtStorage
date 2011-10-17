using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

using Livet;
using OpenRTM;
using RtStorage.Models;
using RtStorage.Properties;
using RtUtility;
using RtUtility.Mock;
using omg.org.CORBA;
using omg.org.RTC;

namespace RtStorage
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;


            NativeTime.BeginPeriod(1);

            CorbaUtility.Initialize();
            LogManager.Initialize();
            SettingHolder.BaseDirectory = Settings.Default.DataDirectory;


            var dir = SettingHolder.BaseDirectory;
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                // データベースファイルが見つからなかった場合は、空のファイルを作成
                var filePath = dir + "RecordDescriptions.db";
                if (!File.Exists(filePath))
                {
                    using (var writer = new BinaryWriter(File.Create(filePath)))
                    {
                        writer.Write(RtStorage.Properties.Resources.RecordDescriptions);
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: ユーザーへのエラー通知が必要
            }

#if !USE_MOCK
            NamingServiceManager.Default.Initialize();
#endif

#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

#if USE_MOCK

            var ns = MockFactory.CreateNamingService("MockServer", 2809);
            var inport = new DelegateInPortCdr(_ => PortStatus.PORT_OK);
            ns.RtObjects = new Dictionary<string, RTObject>()
               {
                   {"SampleIn0.rtc", MockFactory.CreateRTObject("SampleIn", "SampleIn0",
                       MockFactory.CreateInPortService("SampleIn0.in", "IDL:RTC/TimedLong:1.0",inport))},
                   {"SampleIn1.rtc", MockFactory.CreateRTObject("SampleIn", "SampleIn1",
                       MockFactory.CreateInPortService("SampleIn1.in", "IDL:RTC/TimedLong:1.0",inport))},
                   {"SampleOut0.rtc", MockFactory.CreateRTObject("SampleOut", "SampleOut0",
                       MockFactory.CreateOutPortService("SampleOut0.out", "IDL:RTC/TimedLong:1.0"))},
                   {"SampleOut1.rtc", MockFactory.CreateRTObject("SampleOut", "SampleOut1",
                       MockFactory.CreateOutPortService("SampleOut1.out", "IDL:RTC/TimedLong:1.0"))},
               };

            Observable.Start(() => {
                NamingServiceManager.Default.AddNamingService(new NamingService(ns));
                NamingServiceManager.Default.UpdateAsync();
            });


            var repository = RepositoryFactory.CreateRepository();

            var data = new List<RecordDescription>()
            {
                new RecordDescription(){
                    CreatedDateTime = DateTime.Now,
                    TimeSpan = 10000,
                    NamingName = "MockServer/ConsoleIn0.rtc",
                    ComponentType = "ConsoleIn",
                    PortName = "ConsoleIn0.out",
                    DataType = "IDL:RTC/TimedLong:1.0",
                    SumSize = 120000000000,
                    Count = 10,
                    IsLittleEndian = 1,
                    IndexFileName = "Data/hoge.index",
                    DataFileName = "Data/hoge.data",
                },
                new RecordDescription(){
                    CreatedDateTime = DateTime.Now,
                    TimeSpan = 10000,
                    NamingName = "MockServer/SampleInOut0.rtc",
                    ComponentType = "SampleInOut",
                    PortName = "SampleInOut0.in",
                    DataType = "IDL:RTC/TimedDouble:1.0",
                    SumSize = 120,
                    Count = 10,
                    IsLittleEndian = 1,
                    IndexFileName = "Data/hoge.index",
                    DataFileName = "Data/hoge.data",
                },
                new RecordDescription(){
                    CreatedDateTime = DateTime.Now,
                    TimeSpan = 10000,
                    NamingName = "MockServer/SampleInOut0.rtc",
                    ComponentType = "SampleInOut",
                    PortName = "SampleInOut0.out",
                    DataType = "IDL:RTC/TimedDouble:1.0",
                    SumSize = 120,
                    Count = 10,
                    IsLittleEndian = 1,
                    IndexFileName = "Data/hoge.index",
                    DataFileName = "Data/hoge.data",
                },
                new RecordDescription(){
                    CreatedDateTime = DateTime.Now,
                    TimeSpan = 10000,
                    NamingName = "MockServer/SampleComp0.rtc",
                    ComponentType = "SampleComp",
                    PortName = "SampleComp0.out",
                    DataType = "IDL:RTC/TimedString:1.0",
                    SumSize = 120,
                    Count = 10,
                    IsLittleEndian = 1,
                    IndexFileName = "Data/hoge.index",
                    DataFileName = "Data/hoge.data",
                }
            };

            data.ForEach(repository.Insert);



            // 通信遅延を発生させて、UIがロックしないことを確認する
            //MockProxy<NamingServiceMock>.WaitTime = TimeSpan.FromSeconds(1);
            //MockProxy<MockRecordDescriptionRepository>.WaitTime = TimeSpan.FromSeconds(1);

            // ネーミングサービスに接続できないエラーが発生した場合の挙動を確認する。
            //MockProxy<NamingServiceMock>.ErrorMap.Add("GetObjectNames", new TRANSIENT());

            // ネーミングサービスに登録されていたコンポーネントに接続できない場合の挙動を確認する
            //MockProxy<RTObjectMock>.ErrorMap.Add("get_ports", new TRANSIENT());
            //MockProxy<RTObjectMock>.ErrorMap.Add("get_component_profile", new TRANSIENT());

            //MockProxy<MockRecordDescriptionRepository>.ErrorMap.Add("*", new Exception());

#endif

        }

        public void Application_Exit(object sender, ExitEventArgs e)
        {
            NativeTime.EndPeriod(1);
        }

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //TODO:ロギング処理など
            MessageBox.Show(
                //"不明なエラーが発生しました。アプリケーションを終了します。",
                (e.ExceptionObject as Exception).Message,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Environment.Exit(1);
        }
    }
}
