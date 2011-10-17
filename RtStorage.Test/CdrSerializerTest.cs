using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Ch.Elca.Iiop.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RTC;
using RtUtility;

namespace RtStorage.Test
{
    [TestClass]
    public class CdrSerializerTest
    {
        [TestMethod]
        public void シリアライズのテスト()
        {
            var factory = new CdrSerializerFactory();

            var serializer = factory.GetSerializer<TimedLong>(true);

            TimedLong data = new TimedLong(new Time(0x12, 0x34), 0x5678);
            
            var stream = new MemoryStream();
            serializer.Serialize(data, stream);

            stream.Length.Is(12);
            stream.ToArray().Is(new List<byte> {
                0x12, 0x00, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0x78, 0x56, 0x00, 0x00 });
        }

        [TestMethod]
        public void デシリアライズのテスト()
        {
            var factory = new CdrSerializerFactory();

            var serializer = factory.GetSerializer<TimedLong>(true);

            var stream = new MemoryStream(new byte[] {
                0x12, 0x00, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0x78, 0x56, 0x00, 0x00 });
            TimedLong data = serializer.Deserialize(stream);

            data.tm.sec.Is(0x12);
            data.tm.nsec.Is(0x34);
            data.data.Is(0x5678);

            //ObjectDumper.Write(data);
        }
    }
}
