using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Livet;

namespace RtStorage.Models
{
    /// <summary>
    /// 保存したデータの解析を行うクラス
    /// </summary>
    public class Analyzer : NotificationObject
    {

        private readonly RecordDescription _recordDescription;

        public Analyzer(RecordDescription description)
        {
            _recordDescription = description;
            PartitionNumber = 10;
        }


        public IEnumerable<Histogram> CreateHistogram()
        {
            lock (_recordDescription)
            {
                var times = new List<long>();
                var positions = new List<long>();
                var sizes = new List<long>();

                var filePath = SettingHolder.BaseDirectory + _recordDescription.IndexFileName;

                using (var reader = new BinaryReader(File.OpenRead(filePath)))
                {
                    while (true)
                    {
                        try
                        {
                            times.Add(reader.ReadInt64());
                            positions.Add(reader.ReadInt64());
                            sizes.Add(reader.ReadInt64());

                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }

                    }
                }

                var max = _recordDescription.TimeSpan;

                var num = (double) max/PartitionNumber;



                var histogram = times
                    .GroupBy(x => Math.Ceiling(x/num))
                    .Select(x => new {Key = x.Key, Tally = x.LongCount()});

                var result = Enumerable.Range(1, PartitionNumber)
                    .Select(x => new Histogram(){
                        Key = (long) (x*num),
                        Tally = histogram.SingleOrDefault(y => y.Key == x) != null? histogram.Single(y => y.Key == x).Tally: 0
                    });

                return result;

            }
        }

        #region PartitionNumber変更通知プロパティ
        int _PartitionNumber;

        public int PartitionNumber
        {
            get
            { return _PartitionNumber; }
            set
            {
                if (_PartitionNumber == value)
                    return;
                _PartitionNumber = value;
                RaisePropertyChanged("PartitionNumber");
            }
        }
        #endregion


        #region RecordDescriptionのプロパティのラップ
        public string ComponentType
        {
            get { return _recordDescription.ComponentType; }
        }

        public long Count
        {
            get { return _recordDescription.Count; }
        }

        public DateTime CreatedDateTime
        {
            get { return _recordDescription.CreatedDateTime; }
        }

        public string DataType
        {
            get { return _recordDescription.DataType; }
        }

        public string NamingName
        {
            get { return _recordDescription.NamingName; }
        }

        public string PortName
        {
            get { return _recordDescription.PortName; }
        }

        public long SumSize
        {
            get { return _recordDescription.SumSize; }
        }

        public long TimeSpan
        {
            get { return _recordDescription.TimeSpan; }
        }

        public string Title
        {
            get { return _recordDescription.Title; }
        }

        #endregion

    }

    public class Histogram
    {
        public long Key { get; set; }
        public long Tally { get; set; }
    }
}
