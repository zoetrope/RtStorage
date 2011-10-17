using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;

namespace RtUtility.Mock
{

   public class MockProxy<T> : RealProxy
   {
       // http://d.hatena.ne.jp/YokoKen/20080521/1211384842

       static MockProxy()
       {
           ErrorMap = new Dictionary<string, Exception>();
           WaitTime = TimeSpan.Zero;
       }


       /// <summary>
       /// 指定したメソッド名に対して特定の例外を発生させる。
       /// * を指定した場合は全メソッドで例外が発生する。
       /// 型パラメータごとに設定が可能。
       /// </summary>
       public static Dictionary<string, Exception> ErrorMap { get; private set; }

       /// <summary>
       /// メソッドを呼び出したときに一定時間待つ。
       /// </summary>
       public static TimeSpan WaitTime { get; set; }

       /// <summary>
       /// ラッピングするオブジェクトを取得します。
       /// </summary>
       public T TargetInstance
       {
           get;
           private set;
       }

       /// <summary>
       /// WrappingProxy&lt;T> クラスの新しいインスタンスを初期化します。
       /// </summary>
       /// <param name="targetInstance">ラッピングするオブジェクト。</param>
       public MockProxy(T targetInstance)
           : base(typeof(T))
       {
           TargetInstance = targetInstance;
       }

       /// <summary>
       /// 提供された System.Runtime.Remoting.Messaging.IMessage で指定されたメソッドを、
       /// ラッピングするオブジェクトで呼び出します。
       /// </summary>
       /// <param name="msg">メソッドの呼び出しに関する情報の System.Collections.IDictionary を格納している System.Runtime.Remoting.Messaging.IMessage。</param>
       /// <returns>呼び出されたメソッドが返すメッセージで、out パラメータまたは ref パラメータのどちらかと戻り値を格納しているメッセージ。</returns>
       public override IMessage Invoke(IMessage msg)
       {
           var methodCallMessage = (IMethodCallMessage)msg;
           MethodBase targetMethod = methodCallMessage.MethodBase;
           object[] args = methodCallMessage.Args;

           ReturnMessage returnMessage;

           // 通信遅延の模擬。
           Thread.Sleep(WaitTime);

           if (ErrorMap.ContainsKey("*"))
           {
               returnMessage = new ReturnMessage(ErrorMap["*"], methodCallMessage);
           }
           else if (ErrorMap.ContainsKey(targetMethod.Name))
           {
               returnMessage = new ReturnMessage(ErrorMap[targetMethod.Name], methodCallMessage);
           }
           else
           {
               try
               {
                   object invokeResult = targetMethod.Invoke(TargetInstance, args);
                   returnMessage = new ReturnMessage(invokeResult, args, args.Length,
                                                     methodCallMessage.LogicalCallContext, methodCallMessage);
               }
               catch (TargetInvocationException ex)
               {
                   returnMessage = new ReturnMessage(ex.InnerException, methodCallMessage);
               }
           }
           return returnMessage;
       }

       /// <summary>
       /// System.Runtime.Remoting.Proxies.RealProxy の現在のインスタンスの透過プロキシを返します。
       /// </summary>
       /// <returns>現在のプロキシ インスタンスの透過プロキシ。</returns>
       public new T GetTransparentProxy()
       {
           return (T)base.GetTransparentProxy();
       }

   }

}
