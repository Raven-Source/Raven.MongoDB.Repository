using MongoDB.Driver;

#if MongoDB_Repository
namespace MongoDB.Repository
#else
namespace Raven.MongoDB.Repository
#endif
{
    /// <summary>
    /// MongoSession
    /// </summary>
    public class MongoSession
    {
        /// <summary>
        /// MongoDB WriteConcern
        /// </summary>
        private WriteConcern _writeConcern;

        /// <summary>
        /// 
        /// </summary>
        public WriteConcern WriteConcern { get { return _writeConcern; } }

        /// <summary>
        /// 
        /// </summary>
        private ReadPreference _readPreference;

        /// <summary>
        /// 
        /// </summary>
        public ReadPreference ReadPreference { get { return _readPreference; } }

        /// <summary>
        /// MongoClient
        /// </summary>
        private MongoClient _mongoClient;

        /// <summary>
        /// MongoDatabase
        /// </summary>
        public IMongoDatabase Database { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库链接字符串</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="writeConcern">WriteConcern选项</param>
        /// <param name="isSlaveOK"></param>
        /// <param name="readPreference"></param>
        public MongoSession(string connString, string dbName, WriteConcern writeConcern = null, bool isSlaveOK = false, ReadPreference readPreference = null)
            : this(new MongoClient(connString), dbName, writeConcern, isSlaveOK, readPreference)
        { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mongoClientSettings">The settings for a MongoDB client</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="writeConcern">WriteConcern选项</param>
        /// <param name="isSlaveOK"></param>
        /// <param name="readPreference"></param>
        public MongoSession(MongoClientSettings mongoClientSettings, string dbName, WriteConcern writeConcern = null, bool isSlaveOK = false, ReadPreference readPreference = null)
            : this(new MongoClient(mongoClientSettings), dbName, writeConcern, isSlaveOK, readPreference)
        { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mongoClient">MongoClient</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="writeConcern">WriteConcern选项</param>
        /// <param name="isSlaveOK"></param>
        /// <param name="readPreference"></param>
        public MongoSession(MongoClient mongoClient, string dbName, WriteConcern writeConcern = null, bool isSlaveOK = false, ReadPreference readPreference = null)
        {
            this._writeConcern = writeConcern ?? WriteConcern.Unacknowledged;
            this._readPreference = readPreference ?? ReadPreference.SecondaryPreferred;

            var databaseSettings = new MongoDatabaseSettings();
            databaseSettings.WriteConcern = this._writeConcern;
            databaseSettings.ReadPreference = this._readPreference;

            _mongoClient = mongoClient;
            //if (_mongoClient.Settings.SocketTimeout == TimeSpan.Zero)
            //{
            //    _mongoClient.Settings.SocketTimeout = TimeSpan.FromSeconds(10);
            //}

            //if (_mongoClient.Settings.WaitQueueTimeout == TimeSpan.Zero)
            //{
            //    _mongoClient.Settings.WaitQueueTimeout = TimeSpan.FromSeconds(30);
            //}

            Database = _mongoClient.GetDatabase(dbName, databaseSettings);
        }


    }
}
