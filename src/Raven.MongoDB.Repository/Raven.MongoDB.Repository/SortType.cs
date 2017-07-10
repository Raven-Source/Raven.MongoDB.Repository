#if MongoDB_Repository
namespace MongoDB.Repository
#else
namespace Raven.MongoDB.Repository
#endif
{
    /// <summary>
    /// 排序类型
    /// </summary>
    public enum SortType
    {
        /// <summary>
        /// 
        /// </summary>
        Ascending = 0,
        /// <summary>
        /// 
        /// </summary>
        Descending = 1,
    }
}
