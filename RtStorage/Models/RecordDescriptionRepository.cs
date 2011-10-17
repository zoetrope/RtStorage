using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Codeplex.Data;
using Livet;
using RtStorage.Properties;

namespace RtStorage.Models
{
    public class RecordDescriptionRepository : IRecordDescriptionRepository
    {

        private readonly string _connectionString;

        public RecordDescriptionRepository()
            :this(ConfigurationManager.ConnectionStrings["RecordDescriptions"].ConnectionString)
        {
        }

        public RecordDescriptionRepository(string connectionString)
        {
            var newConnectionString = connectionString.Replace("Data Source=", "Data Source=" + SettingHolder.BaseDirectory);

            _connectionString = newConnectionString;

        }
        
        public IEnumerable<RecordDescription> GetRecordDescriptions(SearchCondition param)
        {
            // 引数に応じて検索条件を組み立てる。*はなんでもあり。
            var conditions = new List<string>();

            if (!string.IsNullOrEmpty(param.DataType) && param.DataType!="*")
            {
                conditions.Add("DataType = @DataType");
            }
            if (!string.IsNullOrEmpty(param.ComponentType) && param.ComponentType != "*")
            {
                conditions.Add("ComponentType = @ComponentType");
            }
            if (!string.IsNullOrEmpty(param.PortName) && param.PortName != "*")
            {
                conditions.Add("PortName = @PortName");
            }
            if (param.StartDateTime.HasValue)
            {
                conditions.Add("CreatedDateTime >= @StartDateTime");
            }
            if (param.EndDateTime.HasValue)
            {
                conditions.Add("CreatedDateTime <= @EndDateTime");
            }

            string condition = "";

            if (conditions.Count != 0)
            {
                condition = "where " + string.Join(" and ", conditions);
            }

            var descs = DbExecutor.Select<RecordDescription>(
                new SQLiteConnection(_connectionString),
                @"select * from RecordDescriptions " + condition, param);

            return descs;
        }

        /// <summary>
        /// DataTypeの一覧を取得する。
        /// </summary>
        public IEnumerable<string> GetDataTypes(string componentType, string portName)
        {
            var conditions = new List<string>();

            if (!string.IsNullOrEmpty(componentType) && componentType != "*")
            {
                conditions.Add("ComponentType = @ComponentType");
            }
            if (!string.IsNullOrEmpty(portName) && portName != "*")
            {
                conditions.Add("PortName = @PortName");
            }
            string condition = "";

            if (conditions.Count != 0)
            {
                condition = "where " + string.Join(" and ", conditions);
            }
            var dataTypes = DbExecutor.ExecuteReader(
                new SQLiteConnection(_connectionString),
                @"select distinct DataType from RecordDescriptions " + condition,
                new { ComponentType = componentType, PortName = portName });

            return dataTypes.Select(dr => (string)dr["DataType"])
                .Concat(EnumerableEx.Return("*"));
        }

        /// <summary>
        /// ComponentTypeの一覧を取得する
        /// </summary>
        public IEnumerable<string> GetComponentTypes(string dataType, string portName)
        {
            var conditions = new List<string>();

            if (!string.IsNullOrEmpty(dataType) && dataType != "*")
            {
                conditions.Add("DataType = @DataType");
            }
            if (!string.IsNullOrEmpty(portName) && portName != "*")
            {
                conditions.Add("PortName = @PortName");
            }
            string condition = "";

            if (conditions.Count != 0)
            {
                condition = "where " + string.Join(" and ", conditions);
            }
            var componentTypes = DbExecutor.ExecuteReader(
                new SQLiteConnection(_connectionString),
                @"select distinct ComponentType from RecordDescriptions " + condition,
                new { DataType = dataType, PortName = portName });

            return componentTypes.Select(dr => (string)dr["ComponentType"])
                .Concat(EnumerableEx.Return("*"));
        }

        public IEnumerable<string> GetPortNames(string dataType, string componentType)
        {
            var conditions = new List<string>();

            if (!string.IsNullOrEmpty(dataType) && dataType != "*")
            {
                conditions.Add("DataType = @DataType");
            }
            if (!string.IsNullOrEmpty(componentType) && componentType != "*")
            {
                conditions.Add("ComponentType = @ComponentType");
            }
            string condition = "";

            if (conditions.Count != 0)
            {
                condition = "where " + string.Join(" and ", conditions);
            }
            var portNames = DbExecutor.ExecuteReader(
                new SQLiteConnection(_connectionString),
                @"select distinct PortName from RecordDescriptions " + condition,
                new { DataType = dataType, ComponentType = componentType });

            return portNames.Select(dr => (string)dr["PortName"])
                .Concat(EnumerableEx.Return("*"));
        }

        public void Insert(RecordDescription data)
        {
            // RecordDescriptionはNotificationObjectを継承しているので、そのままInsertすると
            // Livet.WeakEventListenerHolderがunknown sqlce typeと言われてしまう。
            // そこで、いったん匿名型に入れ替える必要がある。

            DbExecutor.Insert(new SQLiteConnection(_connectionString), "RecordDescriptions",
                new {data.Title, data.CreatedDateTime, data.TimeSpan, data.NamingName, data.ComponentType, 
                    data.PortName, data.DataType, data.SumSize, data.Count, data.IsLittleEndian,
                    data.IndexFileName, data.DataFileName });
        }

        /// <summary>
        /// データベースの内容を初期化する。テスト用？
        /// </summary>
        public void Clear()
        {
            DbExecutor.ExecuteNonQuery(
                new SQLiteConnection(_connectionString),
                @"delete from RecordDescriptions");

        }

    }


}