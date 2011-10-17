using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using omg.org.RTC;
using org.omg.SDOPackage;

namespace RtUtility.Mock
{


    public class RTObjectMock : MarshalByRefObject, RTObject
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        private ComponentProfile _profile;
        private List<PortService> _ports;

        public RTObjectMock(ComponentProfile prof, List<PortService> ports)
        {
            _profile = prof;
            _ports = ports;

            var proxy = new MockProxy<ExecutionContextMock>(new ExecutionContextMock());
            _execs = new List<ExecutionContext>() { proxy.GetTransparentProxy() };

            State = LifeCycleState.INACTIVE_STATE;
        }

        public LifeCycleState State { get; set; }

        public ComponentProfile get_component_profile()
        {
            return _profile;
        }

        public PortService[] get_ports()
        {

            return _ports.ToArray();
        }

        private List<ExecutionContext> _execs;
        public ExecutionContext[] get_owned_contexts()
        {
            return _execs.ToArray();
        }

        #region NotImplementation

        public ReturnCode_t _exit()
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t _finalize()
        {
            throw new NotImplementedException();
        }

        public int attach_context(ExecutionContext exec_context)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t detach_context(int exec_handle)
        {
            throw new NotImplementedException();
        }

        public ExecutionContext get_context(int exec_handle)
        {
            throw new NotImplementedException();
        }

        public int get_context_handle(ExecutionContext cxt)
        {
            throw new NotImplementedException();
        }

        public ExecutionContext[] get_participating_contexts()
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t initialize()
        {
            throw new NotImplementedException();
        }

        public bool is_alive(ExecutionContext exec_context)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t on_aborting(int exec_handle)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t on_activated(int exec_handle)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t on_deactivated(int exec_handle)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t on_error(int exec_handle)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t on_finalize()
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t on_initialize()
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t on_reset(int exec_handle)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t on_shutdown(int exec_handle)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t on_startup(int exec_handle)
        {
            throw new NotImplementedException();
        }

        public Configuration get_configuration()
        {
            throw new NotImplementedException();
        }

        public DeviceProfile get_device_profile()
        {
            throw new NotImplementedException();
        }

        public Monitoring get_monitoring()
        {
            throw new NotImplementedException();
        }

        public Organization[] get_organizations()
        {
            throw new NotImplementedException();
        }

        public string get_sdo_id()
        {
            throw new NotImplementedException();
        }

        public SDOService get_sdo_service(string id)
        {
            throw new NotImplementedException();
        }

        public string get_sdo_type()
        {
            throw new NotImplementedException();
        }

        public ServiceProfile get_service_profile(string id)
        {
            throw new NotImplementedException();
        }

        public ServiceProfile[] get_service_profiles()
        {
            throw new NotImplementedException();
        }

        public object get_status(string nme)
        {
            throw new NotImplementedException();
        }

        public NameValue[] get_status_list()
        {
            throw new NotImplementedException();
        }

        public Organization[] get_owned_organizations()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
