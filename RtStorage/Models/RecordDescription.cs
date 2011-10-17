using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;

namespace RtStorage.Models
{

    public class RecordDescription : NotificationObject
    {
        #region Title変更通知プロパティ
        string _Title;

        public string Title
        {
            get
            { return _Title; }
            set
            {
                if (_Title == value)
                    return;
                _Title = value;
                RaisePropertyChanged("Title");
            }
        }
        #endregion

        #region CreatedDateTime変更通知プロパティ
        DateTime _createdDateTime;

        public DateTime CreatedDateTime
        {
            get
            { return _createdDateTime; }
            set
            {
                if (_createdDateTime == value)
                    return;
                _createdDateTime = value;
                RaisePropertyChanged("CreatedDateTime");
            }
        }
        #endregion

        #region TimeSpan変更通知プロパティ
        long _TimeSpan;

        public long TimeSpan
        {
            get
            { return _TimeSpan; }
            set
            {
                if (_TimeSpan == value)
                    return;
                _TimeSpan = value;
                RaisePropertyChanged("TimeSpan");
            }
        }
        #endregion

        #region NamingName変更通知プロパティ
        string _NamingName;

        public string NamingName
        {
            get
            { return _NamingName; }
            set
            {
                if (_NamingName == value)
                    return;
                _NamingName = value;
                RaisePropertyChanged("NamingName");
            }
        }
        #endregion

        #region ComponentType変更通知プロパティ
        string _ComponentType;

        public string ComponentType
        {
            get
            { return _ComponentType; }
            set
            {
                if (_ComponentType == value)
                    return;
                _ComponentType = value;
                RaisePropertyChanged("ComponentType");
            }
        }
        #endregion

        #region PortName変更通知プロパティ
        string _PortName;

        public string PortName
        {
            get
            { return _PortName; }
            set
            {
                if (_PortName == value)
                    return;
                _PortName = value;
                RaisePropertyChanged("PortName");
            }
        }
        #endregion

        #region DataType変更通知プロパティ
        string _DataType;

        public string DataType
        {
            get
            { return _DataType; }
            set
            {
                if (_DataType == value)
                    return;
                _DataType = value;
                RaisePropertyChanged("DataType");
            }
        }
        #endregion

        #region SumSize変更通知プロパティ
        long _SumSize;

        public long SumSize
        {
            get
            { return _SumSize; }
            set
            {
                if (_SumSize == value)
                    return;
                _SumSize = value;
                RaisePropertyChanged("SumSize");
            }
        }
        #endregion
        
        #region Count変更通知プロパティ
        long _Count;

        public long Count
        {
            get
            { return _Count; }
            set
            {
                if (_Count == value)
                    return;
                _Count = value;
                RaisePropertyChanged("Count");
            }
        }
        #endregion

        #region IsLittleEndian変更通知プロパティ
        long _IsLittleEndian;

        public long IsLittleEndian
        {
            get
            { return _IsLittleEndian; }
            set
            {
                if (_IsLittleEndian == value)
                    return;
                _IsLittleEndian = value;
                RaisePropertyChanged("IsLittleEndian");
            }
        }
        #endregion

        #region IndexFileName変更通知プロパティ
        string _IndexFileName;

        public string IndexFileName
        {
            get
            { return _IndexFileName; }
            set
            {
                if (_IndexFileName == value)
                    return;
                _IndexFileName = value;
                RaisePropertyChanged("IndexFileName");
            }
        }
        #endregion

        #region DataFileName変更通知プロパティ
        string _DataFileName;

        public string DataFileName
        {
            get
            { return _DataFileName; }
            set
            {
                if (_DataFileName == value)
                    return;
                _DataFileName = value;
                RaisePropertyChanged("DataFileName");
            }
        }
        #endregion
      
    }
}
