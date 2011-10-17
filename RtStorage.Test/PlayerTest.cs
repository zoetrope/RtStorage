using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenRTM;
using RtStorage.Models;
using RtUtility;
using RtUtility.Mock;
using omg.org.RTC;

namespace RtStorage.Test
{
    [TestClass]
    public class PlayerTest
    {

        private const string TestDataDirectory = @"../../TestData/";

        private IObservable<byte[]> _observer;

        [TestInitialize]
        public void Initialize()
        {
            CorbaUtility.Initialize();
            LogManager.Initialize();

            var subject = new Subject<byte[]>();
            _observer = subject.AsObservable();

            var ns = MockFactory.CreateNamingService("127.0.0.1", 2809);

            ns.RtObjects = new Dictionary<string, RTObject>()
               {
                   {"SampleIn0.rtc", MockFactory.CreateRTObject("SampleIn", "SampleIn0",
                       MockFactory.CreateInPortService("SampleIn0.in", "IDL:RTC.TimedLong:1.0",
                       new DelegateInPortCdr(_ => {
                           subject.OnNext(_);
                           return PortStatus.PORT_OK;
                       })))},
               };
            NamingServiceManager.Default.AddNamingService(new NamingService(ns));

            SettingHolder.BaseDirectory = TestDataDirectory;



        }

        [TestCleanup]
        public void Cleanup()
        {
            NamingServiceManager.Default.RemoveNamingService("127.0.0.1:2809");
        }

        [TestMethod]
        public void 初期化のテスト()
        {
            var description = new RecordDescription()
            {
                CreatedDateTime = DateTime.Now,
                TimeSpan = 1661,
                NamingName = "127.0.0.1:2809/ConsoleOut0.rtc",
                ComponentType = "SampleOut",
                PortName = "SampleOut0.out",
                DataType = "IDL:RTC/TimedLong:1.0",
                SumSize = 1200,
                Count = 100,
                IsLittleEndian = 1,
                IndexFileName = "TestData001.index",
                DataFileName = "TestData001.data",
            };

            var player = new Player(description, "127.0.0.1:2809/SampleIn0.rtc", "SampleIn0.in");

            player.IsNotNull();

            player.IsAlive.Is(true);
        }

        [TestMethod]
        public void ファイルを再生するテスト()
        {
            var description = new RecordDescription()
            {
                CreatedDateTime = DateTime.Now,
                TimeSpan = 1661,
                NamingName = "127.0.0.1:2809/ConsoleOut0.rtc",
                ComponentType = "SampleOut",
                PortName = "SampleOut0.out",
                DataType = "IDL:RTC/TimedLong:1.0",
                SumSize = 1200,
                Count = 100,
                IsLittleEndian = 1,
                IndexFileName = "TestData001.index",
                DataFileName = "TestData001.data",
            };

            var player = new Player(description, "127.0.0.1:2809/SampleIn0.rtc", "SampleIn0.in");
            
            player.IsNotNull();

            player.IsAlive.Is(true);

            player.Start();

            player.IsAlive.Is(true);

            // 100回putされるまで待つ。10秒以内に完了しなかったらTimeoutException
            var data = _observer.Take(100)
                .Timeout(TimeSpan.FromSeconds(10))
                .ToEnumerable().ToList();

            data.Count().Is(100);

            player.Stop();

            player.IsAlive.Is(true);
        }

        [TestMethod]
        public void キャンセルするテスト()
        {

            var description = new RecordDescription()
            {
                CreatedDateTime = DateTime.Now,
                TimeSpan = 17460,
                NamingName = "localhost:2809/ConsoleIn0.rtc",
                ComponentType = "ConsoleIn",
                PortName = "ConsoleIn0.out",
                DataType = "IDL:RTC/TimedLong:1.0",
                SumSize = 228,
                Count = 19,
                IsLittleEndian = 1,
                IndexFileName = "TestData002.index",
                DataFileName = "TestData002.data",
            };

            var player = new Player(description, "127.0.0.1:2809/SampleIn0.rtc", "SampleIn0.in");

            player.IsNotNull();

            player.IsAlive.Is(true);

            player.Start();

            player.IsAlive.Is(true);
            player.IsPlaying.Is(true);

            _observer.Take(3)
                .Do(x => Console.WriteLine(x[8]))
                .Timeout(TimeSpan.FromSeconds(30))
                .ToEnumerable().ToList();

            player.Stop();

            player.IsAlive.Is(true);
            player.IsPlaying.Is(false);


        }


        [TestMethod]
        public void 一時停止と再開()
        {

        }

        [TestMethod]
        public void 複数回の再生()
        {

        }
        

    }
}
