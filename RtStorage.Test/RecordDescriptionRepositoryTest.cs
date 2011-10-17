using System;
using System.Data.SQLite;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Codeplex.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RtStorage.Models;

namespace RtStorage.Test
{
    [TestClass]
    public class RecordDescriptionRepositoryTest
    {
        RecordDescriptionRepository _repository;
        
        [TestInitialize]
        public void Initialize()
        {
            SettingHolder.BaseDirectory = @"..\..\TestData\";
            _repository = new RecordDescriptionRepository("Data Source=RecordDescriptions.db");

            _repository.Clear();

            var data = new List<RecordDescription>()
            {
                new RecordDescription(){
                    CreatedDateTime = DateTime.Now,
                    TimeSpan = 10000,
                    NamingName = "127.0.0.1:2809/ConsoleIn0.rtc",
                    ComponentType = "ConsoleIn",
                    PortName = "ConsoleIn0.out",
                    DataType = "IDL:RTC/TimedLong:1.0",
                    SumSize = 120,
                    Count = 10,
                    IsLittleEndian = 1,
                    IndexFileName = "Data/hoge.index",
                    DataFileName = "Data/hoge.data",
                },
                new RecordDescription(){
                    CreatedDateTime = DateTime.Now,
                    TimeSpan = 10000,
                    NamingName = "127.0.0.1:2809/SampleInOut0.rtc",
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
                    NamingName = "127.0.0.1:2809/SampleInOut0.rtc",
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
                    NamingName = "127.0.0.1:2809/SampleComp0.rtc",
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

            data.ForEach(_repository.Insert);
            
        }

        [TestMethod]
        public void キャストのチェック()
        {
            // SQLiteを1.0.76.0にアップデートしたら、IsLittleEndianをlongからboolにキャストしようとしてエラーになった。
            // SQLiteではboolは使えない。

            dynamic descs = DbExecutor.SelectDynamic(
                new SQLiteConnection("Data Source=../../TestData/RecordDescriptions.db"),
                @"select * from RecordDescriptions ").First();

            var data = new RecordDescription();

            data.ComponentType = descs.ComponentType;
            data.Count = descs.Count;
            data.CreatedDateTime = descs.CreatedDateTime;
            data.DataFileName = descs.DataFileName;
            data.DataType = descs.DataType;
            data.IndexFileName = descs.IndexFileName;
            data.IsLittleEndian = descs.IsLittleEndian;
            data.NamingName = descs.NamingName;
            data.PortName = descs.PortName;
            data.SumSize = descs.SumSize;
            data.TimeSpan = descs.TimeSpan;
            data.Title = descs.Title;

        }


        [TestMethod]
        public void GetRecordDescriptionsのテスト()
        {
            _repository.GetRecordDescriptions(new SearchCondition()).Count().Is(4);

            _repository.GetRecordDescriptions(new SearchCondition() { DataType = "IDL:RTC/TimedLong:1.0" }).Count().Is(1);

            _repository.GetRecordDescriptions(new SearchCondition() {
                DataType = "IDL:RTC/TimedLong:1.0", 
                PortName = "ConsoleIn0.out", 
                ComponentType = "ConsoleIn"
            }).Count().Is(1);

            var start = DateTime.Now.AddYears(-1);
            var end = DateTime.Now.AddYears(1);

            _repository.GetRecordDescriptions(new SearchCondition() {
                DataType = "IDL:RTC/TimedLong:1.0", 
                PortName = "ConsoleIn0.out", 
                ComponentType = "ConsoleIn",
                StartDateTime = start,
                EndDateTime = end
            }).Count().Is(1);


            _repository.GetRecordDescriptions(new SearchCondition()
            {
                DataType = "IDL:RTC/TimedLong:1.0",
                PortName = "ConsoleIn0.out",
                ComponentType = "ConsoleIn",
                StartDateTime = end,
                EndDateTime = start
            }).Count().Is(0);

            _repository.GetRecordDescriptions(new SearchCondition() { DataType = "xxxxxxxxx" }).Count().Is(0);
        }

        [TestMethod]
        public void GetDataTypesのテスト()
        {
            _repository.GetDataTypes(null, null)
                .Is(new List<string> { "IDL:RTC/TimedLong:1.0", "IDL:RTC/TimedDouble:1.0", "IDL:RTC/TimedString:1.0", });

            _repository.GetDataTypes("SampleInOut", null)
             .Is(new List<string> { "IDL:RTC/TimedDouble:1.0" });

            _repository.GetDataTypes("SampleComp", "SampleComp0.out")
             .Is(new List<string> { "IDL:RTC/TimedString:1.0" });
        }



    }
}
