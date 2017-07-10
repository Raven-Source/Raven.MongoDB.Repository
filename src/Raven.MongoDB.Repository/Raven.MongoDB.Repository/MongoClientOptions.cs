using MongoDB.Driver;

#if MongoDB_Repository
namespace MongoDB.Repository
#else
namespace Raven.MongoDB.Repository
#endif
{
    /// <summary>
    /// Options
    /// </summary>
    public class MongoRepositoryOptions
    {
        /// <summary>
        /// 数据库连接节点(必须)
        /// </summary>
        public string ConnString { get; set; }

        /// <summary>
        /// 数据库名称(必须)
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// 数据库集合名称(非必须)
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// WriteConcern(非必须)
        /// </summary>
        public WriteConcern WriteConcern { get; set; }

        /// <summary>
        /// ReadPreference(非必须)
        /// </summary>
        public ReadPreference ReadPreference { get; set; }

        /// <summary>
        /// Mongo自增长ID数据序列对象(非必须)
        /// </summary>
        public MongoSequence Sequence { get; set; }
    }
}
