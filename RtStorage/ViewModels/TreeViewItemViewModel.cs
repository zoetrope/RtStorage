using System.Collections.ObjectModel;
using Livet;

namespace RtStorage.ViewModels
{
    public class TreeViewItemViewModel : NotificationObject
    {
        public TreeViewItemViewModel()
        {
            Children = new ObservableCollection<TreeViewItemViewModel>();
            IsExpand = true;
        }

        public ObservableCollection<TreeViewItemViewModel> Children { get; private set; }

        public TreeViewItemViewModel Parent { get; set; }


        #region IsSelected変更通知プロパティ
        private bool _IsSelected;

        public bool IsSelected
        {
            get
            { return _IsSelected; }
            set
            {
                if (_IsSelected == value)
                    return;
                _IsSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        #endregion


        #region IsExpand変更通知プロパティ
        private bool _IsExpand;

        public bool IsExpand
        {
            get
            { return _IsExpand; }
            set
            {
                if (_IsExpand == value)
                    return;
                _IsExpand = value;
                RaisePropertyChanged("IsExpand");
            }
        }
        #endregion

        public string Name { get; set; }
      
    }
    public class NamingServiceItemViewModel : TreeViewItemViewModel
    {
        public NamingServiceItemViewModel()
        {
            Parent = null;
        }
        
    }
    
    public class ContextItemViewModel : TreeViewItemViewModel
    {
    }

    public class ComponentItemViewModel : TreeViewItemViewModel
    {
    }

    public class InPortItemViewModel : TreeViewItemViewModel
    {
        public string NamingName { get; set; }
        public string DataType { get; set; }
    }

    public class OutPortItemViewModel : TreeViewItemViewModel
    {
        public string NamingName { get; set; }
        public string DataType { get; set; }
    }
}
