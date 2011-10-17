using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RtStorage.Models
{
    public class SettingHolder
    {
        private static string _baseDirectory = @".\Data\";

        public static string BaseDirectory
        {
            get { return _baseDirectory; }
            set
            {
                var dir = Environment.ExpandEnvironmentVariables(value);
                // ディレクトリ名の末尾に区切り文字(/)がついていない場合は勝手につける。
                if (dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    _baseDirectory = dir;
                }
                else
                {
                    _baseDirectory = dir + Path.DirectorySeparatorChar;
                }
            }
        }
    }
}
