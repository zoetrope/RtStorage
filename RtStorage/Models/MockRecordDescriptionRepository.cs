using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RtStorage.Models
{
    
    // モックはここに置きたくないな・・・
    public class MockRecordDescriptionRepository :MarshalByRefObject, IRecordDescriptionRepository
    {
        private static readonly List<RecordDescription> _recordDescriptions = new List<RecordDescription>();

        public IEnumerable<RecordDescription> GetRecordDescriptions(SearchCondition param)
        {
            lock (_recordDescriptions)
            {
                var temp = _recordDescriptions.AsEnumerable();

                if (param.ComponentType != null)
                {
                    temp = temp.Where(x => x.ComponentType == param.ComponentType);
                }
                if (param.DataType != null)
                {
                    temp = temp.Where(x => x.DataType == param.DataType);
                }
                if (param.PortName != null)
                {
                    temp = temp.Where(x => x.PortName == param.PortName);
                }
                if (param.StartDateTime != null)
                {
                    temp = temp.Where(x => x.CreatedDateTime >= param.StartDateTime);
                }
                if (param.EndDateTime != null)
                {
                    temp = temp.Where(x => x.CreatedDateTime <= param.EndDateTime);
                }

                return temp;
            }
        }

        public IEnumerable<string> GetDataTypes(string componentType, string portName)
        {
            lock (_recordDescriptions)
            {
                var temp = _recordDescriptions.AsEnumerable();

                if (componentType != null)
                {
                    temp = temp.Where(x => x.ComponentType == componentType);
                }
                if (portName != null)
                {
                    temp = temp.Where(x => x.PortName == portName);
                }

                return temp.Select(x => x.DataType);
            }
        }

        public IEnumerable<string> GetComponentTypes(string dataType, string portName)
        {
            lock (_recordDescriptions)
            {
                var temp = _recordDescriptions.AsEnumerable();

                if (dataType != null)
                {
                    temp = temp.Where(x => x.DataType == dataType);
                }
                if (portName != null)
                {
                    temp = temp.Where(x => x.PortName == portName);
                }

                return temp.Select(x => x.ComponentType);
            }
        }

        public IEnumerable<string> GetPortNames(string dataType, string componentType)
        {
            lock (_recordDescriptions)
            {
                var temp = _recordDescriptions.AsEnumerable();

                if (dataType != null)
                {
                    temp = temp.Where(x => x.DataType == dataType);
                }
                if (componentType != null)
                {
                    temp = temp.Where(x => x.ComponentType == componentType);
                }

                return temp.Select(x => x.PortName);
            }
        }

        public void Insert(RecordDescription data)
        {
            lock (_recordDescriptions)
            {
                _recordDescriptions.Add(data);
            }
        }

        public void Clear()
        {
            lock (_recordDescriptions)
            {
                _recordDescriptions.Clear();
            }
        }
    }
}
