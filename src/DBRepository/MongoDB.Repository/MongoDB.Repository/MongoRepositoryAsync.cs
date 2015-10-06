using MongoDB.Driver;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MongoRepositoryAsync<TEntity, TKey>
        where TEntity : class,IEntity<TKey>, new()
    {
        MongoSessionAsync _mongoSession;

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

        ///// <summary>
        ///// 自增ID的属性
        ///// </summary>
        //private static Type tBsonIdType = typeof(MongoDB.Bson.Serialization.Attributes.BsonIdAttribute);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库连接节点</param>
        /// <param name="dbName">数据库名称</param>
        public MongoRepositoryAsync(string connString, string dbName, ReadPreference readPreference = null, MongoSequence sequence = null)
        {
            _mongoSession = new MongoSessionAsync(connString, dbName, readPreference: readPreference, sequence: sequence);
        }

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(TKey id)
        {
            var filter = _mongoSession.CreateFilterDefinition<TEntity>().Eq(x => x.ID, id);

            var cursor = await _mongoSession.Query<TEntity>(filter, limit: 1);
            var reslut = await cursor.ToListAsync();

            return reslut.FirstOrDefault();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="item">待添加数据</param>
        /// <returns></returns>
        public async Task Insert(TEntity entity)
        {
            long _id = 0;
            if (entity is IAutoIncr)
            {
                _id = await _mongoSession.CreateIncID<TEntity>();

                IEntity<TKey> tEntity = entity as IEntity<TKey>;
                if (tEntity.ID is int)
                {
                    (entity as IEntity<int>).ID = (int)_id;
                }
                else if (tEntity.ID is long)
                {
                    (entity as IEntity<long>).ID = (long)_id;
                }
                else if (tEntity.ID is short)
                {
                    (entity as IEntity<short>).ID = (short)_id;
                }
            }

            await _mongoSession.Insert(entity);
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="items">待添加数据集合</param>
        /// <returns></returns>
        public async Task InsertBatch(IEnumerable<TEntity> items)
        {
            await _mongoSession.InsertBatch<TEntity>(items);
        }
    }
}
