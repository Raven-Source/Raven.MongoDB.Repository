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
    /// 异步读取仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public partial class MongoReaderRepositoryAsync<TEntity, TKey> : MongoBaseRepository<TEntity, TKey>, IReaderRepositoryAsync<TEntity, TKey>
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
        public MongoReaderRepositoryAsync(string connString, string dbName, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null)
            : base(connString, dbName, writeConcern, readPreference, sequence)
        {
        }

        /// <summary>
        /// 创建自增长ID
        /// </summary>
        /// <returns></returns>
        public async Task<long> CreateIncIDAsync(long inc = 1, int iteration = 0)
        {
            long id = 1;
            var collection = Database.GetCollection<BsonDocument>(base._sequence.SequenceName);
            var typeName = typeof(TEntity).Name;

            var query = Builders<BsonDocument>.Filter.Eq(base._sequence.CollectionName, typeName);
            var update = Builders<BsonDocument>.Update.Inc(base._sequence.IncrementID, inc);
            var options = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>();
            options.IsUpsert = true;
            options.ReturnDocument = ReturnDocument.After;

            var result = await collection.FindOneAndUpdateAsync(query, update, options).ConfigureAwait(false);
            if (result != null)
            {
                id = result[base._sequence.IncrementID].AsInt64;
                return id;
            }
            else if (iteration <= 1)
            {
                return await CreateIncIDAsync(inc, ++iteration);
            }
            else
            {
                throw new Exception("Failed to get on the IncID");
            }
        }

        /// <summary>
        /// 创建自增ID
        /// </summary>
        /// <param name="entity"></param>
        public Task CreateIncIDAsync(TEntity entity)
        {
            return this.CreateIncIDAsync().ContinueWith(x =>
            {
                long _id = x.Result;
                AssignmentEntityID(entity, _id);
            });
        }

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(TKey id, Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.ID, id);
            //var cursor = await base.FindAsync(filter: filter, fieldExp: fieldExp, limit: 1);

            ProjectionDefinition<TEntity, TEntity> projection = null;
            if (includeFieldExp != null)
            {
                projection = base.IncludeFields(includeFieldExp);
            }
            var option = base.CreateFindOptions(projection, sortExp, sortType, limit: 1, hint: hint);
            var result = await base.GetCollection(readPreference).FindAsync(filter, option).ConfigureAwait(false);

            return await result.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filterExp">查询条件表达式</param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, BsonValue hint = null
            , ReadPreference readPreference = null)
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
                projection = base.IncludeFields(includeFieldExp);
            }
            var option = base.CreateFindOptions(projection, sortExp, sortType, limit: 1, hint: hint);
            var result = await base.GetCollection(readPreference).FindAsync(filter, option).ConfigureAwait(false);

            return await result.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="projection"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(FilterDefinition<TEntity> filter
            , ProjectionDefinition<TEntity, TEntity> projection = null
            , SortDefinition<TEntity> sort = null, BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var option = base.CreateFindOptions(projection, sort: sort, limit: 1, hint: hint);
            var result = await base.GetCollection(readPreference).FindAsync(filter, option).ConfigureAwait(false);

            return await result.FirstOrDefaultAsync().ConfigureAwait(false);
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
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filterExp = null
            , Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0, BsonValue hint = null
            , ReadPreference readPreference = null)
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

            sort = base.CreateSortDefinition(sortExp, sortType);

            if (includeFieldExp != null)
            {
                projection = base.IncludeFields(includeFieldExp);
            }
            var option = base.CreateFindOptions(projection, sort, limit, skip, hint: hint);
            var result = await base.GetCollection(readPreference).FindAsync(filter, option).ConfigureAwait(false);

            return await result.ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 根据条件获取获取列表
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="projection"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public async Task<List<TEntity>> GetListAsync(FilterDefinition<TEntity> filter
            , ProjectionDefinition<TEntity, TEntity> projection = null
            , SortDefinition<TEntity> sort = null
            , int limit = 0, int skip = 0, BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var option = base.CreateFindOptions(projection, sort, limit, skip, hint: hint);
            var result = await base.GetCollection(readPreference).FindAsync(filter, option).ConfigureAwait(false);

            return await result.ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldExp"></param>
        /// <param name="filterExp"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public Task<List<TField>> DistinctAsync<TField>(Expression<Func<TEntity, TField>> fieldExp, Expression<Func<TEntity, bool>> filterExp
            , ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter = null;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            return this.DistinctAsync(fieldExp, filter);
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldExp"></param>
        /// <param name="filter"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public async Task<List<TField>> DistinctAsync<TField>(Expression<Func<TEntity, TField>> fieldExp, FilterDefinition<TEntity> filter
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var result = await base.GetCollection(readPreference).DistinctAsync(fieldExp, filter);
            return await result.ToListAsync();
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <param name="filter"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public async Task<List<TField>> DistinctAsync<TField>(FieldDefinition<TEntity, TField> field, FilterDefinition<TEntity> filter
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var result = await base.GetCollection(readPreference).DistinctAsync(field, filter);
            return await result.ToListAsync();
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public Task<long> CountAsync(FilterDefinition<TEntity> filter
            , int limit = 0, int skip = 0, BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            var option = base.CreateCountOptions(limit, skip, hint);

            return base.GetCollection(readPreference).CountAsync(filter, option);
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public Task<long> CountAsync(Expression<Func<TEntity, bool>> filterExp
            , int limit = 0, int skip = 0, BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter = null;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            return this.CountAsync(filter, limit, skip, hint, readPreference);
        }


        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(FilterDefinition<TEntity> filter
            , BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            var option = base.CreateCountOptions(1, 0, hint);

            return base.GetCollection(readPreference).CountAsync(filter, option).ContinueWith(x => x.Result > 0);
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filterExp
            , BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            var option = base.CreateCountOptions(1, 0, hint);

            FilterDefinition<TEntity> filter = null;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            return this.ExistsAsync(filter, hint, readPreference);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TID"></typeparam>
        /// <param name="filterExp"></param>
        /// <param name="id">$group -> _id</param>
        /// <param name="group">$group</param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public Task<List<TResult>> AggregateAsync<TResult, TID>(Expression<Func<TEntity, bool>> filterExp
            , Expression<Func<TEntity, TID>> id, Expression<Func<IGrouping<TID, TEntity>, TResult>> group
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0
            , ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter = null;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var fluent = base.CreateAggregate(filter, base.CreateSortDefinition(sortExp, sortType), limit, skip, readPreference);
            return fluent.Group(id, group).ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TID"></typeparam>
        /// <param name="filterExp"></param>
        /// <param name="group"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public Task<List<TResult>> AggregateAsync<TResult, TID>(Expression<Func<TEntity, bool>> filterExp
            , ProjectionDefinition<TEntity, TResult> group
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0
            , ReadPreference readPreference = null)
        {

            FilterDefinition<TEntity> filter = null;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var fluent = base.CreateAggregate(filter, base.CreateSortDefinition(sortExp, sortType), limit, skip, readPreference);
            return fluent.Group(group).ToListAsync();
        }

    }
}
