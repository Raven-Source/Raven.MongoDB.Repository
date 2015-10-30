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
    public class MongoReaderRepositoryAsync<TEntity, TKey> : MongoBaseRepositoryAsync<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库连接节点</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        /// <param name="readPreference"></param>
        public MongoReaderRepositoryAsync(string connString, string dbName, ReadPreference readPreference = null, MongoSequence sequence = null)
            :base(connString, dbName, readPreference, sequence)
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
        public async Task<TEntity> GetAsync(TKey id, Expression<Func<TEntity, object>> includeFieldExp = null
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
            var result = await _mongoSession.GetCollection<TEntity>().FindAsync(filter, option);
            var reslut = await result.ToListAsync();

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
        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            //var cursor = await _mongoSession.FindAsync(filterExp: filterExp, fieldExp: fieldExp, limit: 1);
            //var reslut = await cursor.ToListAsync();

            FilterDefinition<TEntity> filter = null;
            ProjectionDefinition<TEntity, TEntity> projection = null;

            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            if (includeFieldExp != null)
            {
                projection = _mongoSession.IncludeFields(includeFieldExp);
            }
            var option = _mongoSession.CreateFindOptions(projection, sortExp, sortType, limit: 1);
            var result = await _mongoSession.GetCollection<TEntity>().FindAsync(filter, option);
            var reslut = await result.ToListAsync();

            return reslut.FirstOrDefault();
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(FilterDefinition<TEntity> filter
            , SortDefinition<TEntity> sort = null, ProjectionDefinition<TEntity, TEntity> projection = null)
        {
            //var cursor = await _mongoSession.FindAsync(filter: filter, fieldExp: fieldExp, limit: 1);

            var option = _mongoSession.CreateFindOptions(projection, sort, limit: 1);
            var result = await _mongoSession.GetCollection<TEntity>().FindAsync(filter, option);
            var reslut = await result.ToListAsync();

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
        public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filterExp = null
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
                filter = Builders<TEntity>.Filter.And();
            }

            sort = _mongoSession.CreateSortDefinition(sortExp, sortType);

            if (includeFieldExp != null)
            {
                projection = _mongoSession.IncludeFields(includeFieldExp);
            }
            var option = _mongoSession.CreateFindOptions(projection, sort, limit, skip);
            var result = await _mongoSession.GetCollection<TEntity>().FindAsync(filter, option);
            var reslut = await result.ToListAsync();

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
        public async Task<List<TEntity>> GetListAsync(FilterDefinition<TEntity> filter
            , ProjectionDefinition<TEntity, TEntity> projection = null
            , SortDefinition<TEntity> sort = null
            , int limit = 0, int skip = 0)
        {
            var option = _mongoSession.CreateFindOptions(projection, sort, limit, skip);
            var result = await _mongoSession.GetCollection<TEntity>().FindAsync(filter, option);
            var reslut = await result.ToListAsync();

            return reslut;
        }
        /// <summary>
        /// 数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public async Task<long> CountAsync(FilterDefinition<TEntity> filter
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

            return await _mongoSession.GetCollection<TEntity>().CountAsync(filter, option);
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterExp"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filterExp
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

            return await _mongoSession.GetCollection<TEntity>().CountAsync(filterExp, option);
        }
    }
}
