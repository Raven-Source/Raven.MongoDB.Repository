using System;
using System.Collections.Concurrent;

#if MongoDB_Repository
namespace MongoDB.Repository
#else
namespace Raven.MongoDB.Repository
#endif
{
    /// <summary>
    /// 容器
    /// </summary>
    public static class RepositoryContainer
    {
        /// <summary>
        /// 
        /// </summary>
        static RepositoryContainer()
        {
            Repositorys = new ConcurrentDictionary<string, Lazy<object>>();
        }

        public static ConcurrentDictionary<string, Lazy<object>> Repositorys { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public static void Register<T>(T service)
            where T : IMongoBaseRepository
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
            where T : IMongoBaseRepository, new()
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
            where T : IMongoBaseRepository
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
            where T : IMongoBaseRepository
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
                throw new Exception($"this repository({k}) is not register");
            }

            //repository = Repositorys.GetOrAdd(k, x => { return new Lazy<object>(() => new T()); });
            //return (T)repository.Value;
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

    }
}
