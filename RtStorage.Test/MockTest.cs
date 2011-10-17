using System;
using System.Reactive.Linq;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenRTM;
using RtStorage.Models;
using RtUtility.Mock;
using omg.org.CORBA;
using omg.org.RTC;
using RtUtility;

namespace RtStorage.Test
{
    [TestClass]
    public class MockTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //MockProxy<PortServiceMock>.IsErrorMode = true;
            MockProxy<RTObjectMock>.ErrorMap.Add("get_component_profile", new TRANSIENT());

            var ns = MockFactory.CreateNamingService("127.0.0.1", 2809);
            ns.RtObjects = new Dictionary<string, RTObject>()
               {
                   {"hoge.rtc", MockFactory.CreateRTObject("hoge", "hoge0",
                       MockFactory.CreateInPortService("test", "IDL:RTC.TimedLong:1.0",
                       new DelegateInPortCdr(_ => PortStatus.PORT_OK)))},
               };

            var mock = ns.GetObject<RTObject>("hoge.rtc") as RTObjectMock;

            Observable.Start(() => mock.get_component_profile())
                .Catch((TRANSIENT ex) => Console.WriteLine(ex.Message))
                .Subscribe(prof => Console.WriteLine(prof.instance_name));

            MockProxy<RTObjectMock>.ErrorMap.Clear();

        }
    }
}
