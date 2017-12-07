#if MongoDB_Repository
namespace MongoDB.Repository
#else
using System;

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

        public static readonly Type AUTO_INCR_TYPE = typeof(Raven.Data.Entity.IAutoIncr);

        public static readonly string CREATE_INSTANCE_METHOD = "createInstance";
    }
}
