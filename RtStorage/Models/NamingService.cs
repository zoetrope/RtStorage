using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using RtUtility;

namespace RtStorage.Models
{
    public class NamingService : NotificationObject
    {
        
        public NamingService(INamingServiceClient client)
        {
            HostName = client.HostName;
            PortNumber = client.PortNumber;

            Client = client;

            Factory = new TreeViewItemFactory(Client);
        }

        public NamingService(string hostName, int portNumber)
        {
            HostName = hostName;
            PortNumber = portNumber;

            Client = new CorbaNamingServiceClient(hostName, portNumber);

            Factory = new TreeViewItemFactory(Client);
        }

        public INamingServiceClient Client { get; private set; }
        public TreeViewItemFactory Factory { get; private set; }



        #region IsAlive変更通知プロパティ
        private bool _isAlive;
        public bool IsAlive
        {
            get
            { return _isAlive; }
            set
            {
                if (_isAlive == value)
                    return;
                _isAlive = value;
                RaisePropertyChanged("IsAlive");
            }
        }
        #endregion

        #region HostName変更通知プロパティ
        private string _hostName;
        public string HostName
        {
            get
            { return _hostName; }
            set
            {
                if (_hostName == value)
                    return;
                _hostName = value;
                RaisePropertyChanged("HostName");
            }
        }
        #endregion

        #region PortNumber変更通知プロパティ
        private int _portNumber;
        public int PortNumber
        {
            get
            { return _portNumber; }
            set
            {
                if (_portNumber == value)
                    return;
                _portNumber = value;
                RaisePropertyChanged("PortNumber");
            }
        }
        #endregion


    }
}
