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
    /// 读取仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MongoReaderRepository<TEntity, TKey> : MongoBaseRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库连接节点</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="writeConcern"></param>
        /// <param name="readPreference"></param>
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        public MongoReaderRepository(string connString, string dbName, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null)
            :base(connString, dbName, writeConcern, readPreference, sequence)
        {
        }

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <returns></returns>
        public TEntity Get(TKey id, Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.ID, id);
            //var cursor = await _mongoSession.FindAsync(filter: filter, fieldExp: fieldExp, limit: 1);

            ProjectionDefinition<TEntity, TEntity> projection = null;
            if (includeFieldExp != null)
            {
                projection = _mongoSession.IncludeFields(includeFieldExp);
            }
            var option = _mongoSession.CreateFindOptions(projection, sortExp, sortType, limit: 1);
            var result = _mongoSession.GetCollection<TEntity>().FindAsync(filter, option).Result;
            var reslut = result.ToListAsync().Result;

            return reslut.FirstOrDefault();
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filterExp">查询条件表达式</param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, object>> includeFieldExp = null
                , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            FilterDefinition<TEntity> filter = null;
            ProjectionDefinition<TEntity, TEntity> projection = null;

            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            if (includeFieldExp != null)
            {
                projection = _mongoSession.IncludeFields(includeFieldExp);
            }
            var option = _mongoSession.CreateFindOptions(projection, sortExp, sortType, limit: 1);
            var result = _mongoSession.GetCollection<TEntity>().FindAsync(filter, option).Result;
            var reslut = result.ToListAsync().Result;

            return reslut.FirstOrDefault();
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public TEntity Get(FilterDefinition<TEntity> filter
            , SortDefinition<TEntity> sort = null, ProjectionDefinition<TEntity, TEntity> projection = null)
        {
            //var cursor = await _mongoSession.FindAsync(filter: filter, fieldExp: fieldExp, limit: 1);

            var option = _mongoSession.CreateFindOptions(projection, sort, limit: 1);
            var result = _mongoSession.GetCollection<TEntity>().FindAsync(filter, option).Result;
            var reslut = result.ToListAsync().Result;

            return reslut.FirstOrDefault();
        }

        /// <summary>
        /// 根据条件获取获取列表
        /// </summary>
        /// <param name="filterExp">查询条件表达式</param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public List<TEntity> GetList(Expression<Func<TEntity, bool>> filterExp = null
            , Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0)
        {
            FilterDefinition<TEntity> filter = null;
            ProjectionDefinition<TEntity, TEntity> projection = null;
            SortDefinition<TEntity> sort = null;

            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            sort = _mongoSession.CreateSortDefinition(sortExp, sortType);

            if (includeFieldExp != null)
            {
                projection = _mongoSession.IncludeFields(includeFieldExp);
            }
            var option = _mongoSession.CreateFindOptions(projection, sort, limit, skip);
            var result = _mongoSession.GetCollection<TEntity>().FindAsync(filter, option).Result;
            var reslut = result.ToListAsync().Result;

            return reslut;
        }

        /// <summary>
        /// 根据条件获取获取列表
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="projection"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public List<TEntity> GetList(FilterDefinition<TEntity> filter
            , ProjectionDefinition<TEntity, TEntity> projection = null
            , SortDefinition<TEntity> sort = null
            , int limit = 0, int skip = 0)
        {
            var option = _mongoSession.CreateFindOptions(projection, sort, limit, skip);
            var result = _mongoSession.GetCollection<TEntity>().FindAsync(filter, option).Result;
            var reslut = result.ToListAsync().Result;

            return reslut;
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public long Count(FilterDefinition<TEntity> filter
            , int limit = 0, int skip = 0)
        {
            CountOptions option = new CountOptions();
            if (limit > 0)
            {
                option.Limit = limit;
            }
            if (skip > 0)
            {
                option.Skip = skip;
            }

            return _mongoSession.GetCollection<TEntity>().CountAsync(filter, option).Result;
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public long Count(Expression<Func<TEntity, bool>> filterExp
            , int limit = 0, int skip = 0)
        {
            CountOptions option = new CountOptions();
            if (limit > 0)
            {
                option.Limit = limit;
            }
            if (skip > 0)
            {
                option.Skip = skip;
            }

            return _mongoSession.GetCollection<TEntity>().CountAsync(filterExp, option).Result;
        }

    }
}
