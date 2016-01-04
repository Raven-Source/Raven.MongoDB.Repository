using MongoDB.Bson;
using MongoDB.Driver;
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
    public interface IReaderRepositoryAsync<TEntity, TKey>
    {
        /// <summary>
        /// 创建自增长ID
        /// <remarks>默认自增ID存放 [Sequence] 集合</remarks>
        /// </summary>
        /// <returns></returns>
        Task<long> CreateIncIDAsync(long inc = 1, int iteration = 0);

        /// <summary>
        /// 创建自增ID
        /// </summary>
        /// <param name="entity"></param>
        Task CreateIncIDAsync(TEntity entity);

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <param name="hint">hint索引</param>
        /// <param name="readConcern">访问设置</param>
        /// <returns></returns>
        Task<TEntity> GetAsync(TKey id, Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, BsonValue hint = null
            , ReadConcern readConcern = null);

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filterExp">查询条件表达式</param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <param name="hint">hint索引</param>
        /// <param name="readConcern">访问设置</param>
        /// <returns></returns>
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, BsonValue hint = null
            , ReadConcern readConcern = null);

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="projection"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readConcern">访问设置</param>
        /// <returns></returns>
        Task<TEntity> GetAsync(FilterDefinition<TEntity> filter
             , ProjectionDefinition<TEntity, TEntity> projection = null
             , SortDefinition<TEntity> sort = null, BsonValue hint = null
             , ReadConcern readConcern = null);

        /// <summary>
        /// 根据条件获取获取列表
        /// </summary>
        /// <param name="filterExp">查询条件表达式</param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readConcern">访问设置</param>
        /// <returns></returns>
        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filterExp = null
            , Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0, BsonValue hint = null
            , ReadConcern readConcern = null);

        /// <summary>
        /// 根据条件获取获取列表
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="projection"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readConcern">访问设置</param>
        /// <returns></returns>
        Task<List<TEntity>> GetListAsync(FilterDefinition<TEntity> filter
            , ProjectionDefinition<TEntity, TEntity> projection = null
            , SortDefinition<TEntity> sort = null
            , int limit = 0, int skip = 0, BsonValue hint = null
            , ReadConcern readConcern = null);

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readConcern">访问设置</param>
        /// <returns></returns>
        Task<long> CountAsync(FilterDefinition<TEntity> filter
            , int limit = 0, int skip = 0, BsonValue hint = null
            , ReadConcern readConcern = null);

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readConcern">访问设置</param>
        /// <returns></returns>
        Task<long> CountAsync(Expression<Func<TEntity, bool>> filterExp
             , int limit = 0, int skip = 0, BsonValue hint = null
             , ReadConcern readConcern = null);

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readConcern">访问设置</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(FilterDefinition<TEntity> filter, BsonValue hint = null
            , ReadConcern readConcern = null);

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readConcern">访问设置</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filterExp, BsonValue hint = null
             , ReadConcern readConcern = null);
    }
}
