using DB.Repository;
using MongoDB.Bson;
using MongoDB.Driver;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository
{
    /// <summary>
    /// 同步仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MongoBaseRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// Mongo自增长ID数据序列
        /// </summary>
        private MongoSequence _sequence;

        /// <summary>
        /// 
        /// </summary>
        protected MongoSession _mongoSession;

        /// <summary>
        /// MongoDatabase
        /// </summary>
        public IMongoDatabase Database
        {
            get
            {
                return _mongoSession.Database;
            }
        }

        /// <summary>
        /// get Filter
        /// </summary>
        public static FilterDefinitionBuilder<TEntity> Filter
        {
            get
            {
                return Builders<TEntity>.Filter;
            }
        }

        /// <summary>
        /// get Sort
        /// </summary>
        public static SortDefinitionBuilder<TEntity> Sort
        {
            get
            {
                return Builders<TEntity>.Sort;
            }
        }

        /// <summary>
        /// get Update
        /// </summary>
        public static UpdateDefinitionBuilder<TEntity> Update
        {
            get
            {
                return Builders<TEntity>.Update;
            }
        }

        /// <summary>
        /// get Projection
        /// </summary>
        public static ProjectionDefinitionBuilder<TEntity> Projection
        {
            get
            {
                return Builders<TEntity>.Projection;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库连接节点</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="writeConcern"></param>
        /// <param name="readPreference"></param>
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        public MongoBaseRepository(string connString, string dbName, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null)
        {
            this._sequence = sequence ?? new MongoSequence();
            this._mongoSession = new MongoSession(connString, dbName, writeConcern: writeConcern, readPreference: readPreference);
        }

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection()
        {
            return _mongoSession.GetCollection<TEntity>();
        }

        /// <summary>
        /// 创建自增长ID
        /// <remarks>默认自增ID存放 [Sequence] 集合</remarks>
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public long CreateIncIDAsync<T>(long inc = 1, int iteration = 0) where T : class, new()
        {
            long id = 1;
            var collection = Database.GetCollection<BsonDocument>(this._sequence.SequenceName);
            var typeName = typeof(T).Name;

            var query = Builders<BsonDocument>.Filter.Eq("_id", typeName);
            var update = Builders<BsonDocument>.Update.Inc(this._sequence.IncrementID, inc);
            var options = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>();
            options.IsUpsert = true;
            options.ReturnDocument = ReturnDocument.After;

            var result = collection.FindOneAndUpdateAsync(query, update, options).Result;
            if (result != null)
            {
                id = result[this._sequence.IncrementID].AsInt64;
                return id;
            }
            else if (iteration <= 1)
            {
                return CreateIncIDAsync<T>(inc, ++iteration);
            }
            else
            {
                throw new Exception("Failed to get on the IncID");
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="indexKeyExp"></param>
        /// <returns></returns>
        public async Task<string> CreateIndexAsync(Func<IndexKeysDefinitionBuilder<TEntity>, IndexKeysDefinition<TEntity>> indexKeyExp)
        {
            var indexKey = indexKeyExp(Builders<TEntity>.IndexKeys);
            var result = await _mongoSession.GetCollection<TEntity>().Indexes.CreateOneAsync(indexKey).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// 创建自增ID
        /// </summary>
        public long CreateIncIDAsync(long inc = 1)
        {
            return this.CreateIncIDAsync<TEntity>(inc);
        }

        /// <summary>
        /// 创建自增ID
        /// </summary>
        /// <param name="entity"></param>
        public void CreateIncIDAsync(TEntity entity)
        {
            long _id = 0;
            _id = this.CreateIncIDAsync<TEntity>();
            AssignmentEntityID(entity, _id);
        }

        /// <summary>
        /// ID赋值
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        public void AssignmentEntityID(TEntity entity, long id)
        {
            IEntity<TKey> tEntity = entity as IEntity<TKey>;
            if (tEntity.ID is int)
            {
                (entity as IEntity<int>).ID = (int)id;
            }
            else if (tEntity.ID is long)
            {
                (entity as IEntity<long>).ID = (long)id;
            }
            else if (tEntity.ID is short)
            {
                (entity as IEntity<short>).ID = (short)id;
            }
        }

    }
}
