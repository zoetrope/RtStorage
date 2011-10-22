using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace RtUtility
{
    public static class ObservableExtension
    {
        /// <summary>
        /// TException型の例外をキャッチする。
        /// 毎回Observable.Emptyを返すのが面倒なので。
        /// </summary>
        /// <remarks>
        /// http://neue.cc/2009/11/29_226.html
        /// </remarks>
        public static IObservable<TSource> Catch<TSource, TException>(this IObservable<TSource> source,
            Action<TException> handler) where TException : Exception
        {
            return source.Catch((TException e) =>
            {
                handler(e);
                return Observable.Empty<TSource>();
            });
        }


    }
}
