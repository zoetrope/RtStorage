using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using omg.org.RTC;

namespace RtUtility.Mock
{

    public class ExecutionContextMock : MarshalByRefObject, ExecutionContext
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }
        public LifeCycleState get_component_state(LightweightRTObject comp)
        {
            var mock = comp as RTObjectMock;
            return mock.State;
        }

        public ReturnCode_t activate_component(LightweightRTObject comp)
        {
            var mock = comp as RTObjectMock;

            if (mock.State == LifeCycleState.INACTIVE_STATE)
            {
                mock.State = LifeCycleState.ACTIVE_STATE;
                return ReturnCode_t.RTC_OK;
            }
            else
            {
                return ReturnCode_t.PRECONDITION_NOT_MET;
            }
        }

        public ReturnCode_t deactivate_component(LightweightRTObject comp)
        {
            var mock = comp as RTObjectMock;

            if (mock.State == LifeCycleState.ACTIVE_STATE)
            {
                mock.State = LifeCycleState.INACTIVE_STATE;
                return ReturnCode_t.RTC_OK;
            }
            else
            {
                return ReturnCode_t.PRECONDITION_NOT_MET;
            }
        }

        public ReturnCode_t reset_component(LightweightRTObject comp)
        {
            var mock = comp as RTObjectMock;

            if (mock.State == LifeCycleState.ERROR_STATE)
            {
                mock.State = LifeCycleState.INACTIVE_STATE;
                return ReturnCode_t.RTC_OK;
            }
            else
            {
                return ReturnCode_t.PRECONDITION_NOT_MET;
            }
        }

        #region NotImplementation
        public bool is_running()
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t start()
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t _stop()
        {
            throw new NotImplementedException();
        }

        public double get_rate()
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t set_rate(double rate)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t add_component(LightweightRTObject comp)
        {
            throw new NotImplementedException();
        }

        public ReturnCode_t remove_component(LightweightRTObject comp)
        {
            throw new NotImplementedException();
        }

        public ExecutionKind get_kind()
        {
            throw new NotImplementedException();
        }
        #endregion NotImplementation
    }
}
