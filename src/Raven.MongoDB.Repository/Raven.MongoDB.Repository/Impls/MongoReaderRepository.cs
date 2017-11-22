using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#if MongoDB_Repository
using Repository.IEntity;
namespace MongoDB.Repository
#else
using Raven.Data.Entity;
namespace Raven.MongoDB.Repository
#endif
{
    /// <summary>
    /// 读取仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MongoReaderRepository<TEntity, TKey> : MongoBaseRepository<TEntity, TKey>, IMongoReaderRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库连接节点</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="collectionName">集合名称</param>
        /// <param name="writeConcern"></param>
        /// <param name="readPreference"></param>
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        public MongoReaderRepository(string connString, string dbName, string collectionName = null, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null)
            : base(connString, dbName, collectionName, writeConcern, readPreference, sequence)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public MongoReaderRepository(MongoRepositoryOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// 创建自增长ID
        /// </summary>
        /// <returns></returns>
        public long CreateIncID(long inc = 1, int iteration = 0)
        {
            long id = 1;
            var collection = Database.GetCollection<BsonDocument>(base._sequence.SequenceName);
            var typeName = CollectionName;

            var query = Builders<BsonDocument>.Filter.Eq(base._sequence.CollectionName, typeName);
            var update = Builders<BsonDocument>.Update.Inc(base._sequence.IncrementID, inc);
            var options = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>();
            options.IsUpsert = true;
            options.ReturnDocument = ReturnDocument.After;

            var result = collection.FindOneAndUpdate(query, update, options);
            if (result != null)
            {
                id = result[base._sequence.IncrementID].AsInt64;
                return id;
            }
            else if (iteration <= 1)
            {
                return CreateIncID(inc, ++iteration);
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
        public void CreateIncID(TEntity entity)
        {
            long _id = 0;
            _id = this.CreateIncID();
            AssignmentEntityID(entity, _id);
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
        public TEntity Get(TKey id, Expression<Func<TEntity, object>> includeFieldExp = null
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.ID, id);

            ProjectionDefinition<TEntity, TEntity> projection = null;
            if (includeFieldExp != null)
            {
                projection = base.IncludeFields(includeFieldExp);
            }

            var option = base.CreateFindOptions(projection, sortExp, sortType, limit: 1, hint: hint);
            var result = base.GetCollection(readPreference).FindSync(filter, option);

            return result.FirstOrDefault();
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
        public TEntity Get(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, object>> includeFieldExp = null
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
            var result = base.GetCollection(readPreference).FindSync(filter, option);

            return result.FirstOrDefault();
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="sort">排序</param>
        /// <param name="projection"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public TEntity Get(FilterDefinition<TEntity> filter
            , ProjectionDefinition<TEntity, TEntity> projection = null
            , SortDefinition<TEntity> sort = null, BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var option = base.CreateFindOptions(projection, sort, limit: 1, hint: hint);
            var result = base.GetCollection(readPreference).FindSync(filter, option);

            return result.FirstOrDefault();
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
        public List<TEntity> GetList(Expression<Func<TEntity, bool>> filterExp = null
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
            var result = base.GetCollection(readPreference).FindSync(filter, option);

            return result.ToList();
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
        public List<TEntity> GetList(FilterDefinition<TEntity> filter
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
            var result = base.GetCollection(readPreference).FindSync(filter, option);

            return result.ToList();
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldExp"></param>
        /// <param name="filterExp"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TField> Distinct<TField>(Expression<Func<TEntity, TField>> fieldExp, Expression<Func<TEntity, bool>> filterExp
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

            //var result = base.GetCollection(readPreference).Distinct(fieldExp, filter);
            //return result.ToList();
            return this.Distinct(fieldExp, filter);
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldExp"></param>
        /// <param name="filter"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TField> Distinct<TField>(Expression<Func<TEntity, TField>> fieldExp, FilterDefinition<TEntity> filter
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var result = base.GetCollection(readPreference).Distinct(fieldExp, filter);
            return result.ToList();
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <param name="filter"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TField> Distinct<TField>(FieldDefinition<TEntity, TField> field, FilterDefinition<TEntity> filter
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var result = base.GetCollection(readPreference).Distinct(field, filter);
            return result.ToList();
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
        public long Count(FilterDefinition<TEntity> filter
            , int limit = 0, int skip = 0, BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            var option = base.CreateCountOptions(limit, skip, hint);

            return base.GetCollection(readPreference).Count(filter, option);
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
        public long Count(Expression<Func<TEntity, bool>> filterExp
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

            return this.Count(filter, limit, skip, hint, readPreference);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public bool Exists(FilterDefinition<TEntity> filter
            , BsonValue hint = null
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            //var option = base.CreateCountOptions(1, 0, hint);

            return this.Get(filter, Projection.Include(x => x.ID), null, hint, readPreference) != null;
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public bool Exists(Expression<Func<TEntity, bool>> filterExp
            , BsonValue hint = null
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

            return this.Exists(filter, hint, readPreference);
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
        public List<TResult> Aggregate<TResult, TID>(Expression<Func<TEntity, bool>> filterExp
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

            return this.Aggregate<TResult, TID>(filter, id, group, sortExp, sortType, limit, skip, readPreference);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TID"></typeparam>
        /// <param name="filter"></param>
        /// <param name="id">$group -> _id</param>
        /// <param name="group">$group</param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TResult> Aggregate<TResult, TID>(FilterDefinition<TEntity> filter
            , Expression<Func<TEntity, TID>> id, Expression<Func<IGrouping<TID, TEntity>, TResult>> group
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var fluent = base.CreateAggregate(filter, base.CreateSortDefinition(sortExp, sortType), readPreference);
            var fluentRes = fluent.Group(id, group);
            if (skip > 0)
            {
                fluentRes = fluentRes.Skip(skip);
            }
            if (limit > 0)
            {
                fluentRes = fluentRes.Limit(limit);
            }

            return fluentRes.ToList();
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
        public List<TResult> Aggregate<TResult, TID>(Expression<Func<TEntity, bool>> filterExp
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

            return this.Aggregate<TResult, TID>(filter, group, sortExp, sortType, limit, skip, readPreference);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TID"></typeparam>
        /// <param name="filter"></param>
        /// <param name="group"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TResult> Aggregate<TResult, TID>(FilterDefinition<TEntity> filter
            , ProjectionDefinition<TEntity, TResult> group
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0
            , ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }

            var fluent = base.CreateAggregate(filter, base.CreateSortDefinition(sortExp, sortType), readPreference);
            var fluentRes = fluent.Group(group);
            if (skip > 0)
            {
                fluentRes = fluentRes.Skip(skip);
            }
            if (limit > 0)
            {
                fluentRes = fluentRes.Limit(limit);
            }

            return fluentRes.ToList();
        }
    }
}
