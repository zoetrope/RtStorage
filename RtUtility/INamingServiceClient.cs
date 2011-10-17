using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RtUtility
{
    public interface INamingServiceClient : IDisposable
    {
        string Key { get; }
        string HostName { get; }
        int PortNumber { get; }


        /// <summary>
        /// 指定した型の無効なオブジェクトを削除する
        /// </summary>
        void ClearZombie<TObject>();

        /// <summary>
        /// コンポーネントをネーミングサービスに登録する
        /// </summary>
        /// <param name="name">登録する名前</param>
        /// <param name="obj">登録するコンポーネントの参照</param>
        void RegisterObject(string name, MarshalByRefObject obj);

        /// <summary>
        /// ネーミングサービスに登録されているオブジェクトを削除する
        /// </summary>
        /// <param name="name">登録されている名前</param>
        void UnregisterObject(string name);

        /// <summary>
        ///   名前指定でコンポーネントの取得
        /// </summary>
        /// <param name="name">コンポーネントの名前</param>
        /// <returns>取得したコンポーネント</returns>
        TObjectType GetObject<TObjectType>(string name) where TObjectType : class;

        /// <summary>
        ///   ネーミングサービスに登録されているオブジェクト名一覧の取得
        /// </summary>
        /// <returns>オブジェクト名一覧</returns>
        IEnumerable<string> GetObjectNames();

        /// <summary>
        /// 指定した名前のオブジェクトがTObject型かどうかを判断する
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="name">オブジェクトの名前</param>
        /// <returns></returns>
        bool IsA<TObject>(string name);
    }

}
