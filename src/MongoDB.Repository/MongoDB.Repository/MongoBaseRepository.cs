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
    /// 仓储Base
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MongoBaseRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// Mongo自增长ID数据序列
        /// </summary>
        protected MongoSequence _sequence;

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
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection(MongoCollectionSettings settings = null)
        {
            return Database.GetCollection<TEntity>(typeof(TEntity).Name, settings);
        }

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection(WriteConcern writeConcern)
        {
            MongoCollectionSettings settings = null;
            if (writeConcern != null)
            {
                settings = new MongoCollectionSettings();
                settings.WriteConcern = writeConcern;
            }
            return Database.GetCollection<TEntity>(typeof(TEntity).Name, settings);
        }

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection(ReadPreference readPreference)
        {
            MongoCollectionSettings settings = null;
            if (readPreference != null)
            {
                settings = new MongoCollectionSettings();
                settings.ReadPreference = readPreference;
            }
            return Database.GetCollection<TEntity>(typeof(TEntity).Name, settings);
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

        #region 获取字段

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="fieldsExp"></param>
        /// <returns></returns>
        public ProjectionDefinition<T> IncludeFields<T>(Expression<Func<T, object>> fieldsExp) where T : class, new()
        {
            var builder = Builders<T>.Projection;

            if (fieldsExp != null)
            {
                List<ProjectionDefinition<T>> fieldDocument = new List<ProjectionDefinition<T>>();
                var body = (fieldsExp.Body as NewExpression);
                if (body == null || body.Members == null)
                {
                    throw new Exception("fieldsExp is invalid expression format， eg: x => new { x.Field1, x.Field2 }");
                }
                foreach (var m in body.Members)
                {
                    fieldDocument.Add(builder.Include(m.Name));
                }
                return builder.Combine(fieldDocument);
            }
            return null;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public SortDefinition<T> CreateSortDefinition<T>(Expression<Func<T, object>> sortExp, SortType sortType = SortType.Ascending)
        {
            SortDefinition<T> sort = null;
            if (sortExp != null)
            {
                if (sortType == SortType.Ascending)
                {
                    sort = Builders<T>.Sort.Ascending(sortExp);
                }
                else
                {
                    sort = Builders<T>.Sort.Descending(sortExp);
                }
            }
            return sort;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projection"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        public FindOptions<T, T> CreateFindOptions<T>(ProjectionDefinition<T, T> projection = null
            , SortDefinition<T> sort = null
            , int limit = 0, int skip = 0, BsonValue hint = null)
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

            if (projection != null)
            {
                option.Projection = projection;
            }

            if (sort != null)
            {
                option.Sort = sort;
            }

            if (hint != null)
            {
                option.Modifiers = new BsonDocument
                {
                    { "$hint",hint }
                };
            }

            return option;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projection"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        public FindOptions<T, T> CreateFindOptions<T>(ProjectionDefinition<T, T> projection = null
            , Expression<Func<T, object>> sortExp = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0, BsonValue hint = null)
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

            if (projection != null)
            {
                option.Projection = projection;
            }

            SortDefinition<T> sort = CreateSortDefinition(sortExp, sortType);
            if (sort != null)
            {
                option.Sort = sort;
            }

            if (hint != null)
            {
                option.Modifiers = new BsonDocument
                {
                    { "$hint",hint }
                };
            }

            return option;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        public CountOptions CreateCountOptions(int limit = 0, int skip = 0, BsonValue hint = null)
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

            if (hint != null)
            {
                option.Hint = hint;
            }

            return option;
        }

        /// <summary>
        /// ID赋值
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        protected void AssignmentEntityID(TEntity entity, long id)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        protected IAggregateFluent<TEntity> CreateAggregate(FilterDefinition<TEntity> filter
            , SortDefinition<TEntity> sort
            , int limit = 0, int skip = 0
            , ReadPreference readPreference = null)
        {
            var fluent = GetCollection(readPreference).Aggregate();            

            if (sort != null)
            {
                fluent = fluent.Sort(sort);
            }

            fluent = fluent.Match(filter);

            if (skip > 0)
            {
                fluent = fluent.Skip(skip);
            }

            if (limit > 0)
            {
                fluent = fluent.Limit(limit);
            }

            return fluent;
        }
    }
}
