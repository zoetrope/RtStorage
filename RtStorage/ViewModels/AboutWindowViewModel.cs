using System;
using System.Diagnostics;
using Livet;
using Livet.Commands;

namespace RtStorage.ViewModels
{
    public class AboutWindowViewModel : ViewModel
    {
        #region OpenUriCommand
        ListenerCommand<Uri> _OpenUriCommand;

        public ListenerCommand<Uri> OpenUriCommand
        {
            get
            {
                if (_OpenUriCommand == null)
                    _OpenUriCommand = new ListenerCommand<Uri>(OpenUri);
                return _OpenUriCommand;
            }
        }

        private void OpenUri(Uri parameter)
        {
            try
            {
                // Uriをブラウザで開く。
                Process.Start(parameter.ToString());
            }
            catch (Exception)
            {
                // 何かあってもしらね。
            }
        }
        #endregion
      
      
    }
}
