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
        public const string PRIMARY_KEY_NAME = "_id";
    }
}
