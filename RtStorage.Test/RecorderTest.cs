using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RTC;
using RtStorage.Models;
using RtUtility;
using RtUtility.Mock;
using omg.org.CORBA;
using omg.org.RTC;

namespace RtStorage.Test
{
    [TestClass]
    public class RecorderTest
    {
        private const string TestDataDirectory = @"./Data/";

        [TestInitialize]
        public void Initialize()
        {
            CorbaUtility.Initialize();
            LogManager.Initialize();

            var ns = MockFactory.CreateNamingService("127.0.0.1", 2809);
            ns.RtObjects = new Dictionary<string, RTObject>()
               {
                   {"SampleOut0.rtc", MockFactory.CreateRTObject("SampleOut", "SampleOut0",
                       MockFactory.CreateOutPortService("SampleOut0.out", "IDL:RTC.TimedLong:1.0"))},
               };
            NamingServiceManager.Default.AddNamingService(new NamingService(ns));

            SettingHolder.BaseDirectory = TestDataDirectory;

            if (Directory.Exists(TestDataDirectory))
            {
                Directory.Delete(TestDataDirectory, true);
                while (Directory.Exists(TestDataDirectory))
                {
                    //完全に消えるまで待つ
                    Thread.Sleep(10);
                }
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            NamingServiceManager.Default.RemoveNamingService("127.0.0.1:2809");
        }

        [TestMethod]
        public void 初期化だけ()
        {
            var recorder = new Recorder("127.0.0.1:2809/SampleOut0.rtc", "SampleOut0.out");

            recorder.IsAlive.Is(true);
        }


        [TestMethod]
        public void 開始して何もせず終了()
        {
            var recorder = new Recorder("127.0.0.1:2809/SampleOut0.rtc", "SampleOut0.out");

            recorder.IsAlive.Is(true);

            recorder.Play();

            recorder.IsAlive.Is(true);

            recorder.Stop();

            recorder.IsAlive.Is(true);

            //TODO: InPortCdr のスレッドが終了するときのエラー？
        }

        [TestMethod]
        public void データを書き込んでみる()
        {
            var recorder = new Recorder("127.0.0.1:2809/SampleOut0.rtc", "SampleOut0.out");

            recorder.IsAlive.Is(true);

            recorder.Play();

            recorder.IsAlive.Is(true);

            var comp = NamingServiceManager.Default.GetComponent("127.0.0.1:2809/SampleOut0.rtc");
            var port = comp.GetPort("SampleOut0.out") as OutPortServiceMock;

            port.IsNotNull();

            var inportcdr = port.GetInPortCdr();

            inportcdr.IsNotNull();

            var factory = new CdrSerializerFactory();
            var serializer = factory.GetSerializer<TimedLong>();

            for (int i = 0; i < 100; i++)
            {
                var stream = new MemoryStream();
                var data = new TimedLong(new Time(0x12, 0x34), i);
                serializer.Serialize(data, stream);

                inportcdr.put(stream.ToArray());
            }
            
            recorder.IsAlive.Is(true);

            recorder.Stop();

            recorder.IsAlive.Is(true);

            recorder.RecordDescription.Count.Is(100);
            recorder.RecordDescription.SumSize.Is(1200);

            Directory.EnumerateFiles(TestDataDirectory).Count().Is(2);
        }

    }
}
