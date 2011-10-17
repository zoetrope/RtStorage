using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using omg.org.RTC;

namespace RtUtility
{
    
    public enum PortType
    {
        DataInPort,
        DataOutPort,
        ServiceProvider,
        ServiceConsumer
    }

    public static class PortProfileExtension
    {
        public static string GetDataType(this PortProfile target)
        {
            var portType = target.properties.GetStringValue("port.port_type");

            switch (portType)
            {
                case "DataInPort":
                    return target.properties.GetStringValue("dataport.data_type");
                case "DataOutPort":
                    return target.properties.GetStringValue("dataport.data_type");
                case "CorbaPort":
                    return target.interfaces.First().type_name;
                default:
                    throw new ArgumentException();
            }
        }


        public static PortType GetPortType(this PortProfile target)
        {
            var portType = target.properties.GetStringValue("port.port_type");

            switch (portType)
            {
                case "DataInPort":
                    return PortType.DataInPort;
                case "DataOutPort":
                    return PortType.DataOutPort;
                case "CorbaPort":
                    switch (target.interfaces.First().polarity)
                    {
                        case PortInterfacePolarity.PROVIDED:
                            return PortType.ServiceProvider;
                        case PortInterfacePolarity.REQUIRED:
                            return PortType.ServiceConsumer;
                        default:
                            throw new ArgumentException();
                    }
                default:
                    throw new ArgumentException();
            }
        }

        public static PortType GetPartnerType(PortProfile profile)
        {
            PortType type = profile.GetPortType();

            switch (type)
            {
                case PortType.DataInPort:
                    return PortType.DataOutPort;

                case PortType.DataOutPort:
                    return PortType.DataInPort;

                case PortType.ServiceProvider:
                    return PortType.ServiceConsumer;

                case PortType.ServiceConsumer:
                    return PortType.ServiceProvider;
            }

            throw new ArgumentException();
        }

    }
}

