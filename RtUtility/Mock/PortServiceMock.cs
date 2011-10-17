using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRTM;
using omg.org.CORBA;
using omg.org.RTC;

namespace RtUtility.Mock
{
    public class OutPortServiceMock : PortServiceMock
    {
        private InPortCdr _inPortCdr;
        public OutPortServiceMock(PortProfile prof)
            : base(prof)
        {
        }

        public InPortCdr GetInPortCdr()
        {
            return _inPortCdr;
        }

        public override ReturnCode_t connect(ref ConnectorProfile connector_profile)
        {
            var ior = connector_profile.GetInPortIor();


            var orb = OrbServices.GetSingleton();

            _inPortCdr = (InPortCdr)orb.string_to_object(ior);

            return ReturnCode_t.RTC_OK;
        }
    }

    public class InPortServiceMock : PortServiceMock
    {
        private InPortCdr _inPortCdrMock;
        public InPortServiceMock(PortProfile prof, InPortCdr mock)
            :base(prof)
        {
            _inPortCdrMock = mock;
        }

        public override ReturnCode_t connect(ref ConnectorProfile connector_profile)
        {
            var orb = OrbServices.GetSingleton();
            var ior = orb.object_to_string(_inPortCdrMock);
            ConnectorProfileExtension.AddInPortIor(ref connector_profile, ior);

            return ReturnCode_t.RTC_OK;
        }
    }

    public abstract class PortServiceMock : MarshalByRefObject, PortService
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }
        private PortProfile _profile;

        public PortServiceMock(PortProfile prof)
        {
            _profile = prof;
        }

        public abstract ReturnCode_t connect(ref ConnectorProfile connector_profile);

        public ReturnCode_t disconnect(string connector_id)
        {
            return ReturnCode_t.RTC_OK;
        }

        public PortProfile get_port_profile()
        {
            return _profile;
        }

        #region NotImplementation
        public ReturnCode_t disconnect_all()
        {
            throw new NotImplementedException();
        }

        public ConnectorProfile get_connector_profile(string connector_id)
        {
            throw new NotImplementedException();
        }

        public ConnectorProfile[] get_connector_profiles()
        {
            throw new NotImplementedException();
        }


        public ReturnCode_t notify_connect(ref ConnectorProfile connector_profile)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t notify_disconnect(string connector_id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
