using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;
using omg.org.CosNaming.NamingContext_package;

namespace RtUtility
{
    /// <summary>
    /// CORBAのネーミングサービスを利用するためのクラス
    /// </summary>
    public class CorbaNamingServiceClient : INamingServiceClient
    {
        private readonly NamingContext _rootContext;

        /// <summary>
        ///   <see cref="CorbaNamingServiceClient"/>のインスタンス生成<br/>  
        ///   ホスト名:"localhost"、ポート番号:2809 でインスタンスを生成する
        /// </summary>
        public CorbaNamingServiceClient()
            : this("localhost", 2809)
        {
        }

        /// <summary>
        ///   <see cref="CorbaNamingServiceClient"/>のインスタンス生成
        /// </summary>
        /// <param name="host">ホスト名</param>
        /// <param name="port">ポート番号</param>
        public CorbaNamingServiceClient(string host, int port)
        {
            // ネーミングサービスの参照を取得する
            CorbaInit init = CorbaInit.GetInit();
            _rootContext = init.GetNameService(host, port);

            NameDelimiter = '.';
            TreeDelimiter = '/';

            HostName = host;
            PortNumber = port;
            Key = HostName + ":" + PortNumber;
        }

        public string Key
        {
            get;
            private set;
        }

        public string HostName
        {
            get;
            private set;
        }

        public int PortNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Dispose()
        {
            try
            {
                _rootContext.destroy();
            }
            catch (Exception)
            {
                // IIOP.NETのネーミングサービスは、destroyがnot implementedなので無視する。
                //throw ex;
            }

        }


        /// <summary>
        /// 指定した型の無効なオブジェクトを削除する
        /// </summary>
        public void ClearZombie<TObject>()
        {
            foreach (string name in GetObjectNames())
            {
                MarshalByRefObject obj;
                try
                {
                    obj = _rootContext.resolve(ToName(name));
                }
                catch (NotFound)
                {
                    // Refresh中にUnbindされることがある。
                    continue;
                }

                try
                {
                    var orb = omg.org.CORBA.OrbServices.GetSingleton();
                    if (!orb.is_a(obj, typeof(TObject)))
                    {
                        // TObjectではない
                        continue;
                    }
                }
                catch (omg.org.CORBA.TRANSIENT)
                {
                    // ゾンビ
                    try
                    {
                        // 有効ではないTObjectは、NamingServiceから削除
                        UnregisterObject(name);
                    }
                    catch (NotFound)
                    {
                        // Refresh中にUnbindされることがある。
                    }
                }

            }
        }

        /// <summary>
        /// コンポーネントをネーミングサービスに登録する
        /// </summary>
        /// <param name="name">登録する名前</param>
        /// <param name="obj">登録するコンポーネントの参照</param>
        public void RegisterObject(string name, MarshalByRefObject obj)
        {
            Rebind(ToName(name), obj);
        }

        private void Rebind(NameComponent[] name, MarshalByRefObject obj)
        {
            try
            {
                _rootContext.rebind(name, obj);
            }
            catch (NotFound)
            {
                RebindRecursive(_rootContext, name, obj);

            }
            catch (CannotProceed ex)
            {
                RebindRecursive(ex.cxt, ex.rest_of_name, obj);

            }
        }

        private void RebindRecursive(NamingContext context, NameComponent[] name, MarshalByRefObject obj)
        {
            int len = name.Length;
            NamingContext cxt = context;

            for (int i = 0; i < len; i++)
            {
                if (i == len - 1)
                {
                    var objectName = new[] { name[len - 1] };
                    Rebind(objectName, obj);
                }
                else
                {
                    if (IsNamingContext((MarshalByRefObject)cxt))
                    {
                        var contextName = new[] { name[i] };
                        try
                        {
                            cxt = cxt.bind_new_context(contextName);
                        }
                        catch (AlreadyBound)
                        {
                            cxt = (NamingContext)cxt.resolve(contextName);
                        }
                    }
                    else
                    {
                        throw new CannotProceed {cxt = cxt, rest_of_name = SubName(name, i)};
                    }


                }
            }

        }

        /// <summary>
        /// ネーミングサービスに登録されているオブジェクトを削除する
        /// </summary>
        /// <param name="name">登録されている名前</param>
        public void UnregisterObject(string name)
        {
            _rootContext.unbind(ToName(name));
        }

        
        /// <summary>
        ///   名前指定でコンポーネントの取得
        /// </summary>
        /// <param name="name">コンポーネントの名前</param>
        /// <returns>取得したコンポーネント</returns>
        public TObjectType GetObject<TObjectType>(string name) where TObjectType : class
        {
            var obj = _rootContext.resolve(ToName(name));

            omg.org.CORBA.OrbServices orb = omg.org.CORBA.OrbServices.GetSingleton();
            if (!orb.is_a(obj, typeof (TObjectType)))
            {
                // TObjectTypeではない
                throw new InvalidCastException(typeof (TObjectType).FullName + "にキャストできません。");
            }

            return obj as TObjectType;
        }

