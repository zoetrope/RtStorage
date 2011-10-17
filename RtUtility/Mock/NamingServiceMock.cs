using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using Ch.Elca.Iiop;
using omg.org.CosNaming.NamingContext_package;
using omg.org.RTC;

namespace RtUtility.Mock
{
    public class NamingServiceMock :MarshalByRefObject, INamingServiceClient
    {
        public Dictionary<string, RTObject> RtObjects { get; set; }

        public NamingServiceMock(string host, int port)
        {
            HostName = host;
            PortNumber = port;
            Key = HostName + ":" + PortNumber;
        }


        public string Key
        {
            get;
            set;
        }

        public string HostName
        {
            get;
            set;
        }

        public int PortNumber
        {
            get;
            set;
        }

        public void ClearZombie<TObject>()
        {
            return;
        }

        public void RegisterObject(string name, MarshalByRefObject obj)
        {
            throw new NotImplementedException();
        }

        public void UnregisterObject(string name)
        {
            throw new NotImplementedException();
        }

        public TObjectType GetObject<TObjectType>(string name) where TObjectType : class
        {
            if (typeof(TObjectType) == typeof(RTObject))
            {
                if (!RtObjects.ContainsKey(name))
                {
                    throw new NotFound();
                }

                return RtObjects[name] as TObjectType;
            }

            throw new NotFound();
        }

        public IEnumerable<string> GetObjectNames()
        {
            return RtObjects.Keys;
        }

        public bool IsA<TObject>(string name)
        {
            if (RtObjects.ContainsKey(name)) return true;
            
            return false;
        }

        public void Dispose()
        {
            return;
        }
    }
}
