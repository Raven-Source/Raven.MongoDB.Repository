using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository
{
    /// <summary>
    /// 容器
    /// </summary>
    public class RepositoryContainer
    {
        #region Instance

        private static Lazy<RepositoryContainer> _instance = new Lazy<RepositoryContainer>(() => new RepositoryContainer());
        private static RepositoryContainer Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        #endregion

        private RepositoryContainer()
        {
            Repositorys = new Dictionary<Type, Lazy<object>>();
        }

        private Dictionary<Type, Lazy<object>> Repositorys { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public static void Register<T>(T service)
        {
            Instance.Repositorys[typeof(T)] = new Lazy<object>(() => service);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>()
            where T : new()
        {
            Instance.Repositorys[typeof(T)] = new Lazy<object>(() => new T());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        public static void Register<T>(Func<object> function)
        {
            Instance.Repositorys[typeof(T)] = new Lazy<object>(function);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
            where T : new()
        {
            Lazy<object> repository;
            if (Instance.Repositorys.TryGetValue(typeof(T), out repository))
            {
                return (T)repository.Value;
            }
            else
            {
                Instance.Repositorys[typeof(T)] = new Lazy<object>(() => new T());
                return Resolve<T>();
                //throw new KeyNotFoundException(string.Format("Service not found for type '{0}'", typeof(T)));
            }
        }
    }
}
