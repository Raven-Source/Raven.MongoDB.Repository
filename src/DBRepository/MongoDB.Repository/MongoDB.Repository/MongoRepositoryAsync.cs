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
        /// 自增ID的属性
        /// </summary>
        private static Type tBsonIdType = typeof(MongoDB.Bson.Serialization.Attributes.BsonIdAttribute);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库连接节点</param>
        /// <param name="dbName">数据库名称</param>
        public MongoRepositoryAsync(string connString, string dbName, ReadPreference readPreference = null)
        {
            _mongoSession = new MongoSessionAsync(connString, dbName, readPreference: readPreference);
        }

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(TKey id)
        {
            var option = new FindOptions<TEntity,TEntity>(){ Limit = 1};
            var filter = _mongoSession.CreateFilterDefinition<TEntity>().Eq(x => x.ID, id);
            var result = await _mongoSession.GetCollection<TEntity>().FindAsync(filter, option);            

            return result.Current.FirstOrDefault();
        }

    }
}