        /// <summary>
        ///   ネーミングサービスに登録されているオブジェクト名一覧の取得
        /// </summary>
        public IEnumerable<string> GetObjectNames()
        {
            return GetNameTreeRecursive(_rootContext, "");
        }


        /// <summary>
        /// 指定した名前のオブジェクトがTObject型かどうかを判断する
        /// </summary>
        public bool IsA<TObject>(string name)
        {
            var obj = _rootContext.resolve(ToName(name));

            try
            {
                omg.org.CORBA.OrbServices orb = omg.org.CORBA.OrbServices.GetSingleton();
                if (!orb.is_a(obj, typeof(TObject)))
                {
                    return false;
                }
            }
            catch (omg.org.CORBA.TRANSIENT)
            {
                return false;
            }

            return true;
        }

        private IEnumerable<string> GetNameTreeRecursive(NamingContext context, string name)
        {
            const int lote = 10;
            Binding[] bindList;
            BindingIterator bindIter;

            // 現在の階層に登録されているコンテキストをlote個ずつ取得する
            context.list(lote, out bindList, out bindIter);

            do
            {
                for (int i = 0; i < bindList.Length; i++)
                {
                    string newName = string.Copy(name);

                    // IDとKINDを'.'で区切る。
                    newName += bindList[i].binding_name[0].id;
                    newName += NameDelimiter;
                    newName += bindList[i].binding_name[0].kind;

                    if (bindList[i].binding_type == BindingType.ncontext)
                    {
                        // バインドされているものがコンテキストでない場合は
                        // さらに下の階層がある。

                        // 階層を'/'で区切る
                        newName += TreeDelimiter;

                        // 一つ下の階層のネーミングコンテキストを取得する
                        MarshalByRefObject obj = context.resolve(bindList[i].binding_name);
                        NamingContext nc = (NamingContext) obj;

                        // 次の階層へ
                        foreach (var n in GetNameTreeRecursive(nc, newName))
                        {
                            yield return n;
                        }
                    }
                    else
                    {
                        // これより下の階層はないので、現在の名前を返す
                        yield return newName;
                    }
                }
            } while ((bindIter != null) && bindIter.next_n(lote, out bindList));

            // 後片付け
            if (bindIter != null)
            {
                bindIter.destroy();
            }
        }

        /// <summary>
        /// オブジェクトの名前を文字列で表現するときのIDとKINDの区切り文字。
        /// デフォルトでは'.'
        /// </summary>
        public char NameDelimiter { get; set; }

        /// <summary>
        /// オブジェクトの名前を文字列で表現するときの階層の区切り文字。
        /// デフォルトでは'/'
        /// </summary>
        public char TreeDelimiter { get; set; }


        private NameComponent[] ToName(string stringName)
        {
            if (stringName == string.Empty)
            {
                throw new InvalidName("stringName is empty.");
            }

            var delim0 = new[] { TreeDelimiter };
            var delim1 = new[] { NameDelimiter };

            string[] subcol = stringName.Split(delim0);
            var context = new NameComponent[subcol.Length];
            int index = 0;

            foreach (string sub in subcol)
            {
                string[] subsubcol = sub.Split(delim1);
                if (subsubcol.Length == 2)
                {
                    context[index++] = new NameComponent(subsubcol[0], subsubcol[1]);
                }
                else
                {
                    context[index++] = new NameComponent(sub, "");
                }
            }

            return context;
        }

        private bool IsNamingContext(MarshalByRefObject obj)
        {
            var nc = obj as NamingContext;
            if (nc == null)
            {
                return false;
            }
            return true;
        }
        
        private NameComponent[] SubName(NameComponent[] name, int begin, int end = -1)
        {
            if (end < 0)
            {
                end = name.Length;
            }
            NameComponent[] subName;
            int subLen = end - begin - 1;
            if (subLen > 0)
            {
                subName = new NameComponent[subLen];
            }
            else
            {
                subName = new NameComponent[0];
                return subName;
            }
            for (int i = 0; i < subLen; i++)
            {
                subName[i] = name[begin + i];
            }
            return subName;
        }
    }
    
}
