using System;

namespace RtUtility
{
    public static class StringExtension
    {
        // 大文字小文字を区別しないString.Contains
        // http://ppetrov.wordpress.com/2008/06/27/useful-method-6-of-n-ignore-case-on-stringcontains/
        public static bool Contains(this string original, string value, StringComparison comparisionType)
        {
            return original.IndexOf(value, comparisionType) >= 0;
        }
    }
}
