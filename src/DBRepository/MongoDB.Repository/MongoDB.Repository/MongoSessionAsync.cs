using DB.Repository;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository
{
    public class MongoSessionAsync
    {
        #region 私有方法

        /// <summary>
        /// MongoDB连接字符串默认配置节
        /// </summary>
        private const string DEFAULT_CONFIG_NODE = "MongoDB";
        /// <summary>
        /// Mongo自增长ID数据序列
        /// </summary>
        private MongoSequence _sequence;
        /// <summary>
        /// MongoDB WriteConcern
        /// </summary>
        private WriteConcern _writeConcern;
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
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        /// <param name="isSlaveOK"></param>
        public MongoSessionAsync(string connString, string dbName, WriteConcern writeConcern = null, MongoSequence sequence = null, bool isSlaveOK = false, ReadPreference readPreference = null)
        {
            this._writeConcern = writeConcern ?? WriteConcern.Unacknowledged;
            this._sequence = sequence ?? new MongoSequence();

            var databaseSettings = new MongoDatabaseSettings();
            databaseSettings.WriteConcern = this._writeConcern;
            databaseSettings.ReadPreference = readPreference ?? ReadPreference.SecondaryPreferred;

            _mongoClient = new MongoClient(connString);
            Database = _mongoClient.GetDatabase(dbName, databaseSettings);
        }

        #endregion


        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>() where T : class, new()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public UpdateDefinitionBuilder<T> CreateUpdateDefinition<T>()
        {
            return Builders<T>.Update;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FilterDefinitionBuilder<T> CreateFilterDefinition<T>()
        {
            return Builders<T>.Filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IndexKeysDefinitionBuilder<T> CreateIndexKeysDefinition<T>()
        {
            return Builders<T>.IndexKeys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SortDefinitionBuilder<T> CreateSortDefinition<T>()
        {
            return Builders<T>.Sort;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ProjectionDefinitionBuilder<T> CreateProjectionDefinition<T>()
        {
            return Builders<T>.Projection;
        }

        /// <summary>
        /// 创建自增长ID
        /// <remarks>默认自增ID存放 [Sequence] 集合</remarks>
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public async Task<long> CreateIncID<T>() where T : class, new()
        {
            long id = 1;
            var collection = Database.GetCollection<BsonDocument>(this._sequence.SequenceName);
            var typeName = typeof(T).Name;

            var query = CreateFilterDefinition<BsonDocument>().Eq(this._sequence.CollectionName, typeName);
            var update = CreateUpdateDefinition<BsonDocument>().Inc(this._sequence.IncrementID, 1L);
            var options = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>();
            options.IsUpsert = true;
            options.ReturnDocument = ReturnDocument.After;

            var result = await collection.FindOneAndUpdateAsync(query, update, options);
            id = result[this._sequence.IncrementID].AsInt64;

            return id;
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="indexKey">索引字段</param>
        public async Task<string> CreateIndex<T>(Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> indexKeyExp) where T : class,new()
        {
            var indexKey = indexKeyExp(Builders<T>.IndexKeys);
            var result = await this.GetCollection<T>().Indexes.CreateOneAsync(indexKey);
            return result;
        }

        /// <summary>
        /// 获取系统当前时间
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> GetSysDateTime()
        {
            var result = await Database.RunCommandAsync<BsonValue>("new Date()");
            return result.ToUniversalTime();
        }

        /// <summary>
        /// 查询记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        /// <param name="sort"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public Task<IAsyncCursor<T>> Query<T>(Expression<Func<T, bool>> filter
            , Expression<Func<T, object>> field = null
            , Expression<Func<T, object>> sort = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0)
            where T : class, new()
        {
            var filterDef = Builders<T>.Filter.Where(filter);
            return Query<T>(filterDef, field, sort, sortType, limit, skip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        /// <param name="sort"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public async Task<IAsyncCursor<T>> Query<T>(FilterDefinition<T> filter
            , Expression<Func<T, object>> field = null
            , Expression<Func<T, object>> sort = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0)
            where T : class, new()
        {
            var option = new FindOptions<T, T>();
            if (limit > 0)
            {
                option.Limit = limit;
            }
            if (skip > 0)
            {
                option.Skip = skip;
            }

            if (field != null)
            {
                option.Projection = CreateProjectionDefinition<T>().Include(field);
            }

            if (sort != null)
            {
                if (sortType == SortType.Ascending)
                {
                    option.Sort = CreateSortDefinition<T>().Ascending(sort);
                }
                else
                {
                    option.Sort = CreateSortDefinition<T>().Descending(sort);
                }
            }

            var result = await this.GetCollection<T>().FindAsync(filter, option);

            return result;
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="item">待添加数据</param>
        /// <returns></returns>
        public async Task Insert<T>(T item) where T : class, new()
        {
            await this.GetCollection<T>().InsertOneAsync(item);
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="items">待添加数据集合</param>
        /// <returns></returns>
        public async Task InsertBatch<T>(IEnumerable<T> items) where T : class, new()
        {
            await this.GetCollection<T>().InsertManyAsync(items);
        }

    }
}
