using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;

using RtStorage.Models;

namespace RtStorage.ViewModels
{
    public class SearchResultViewModel : ViewModel
    {
        private RecordDescription _recordDescription;
        private Action<RecordDescription> _notifier;
        public SearchResultViewModel(RecordDescription description, Action<RecordDescription> notifier)
        {
            _recordDescription = description;
            _notifier = notifier;
        }


        #region AdoptResultCommand
        ViewModelCommand _AdoptResultCommand;

        public ViewModelCommand AdoptResultCommand
        {
            get
            {
                if (_AdoptResultCommand == null)
                    _AdoptResultCommand = new ViewModelCommand(AdoptResult, CanAdoptResult);
                return _AdoptResultCommand;
            }
        }

        private bool CanAdoptResult()
        {
            return true;
        }

        private void AdoptResult()
        {
            _notifier(_recordDescription);
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
}
