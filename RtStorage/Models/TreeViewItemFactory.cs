using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RtStorage.ViewModels;
using RtUtility;
using omg.org.CORBA;
using omg.org.RTC;

namespace RtStorage.Models
{
    //TODO:仮実装。見直しが必要。
    // ModelがViewModelを生成するのはいやだな。
    public class TreeViewItemFactory
    {

        public INamingServiceClient Client { get; private set; }

        public TreeViewItemFactory(INamingServiceClient client)
        {
            Client = client;
        }

        public NamingServiceItemViewModel CreateOutPortTree()
        {
            var tree = new NamingServiceItemViewModel()
            {
                Name = Client.Key
            };

            foreach (var rtc in Client.GetObjectNames().Where(Client.IsA<RTObject>))
            {
                try
                {
                    if (HasOutPort(rtc))
                    {
                        var names = rtc.Split('/');
                        RecursiveCreateNamingServiceTree(PortType.DataOutPort, tree, names);
                    }

                }
                catch (TRANSIENT)
                {
                    // 接続に失敗したコンポーネントはツリーに追加しない
                    continue;
                }
            }

            return tree;
        }

        public NamingServiceItemViewModel CreateInPortTree()
        {
            var tree = new NamingServiceItemViewModel()
            {
                Name = Client.Key
            };

            foreach (var rtc in Client.GetObjectNames().Where(Client.IsA<RTObject>))
            {
                try
                {
                    if (HasInPort(rtc))
                    {
                        var names = rtc.Split('/');
                        RecursiveCreateNamingServiceTree(PortType.DataInPort, tree, names);
                    }
                }
                catch (TRANSIENT)
                {
                    // 接続に失敗したコンポーネントはツリーに追加しない
                    continue;
                }
            }
            return  tree;
        }

        private bool HasOutPort(string name)
        {
            var comp = Client.GetObject<RTObject>(name);

            return comp.get_ports()
                .Any(port => port.get_port_profile().properties.GetStringValue("port.port_type") == "DataOutPort");
        }

        private bool HasInPort(string name)
        {
            var comp = Client.GetObject<RTObject>(name);

            return comp.get_ports()
                .Any(port => port.get_port_profile().properties.GetStringValue("port.port_type") == "DataInPort");
        }

        private string GetComponentName(TreeViewItemViewModel tree)
        {
            if (tree.Parent is NamingServiceItemViewModel)
            {
                return tree.Name;
            }
            else
            {
                return GetComponentName(tree.Parent) + "/" + tree.Name;
            }
        }

        private string GetNamingName(TreeViewItemViewModel tree)
        {
            if (tree.Parent == null)
            {
                return tree.Name;
            }
            else
            {
                return GetNamingName(tree.Parent) + "/" + tree.Name;
            }
        }

        private void RecursiveCreateNamingServiceTree(PortType type, TreeViewItemViewModel tree, IEnumerable<string> names)
        {
            Debug.Assert(names != null, "names != null");

            if (names.Count() == 1)
            {
                var item = new ComponentItemViewModel()
                {
                    Name = names.First(),
                    Parent = tree
                };
                var compName = GetComponentName(item);

                ObservableCollection<PortService> portNames;
                switch (type)
                {
                    case PortType.DataInPort:
                        portNames = GetInPorts(compName);
                        break;
                    case PortType.DataOutPort:
                        portNames = GetOutPorts(compName);
                        break;
                    default:
                        return;
                }


                foreach (var portName in portNames)
                {
                    var prof = portName.get_port_profile();


                    switch (type)
                    {
                        case PortType.DataInPort:
                            var inport = new InPortItemViewModel()
                            {
                                Name = prof.name,
                                NamingName = GetNamingName(item),
                                DataType = prof.GetDataType(),
                                Parent = item
                            };
                            item.Children.Add(inport);
                            break;
                        case PortType.DataOutPort:
                            var outport = new OutPortItemViewModel()
                            {
                                Name = prof.name,
                                NamingName = GetNamingName(item),
                                DataType = prof.GetDataType(),
                                Parent = item
                            };
                            item.Children.Add(outport);
                            break;
                        default:
                            return;
                    }
                }

                tree.Children.Add(item);
            }
            else
            {
                var item = new ContextItemViewModel()
                {
                    Name = names.First(),
                    Parent = tree
                };

                tree.Children.Add(item);
                RecursiveCreateNamingServiceTree(type, item, names.Skip(1));
            }
        }


        public ObservableCollection<string> GetComponents()
        {
            var ret = Client.GetObjectNames().Where(Client.IsA<RTObject>);
            return new ObservableCollection<string>(ret);
        }

        public ObservableCollection<PortService> GetInPorts(string componentName)
        {
            var comp = Client.GetObject<RTObject>(componentName);
            var ports = comp.get_ports()
                .Where(p => p.get_port_profile().GetPortType() == PortType.DataInPort);

            return new ObservableCollection<PortService>(ports);
        }

        public ObservableCollection<PortService> GetOutPorts(string componentName)
        {
            var comp = Client.GetObject<RTObject>(componentName);
            var ports = comp.get_ports()
                .Where(p => p.get_port_profile().GetPortType() == PortType.DataOutPort);

            return new ObservableCollection<PortService>(ports);
        }

        public ObservableCollection<PortService> GetPorts(string componentName)
        {
            var comp = Client.GetObject<RTObject>(componentName);
            var ports = comp.get_ports();

            return new ObservableCollection<PortService>(ports);
        }

        public PortService GetPort(string componentName, string portName)
        {
            var comp = Client.GetObject<RTObject>(componentName);
            return comp.get_ports().Where(p => p.get_port_profile().name == portName).FirstOrDefault();
        }
    }
}
