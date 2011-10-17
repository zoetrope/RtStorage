using System;
using System.Reactive.Linq;
using omg.org.RTC;
using System.Linq;

namespace RtUtility
{
    /// <summary>
    /// RTコンポーネントの操作を簡略化するための拡張メソッド
    /// </summary>
    public static class RTObjectExtension
    {
        public static ReturnCode_t Activate(this RTObject target, int execHandle)
        {
            return target.get_owned_contexts()[execHandle].activate_component(target);
        }

        public static ReturnCode_t Deactivate(this RTObject target, int execHandle)
        {
            return target.get_owned_contexts()[execHandle].deactivate_component(target);
        }

        public static ReturnCode_t Reset(this RTObject target, int execHandle)
        {
            return target.get_owned_contexts()[execHandle].reset_component(target);
        }

        public static ReturnCode_t SetExecutionRate(this RTObject target, int execHandle, double rate)
        {
            return target.get_owned_contexts()[execHandle].set_rate(rate);
        }

        public static double GetExecutionRate(this RTObject target, int execHandle)
        {
            return target.get_owned_contexts()[execHandle].get_rate();
        }

        public static LifeCycleState GetState(this RTObject target, int execHandle)
        {
            return target.get_owned_contexts()[execHandle].get_component_state(target);
        }

        public static PortService GetPort(this RTObject target, string name)
        {
            return target.get_ports().FirstOrDefault(x => x.get_port_profile().name == name);
        }

        public static ExecutionContext GetExecutionContext(this RTObject target, int execHandle)
        {
            return target.get_owned_contexts()[execHandle];
        }

        public static bool IsOwnExecutionContext(this RTObject target, int execHandle)
        {
            return true;
        }

        public static bool SetConfigurationValue(this RTObject target, string name, string value)
        {
            var conf = target.get_configuration();
            var set = conf.get_active_configuration_set();
            if (!set.configuration_data.Any(x=>x.name == name)) return false;
            set.configuration_data.SetStringValue(name, value);
            return conf.set_configuration_set_values(set);
        }

        public static bool AddConfigurationValue(this RTObject target, string name, string value)
        {
            var conf = target.get_configuration();
            var set = conf.get_active_configuration_set();
            if (set.configuration_data.Any(x => x.name == name)) return false;
            NameValueExtension.AddStringValue(ref set.configuration_data, name, value);
            return conf.set_configuration_set_values(set);
        }

        public static string GetConfigurationValue(this RTObject target, string name)
        {
            var conf = target.get_configuration();
            var set = conf.get_active_configuration_set();
            if (!set.configuration_data.Any(x => x.name == name)) return null;
            return set.configuration_data.GetStringValue(name);
        }


        public static bool WaitState(this RTObject target, LifeCycleState state, TimeSpan timeout)
        {
            Observable.Interval(TimeSpan.FromMilliseconds(500))
                .Select(_ => target.GetState(0))
                .Where(x => x == state)
                .Timeout(timeout)
                .First();

            // コンポーネント接続失敗
            // タイムアウト

            return true;

        }
    }
}
