using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository
{
    /// <summary>
    /// 容器
    /// </summary>
    public static class RepositoryContainer
    {
        //#region Instance

        //private static Lazy<RepositoryContainer> _instance = new Lazy<RepositoryContainer>(() => new RepositoryContainer());
        //private static RepositoryContainer Instance
        //{
        //    get
        //    {
        //        return _instance.Value;
        //    }
        //}

        //#endregion

        //private RepositoryContainer()
        //{
        //    Repositorys = new Dictionary<Type, Lazy<object>>();
        //}

        /// <summary>
        /// 
        /// </summary>
        static RepositoryContainer()
        {
            Repositorys = new ConcurrentDictionary<string, Lazy<object>>();
        }

        private static ConcurrentDictionary<string, Lazy<object>> Repositorys { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public static void Register<T>(T service)
        {
            var t = typeof(T);
            var lazy = new Lazy<object>(() => service);

            Repositorys.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>()
            where T : new()
        {
            var t = typeof(T);
            var lazy = new Lazy<object>(() => new T());

            Repositorys.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        public static void Register<T>(Func<object> function)
        {
            var t = typeof(T);
            var lazy = new Lazy<object>(function);

            Repositorys.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
            where T : new()
        {
            var t = typeof(T);
            var k = GetKey(t);

            Lazy<object> repository;
            if (Repositorys.TryGetValue(k, out repository))
            {
                return (T)repository.Value;
            }
            else
            {
                //Repositorys[t] = new Lazy<object>(() => new T());
                var lazy = new Lazy<object>(() => new T());
                Repositorys.AddOrUpdate(k, lazy, (x, y) => lazy);

                return Resolve<T>();
                //throw new KeyNotFoundException(string.Format("Service not found for type '{0}'", typeof(T)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static string GetKey(Type t)
        {
            //return string.Concat(t.AssemblyQualifiedName, ",", t.FullName);
            return t.FullName;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="service"></param>
        //public static void Replace<T>(T service)
        //{
        //    var t = typeof(T);
        //    var lazy = new Lazy<object>(() => service);
        //    Repositorys.AddOrUpdate(t, lazy, (x, y) => lazy);
        //}
    }
}
