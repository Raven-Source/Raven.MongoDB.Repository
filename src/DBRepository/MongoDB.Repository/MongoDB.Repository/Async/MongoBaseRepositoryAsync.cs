using DB.Repository;
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
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MongoBaseRepositoryAsync<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        protected MongoSessionAsync _mongoSession;

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
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        /// <param name="readPreference"></param>
        public MongoBaseRepositoryAsync(string connString, string dbName, ReadPreference readPreference = null, MongoSequence sequence = null)
        {
            _mongoSession = new MongoSessionAsync(connString, dbName, readPreference: readPreference, sequence: sequence);
        }

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection()
        {
            return _mongoSession.GetCollection<TEntity>();
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="indexKeyExp">索引字段</param>
        public async Task<string> CreateIndexAsync(Func<IndexKeysDefinitionBuilder<TEntity>, IndexKeysDefinition<TEntity>> indexKeyExp)
        {
            var indexKey = indexKeyExp(Builders<TEntity>.IndexKeys);
            var result = await _mongoSession.GetCollection<TEntity>().Indexes.CreateOneAsync(indexKey);
            return result;
        }
        
        /// <summary>
        /// 创建自增ID
        /// </summary>
        public async Task<long> CreateIncIDAsync(long inc = 1)
        {
            return await _mongoSession.CreateIncIDAsync<TEntity>(inc);
        }
        /// <summary>
        /// 创建自增ID
        /// </summary>
        /// <param name="entity"></param>
        public async Task CreateIncIDAsync(TEntity entity)
        {
            long _id = 0;
            _id = await _mongoSession.CreateIncIDAsync<TEntity>();
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
