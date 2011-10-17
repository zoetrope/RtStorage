using System;
using System.Collections.Generic;
using System.Linq;
using omg.org.CORBA;
using org.omg.SDOPackage;

namespace RtUtility
{
    public static class NameValueExtension
    {
        public static string GetStringValue(this NameValue[] target, string name)
        {
            var val = target.First(x => x.name == name).value;
            if (val is Any)
            {
                var any = (Any)val;
                if (any.Type.ToString() == "omg.org.CORBA.StringTC")
                {
                    return any.Value as string;
                }
                throw new InvalidCastException();
            }

            if (val is string)
            {
                return (string)val;
            }

            throw new InvalidCastException();
        }

        public static void SetStringValue(this NameValue[] target, string name, string value)
        {
            var stringTC = OrbServices.GetSingleton().create_string_tc(0);
            for (int i = 0; i < target.Length; i++)
            {
                if (target[i].name == name)
                {
                    target[i].value = new Any(value, stringTC);
                }
            }
        }

        public static bool ContainsKey(this NameValue[] target, string name)
        {
            return target.Any(nv => nv.name == name);
        }

        public static NameValue Create(string name, string value)
        {
            var stringTC = OrbServices.GetSingleton().create_string_tc(0);

            var item = new NameValue();
            item.name = name;
            item.value = new Any(value, stringTC);

            return item;
        }

        public static void AddStringValue(ref NameValue[] target, string name, string value)
        {
            var list = target == null ? new List<NameValue>() : target.ToList();
            list.Add(Create(name, value));
            target = list.ToArray();
        }
    }
}
