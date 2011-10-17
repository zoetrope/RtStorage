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

        /// <summary>
        /// 配列の要素をすべて削除する。
        /// ObservableCollection.Clearは、CollectionChangedイベントが発生しないので。
        /// スレッドセーフではない。
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/224155/when-clearing-an-observablecollection-there-are-no-items-in-e-olditems
        /// </remarks>
        public static void RemoveAll<T>(this ObservableCollection<T> list)
        {
            lock (list)
            {
                while (list.Count > 0)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }
        }

    }
}
