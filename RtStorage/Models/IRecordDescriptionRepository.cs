using System;
using System.Collections.Generic;

namespace RtStorage.Models
{
    public interface IRecordDescriptionRepository
    {
        IEnumerable<RecordDescription> GetRecordDescriptions(SearchCondition param);

        /// <summary>
        /// DataTypeの一覧を取得する。
        /// </summary>
        IEnumerable<string> GetDataTypes(string componentType, string portName);

        /// <summary>
        /// ComponentTypeの一覧を取得する
        /// </summary>
        IEnumerable<string> GetComponentTypes(string dataType, string portName);

        IEnumerable<string> GetPortNames(string dataType, string componentType);
        void Insert(RecordDescription data);

        /// <summary>
        /// データベースの内容を初期化する。
        /// </summary>
        void Clear();

    }

    /// <summary>
    /// Selectメソッドで検索するときの条件。
    /// nullに設定したフィールドは検索条件に含めない
    /// </summary>
    public class SearchCondition
    {
        public SearchCondition()
        {
            DataType = null;
            ComponentType = null;
            PortName = null;
            StartDateTime = null;
            EndDateTime = null;
        }

        public string DataType { get; set; }
        public string ComponentType { get; set; }
        public string PortName { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }
}