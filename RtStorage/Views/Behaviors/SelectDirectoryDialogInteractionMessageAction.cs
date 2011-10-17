using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Livet.Behaviors.Messaging;
using Livet.Messaging;
using RtStorage.Views.Messages;

namespace RtStorage.Views.Behaviors
{
    public class SelectDirectoryDialogInteractionMessageAction : InteractionMessageAction<DependencyObject>
    {
        protected override void InvokeAction(InteractionMessage m)
        {
            var message = m as DirectorySelectionMessage;
            
            if (message != null)
            {
                //bool isRelative = false;
                string path = message.SelectedPath;

                if (!Path.IsPathRooted(path)) // 相対パスの場合、絶対パスに変換
                {
                    //isRelative = true;
                    // 現在の作業ディレクトリを退避
                    var temp = Environment.CurrentDirectory;
                    
                    // 相対パスから絶対パスに変換
                    Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    path = Path.GetFullPath(message.SelectedPath);
                    
                    // 退避しておいたパスを戻す
                    Environment.CurrentDirectory = temp;
                }

                using (var dialog = new FolderBrowserDialog() {
                    SelectedPath = path,
                    Description = message.Description })
                {
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        
//                        if (isRelative)
//                        {
                              //もとが相対パスだった場合は相対パスにして返したい。
//                            var baseUri = new Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                        //エンコーディングの必要あり。このままだと日本語が化ける。
                        //HttpUtility.UrlDecode(relativePath).Replace('/', '\\');はClient Profileで使えない
//                            message.Response = @".\" + baseUri.MakeRelativeUri(new Uri(dialog.SelectedPath)).ToString().Replace("/", @"\");
//                        }
//                        else
                        {
                            message.Response = dialog.SelectedPath;
                        }
                        
                    }
                }
            }
        }
    }
}
