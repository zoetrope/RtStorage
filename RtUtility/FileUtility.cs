using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RtUtility
{
    public static class FileUtility
    {
        /// <summary>
        /// ファイル名に使えない文字が含まれていた場合"_"に置き換える
        /// </summary>
        /// <param name="s">元ファイル名</param>
        /// <returns>変換後ファイル名</returns>
        public static string ValidFileName(string s)
        {
            string valid = s;
            var invalidch = Path.GetInvalidFileNameChars();

            return invalidch.Aggregate(valid, (current, c) => current.Replace(c, '_'));
        }
    }
}
