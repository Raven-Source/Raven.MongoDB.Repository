using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Raven.Data.Entity;

#if MongoDB_Repository
namespace MongoDB.Repository
#else
namespace Raven.MongoDB.Repository
#endif
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Util
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly string PRIMARY_KEY_NAME = "_id";

        public static readonly Type AUTO_INCR_TYPE = typeof(IAutoIncr);

        public static readonly string CREATE_INSTANCE_METHOD = "CreateInstance";

        /// <summary>
        /// 取得某个接口下所有实现这个接口的类
        /// </summary>
        /// <param name="t">指定接口</param>
        /// <param name="assemblyName">指定程序集名称</param>
        /// <returns></returns>
        public static List<Type> GetAllClassByInterface(Type t, string assemblyName)
        {
            if (!t.IsInterface || string.IsNullOrWhiteSpace(assemblyName))
            {
                return null;
            }

            //获取指定包下面所有的class
            List<Type> allClassList = GetClasses(assemblyName);
            if (allClassList == null || !allClassList.Any())
            {
                return null;
            }
            List<Type> returnClassList = new List<Type>();
            foreach (var type in allClassList)
            {
                if (type.GetInterface(t.Name, true) != null && !type.IsAbstract && !type.IsInterface)
                {
                    returnClassList.Add(type);
                }
            }
            return returnClassList;
        }

        /// <summary>
        /// 从指定程序集下获取所有的Class
        /// </summary>
        /// <param name="assemblyName">指定程序集名称</param>
        /// <returns></returns>
        public static List<Type> GetClasses(string assemblyName)
        {
            List<Type> classes = null;
            //获取指定程序集下所有的类
            var assembly = Assembly.Load(assemblyName);
            //取出所有类型集合
            var typeArray = assembly.GetTypes();
            if (typeArray != null)
            {
                classes = typeArray.ToList();
            }
            return classes;
        }
    }
}
