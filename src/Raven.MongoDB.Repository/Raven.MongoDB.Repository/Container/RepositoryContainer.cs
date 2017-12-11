using System;
using System.Collections.Concurrent;
using System.Reflection;

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
        private static BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        /// <summary>
        /// 
        /// </summary>
        static RepositoryContainer()
        {
            Instances = new ConcurrentDictionary<string, Lazy<object>>();
        }

        /// <summary>
        /// 
        /// </summary>
        //public static ConcurrentDictionary<string, Lazy<object>> Instances { get; private set; }
        private static ConcurrentDictionary<string, Lazy<object>> Instances;

        #region 注册
        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public static void Register<T>(T service)
            where T : IMongoBaseRepository
        {
            var t = typeof(T);
            var lazy = new Lazy<object>(() => service);

            Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>()
            where T : IMongoBaseRepository, new()
        {
            var t = typeof(T);
            var lazy = new Lazy<object>(() => new T());

            Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        public static void Register<T>(Func<object> function)
            where T : IMongoBaseRepository
        {
            var t = typeof(T);
            var lazy = new Lazy<object>(function);

            Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <param name="t">typeof(T)</param>
        public static void Register(Type t)
        {
            var instance = Activator.CreateInstance(t, true);
            var lazy = new Lazy<object>(() => instance);
            Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <param name="assemblyNames"></param>
        public static void RegisterAll(params string[] assemblyNames)
        {
            LoadAll((t, repo) =>
            {
                var lazy = new Lazy<object>(() => repo);
                Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
            }, assemblyNames);
        }
        #endregion

        /// <summary>
        /// 取出指定对象
        /// </summary>
        /// <typeparam name="T">指定对象类型</typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
            where T : IMongoBaseRepository
        {
            var t = typeof(T);
            var k = GetKey(t);

            if (Instances.TryGetValue(k, out Lazy<object> repository))
            {
                return (T)repository.Value;
            }
            else
            {
                throw new Exception($"this repository({k}) is not register");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="assemblyNames"></param>
        private static void LoadAll(Action<Type, IMongoBaseRepository> a, params string[] assemblyNames)
        {
            foreach (var assemblyName in assemblyNames)
            {
                var classList = Util.GetAllClassByInterface(typeof(IMongoBaseRepository), assemblyName);
                foreach (var c in classList)
                {
                    IMongoBaseRepository repos = null;
                    var instance = Activator.CreateInstance(c, true);
                    try
                    {
                        var method = c.GetMethod(Util.CREATE_INSTANCE_METHOD, bindingFlags);
                        if (method != null)
                        {
                            repos = (IMongoBaseRepository)method.Invoke(instance, null);
                        }
                    }
                    catch { }
                    if (repos == null)
                    {
                        repos = (IMongoBaseRepository)instance;
                    }
                    if (repos == null)
                    {
                        throw new Exception(string.Format("this repository({0}.{1}) is not create", c.Namespace, c.Name));
                    }
                    a?.Invoke(c, repos);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static string GetKey(Type t)
        {
            return t.FullName;
        }

    }
}
