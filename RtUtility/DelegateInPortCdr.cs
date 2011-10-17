using System;
using OpenRTM;

namespace RtUtility
{
    /// <summary>
    /// put処理を移譲するInPortCdr
    /// </summary>
    public class DelegateInPortCdr : MarshalByRefObject, InPortCdr
    {
        public override object InitializeLifetimeService()
        {
            // CORBAメソッドがタイムアウトしないようにnullを返す
            return null;
        }

        private readonly Func<Byte[], PortStatus> _action;


        public DelegateInPortCdr(Func<Byte[], PortStatus> action)
        {
            _action = action;
        }

        public PortStatus put(byte[] data)
        {
            return _action(data);
        }
    }
}
