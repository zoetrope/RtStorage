using System;
using System.Linq;
using System.Runtime.Remoting.Channels;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;

namespace RtUtility
{
    public static class CorbaUtility
    {
        private static IiopChannel _channel;
        private static readonly object _channelLock = new object();
        public static void Initialize()
        {
            lock (_channelLock)
            {
                if (_channel == null)
                {
                    // チャンネルを登録する
                    _channel = new IiopChannel(0); // 自動的にポートを割り当てる
                    ChannelServices.RegisterChannel(_channel, false);
                }

            }
        }

        public static void Destroy()
        {
            ChannelServices.UnregisterChannel(_channel);

            _channel = null;
        }

        public static string GetIor(object obj)
        {
            OrbServices orb = OrbServices.GetSingleton();

            return orb.object_to_string(obj);
        }

        public static string GetRepositoryID(Type type)
        {
            return type.GetCustomAttributes(typeof(RepositoryIDAttribute), true)
                .Cast<RepositoryIDAttribute>().First().Id;
        }
    }
}
