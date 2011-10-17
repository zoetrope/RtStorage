using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RtUtility
{
    // http://www.amazedsaint.com/2010/05/accessprivatewrapper-c-40-dynamic.html
    /// <summary>  
    /// A 10 minute wrapper to access private members, havn't tested in detail.  
    /// Use under your own risk - amazedsaint@gmail.com  
    /// </summary>  
    public class AccessPrivateWrapper : DynamicObject
    {

        /// <summary>  
        /// The object we are going to wrap  
        /// </summary>  
        object _wrapped;

        
        /// <summary>  
        /// Specify the flags for accessing members  
        /// </summary>  
        static BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.Public;

        // staticなコンストラクタとインスタンスコンストラクタの区別がつかないので、BindingFlags.Staticは外した。
        static BindingFlags cons_flags = BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Public;
        

        /// <summary>  
        /// Create a simple private wrapper  
        /// </summary>  
        public AccessPrivateWrapper(object o)
        {
            _wrapped = o;
        }

        /// <summary>  
        /// Create an instance via the constructor matching the args   
        /// </summary>  
        public static dynamic FromType(Assembly asm, string type, params object[] args)
        {

            var allt = asm.GetTypes();
            var t = allt.First(item => item.FullName == type);

            var newArgs = args.Select(arg => arg is AccessPrivateWrapper ?
                ((AccessPrivateWrapper)arg)._wrapped : arg);

            var types = newArgs.Select(arg => arg.GetType());
 
            
            //Gets the constructor matching the specified set of args  
            var ctor = t.GetConstructor(cons_flags, null, types.ToArray(), null);

            if (ctor != null)
            {
                var instance = ctor.Invoke(newArgs.ToArray());
                return new AccessPrivateWrapper(instance);
            }

            return null;
        }

        /// <summary>  
        /// Try invoking a method  
        /// </summary>  
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var newArgs = args.Select(arg => arg is AccessPrivateWrapper ?
                ((AccessPrivateWrapper)arg)._wrapped : arg);

            var types = newArgs.Select(arg => arg.GetType());

            var method = _wrapped.GetType().GetMethod
                (binder.Name, flags, null, types.ToArray(), null);

            if (method == null) return base.TryInvokeMember(binder, newArgs.ToArray(), out result);
            else
            {
                result = method.Invoke(_wrapped, newArgs.ToArray());
                return true;
            }
        }

        /// <summary>  
        /// Tries to get a property or field with the given name  
        /// </summary>  
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            //Try getting a property of that name  
            var prop = _wrapped.GetType().GetProperty(binder.Name, flags);

            if (prop == null)
            {
                //Try getting a field of that name  
                var fld = _wrapped.GetType().GetField(binder.Name, flags);
                if (fld != null)
                {
                    result = fld.GetValue(_wrapped);
                    return true;
                }
                else
                    return base.TryGetMember(binder, out result);
            }
            else
            {
                result = prop.GetValue(_wrapped, null);
                return true;
            }
        }

        /// <summary>  
        /// Tries to set a property or field with the given name  
        /// </summary>  
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var prop = _wrapped.GetType().GetProperty(binder.Name, flags);
            if (prop == null)
            {
                var fld = _wrapped.GetType().GetField(binder.Name, flags);
                if (fld != null)
                {
                    fld.SetValue(_wrapped, value);
                    return true;
                }
                else
                    return base.TrySetMember(binder, value);
            }
            else
            {
                prop.SetValue(_wrapped, value, null);
                return true;
            }
        }

    }  
}
