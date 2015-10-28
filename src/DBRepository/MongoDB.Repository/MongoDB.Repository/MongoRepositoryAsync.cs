using DB.Repository;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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
    /// 异步仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MongoRepositoryAsync<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        private MongoSessionAsync _mongoSession;

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
        public MongoRepositoryAsync(string connString, string dbName, ReadPreference readPreference = null, MongoSequence sequence = null)
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
        /// 获取系统当前时间
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> GetSysDateTimeAsync()
        {
            var result = await Database.RunCommandAsync<BsonValue>("new Date()");
            return result.ToUniversalTime();
        }

        /// <summary>
        /// 创建自增ID
        /// </summary>
        public Task<long> CreateIncIDAsync()
        {
            return _mongoSession.CreateIncIDAsync<TEntity>();
        }

        /// <summary>
        /// 创建自增ID
        /// </summary>
        /// <param name="entity"></param>
        public async Task CreateIncIDAsync(TEntity entity)
        {
            long _id = 0;
            _id = await _mongoSession.CreateIncIDAsync<TEntity>();

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
        /// 添加数据
        /// </summary>
        /// <param name="entity">待添加数据</param>
        /// <returns></returns>
        public async Task InsertAsync(TEntity entity)
        {
            if (entity is IAutoIncr)
            {
                await CreateIncIDAsync(entity);
            }
            //await _mongoSession.InsertAsync(entity);
            await _mongoSession.GetCollection<TEntity>().InsertOneAsync(entity);
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="entitys">待添加数据集合</param>
        /// <returns></returns>
        public async Task InsertBatchAsync(IEnumerable<TEntity> entitys)
        {
            foreach (var entity in entitys)
            {
                if (entity is IAutoIncr)
                {
                    await CreateIncIDAsync(entity);
                }
            }
            //await _mongoSession.InsertBatchAsync(entitys);
            await _mongoSession.GetCollection<TEntity>().InsertManyAsync(entitys);
        }

        /// <summary>
        /// 根据实体创建UpdateDefinition
        /// </summary>
        /// <param name="updateEntity"></param>
        /// <param name="isUpsert"></param>
        /// <returns></returns>
        public async Task<UpdateDefinition<TEntity>> CreateUpdateDefinition(TEntity updateEntity, bool isUpsert = false)
        {
            long id = 0;
            BsonDocument bsDoc = updateEntity.ToBsonDocument();
            if (isUpsert && updateEntity is IAutoIncr)
            {
                id = await CreateIncIDAsync();
                bsDoc.Remove("_id");
            }
            UpdateDefinition<TEntity> update = new UpdateDocument("$set", bsDoc);// string.Concat("{$set:", bsDoc.ToJson(), "}");
            if (isUpsert && updateEntity is IAutoIncr)
            {
                update = update.SetOnInsert("_id", id);
            }

            return update;
        }

        /// <summary>
        /// 修改单条数据
        /// 如果isUpsert 为 true ，且updateEntity继承IAutoIncr，则ID内部会自增
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="updateEntity">更新实体（不是replace，updateEntity不会减少原实体字段）</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public async Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false)
        {
            var filter = Filter.Eq(x => x.ID, updateEntity.ID);
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinition(updateEntity, isUpsert);

            return await _mongoSession.GetCollection<TEntity>().UpdateOneAsync(filterExp, update, option);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="updateEntity">更新实体（不是replace，updateEntity不会减少原实体字段）</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinition(updateEntity, isUpsert);

            return await _mongoSession.GetCollection<TEntity>().UpdateOneAsync(filter, update, option);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public async Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return await _mongoSession.GetCollection<TEntity>().UpdateOneAsync(filterExp, update, option);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return await _mongoSession.GetCollection<TEntity>().UpdateOneAsync(filter, update, option);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public async Task<UpdateResult> UpdateManyAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return await _mongoSession.GetCollection<TEntity>().UpdateManyAsync(filterExp, update, option);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return await _mongoSession.GetCollection<TEntity>().UpdateManyAsync(filter, update, option);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="update"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;

            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;
            return await _mongoSession.GetCollection<TEntity>().FindOneAndUpdateAsync(filterExp, update, option);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="updateEntity">更新实体</param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;

            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinition(updateEntity, isUpsert);

            return await _mongoSession.GetCollection<TEntity>().FindOneAndUpdateAsync(filterExp, update, option);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="update"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , SortDefinition<TEntity> sort = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;
            return await _mongoSession.GetCollection<TEntity>().FindOneAndUpdateAsync(filter, update, option);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="updateEntity">更新实体</param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false
            , SortDefinition<TEntity> sort = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinition(updateEntity, isUpsert);

            return await _mongoSession.GetCollection<TEntity>().FindOneAndUpdateAsync(filter, update, option);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="entity"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndReplaceAsync(Expression<Func<TEntity, bool>> filterExp, TEntity entity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && entity is IAutoIncr)
            {
                await CreateIncIDAsync(entity);
            }
            return await _mongoSession.GetCollection<TEntity>().FindOneAndReplaceAsync(filterExp, entity, option);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="entity"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndReplaceAsync(FilterDefinition<TEntity> filter, TEntity entity, bool isUpsert = false, SortDefinition<TEntity> sort = null)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && entity is IAutoIncr)
            {
                await CreateIncIDAsync(entity);
            }
            return await _mongoSession.GetCollection<TEntity>().FindOneAndReplaceAsync(filter, entity, option);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndDeleteAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = sort;
            return await _mongoSession.GetCollection<TEntity>().FindOneAndDeleteAsync(filter, option);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndDeleteAsync(Expression<Func<TEntity, bool>> filterExp
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            return await _mongoSession.GetCollection<TEntity>().FindOneAndDeleteAsync(filterExp, option);
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="id">ID</param>
        public async Task<DeleteResult> DeleteOneAsync(TKey id)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.ID, id);
            return await _mongoSession.GetCollection<TEntity>().DeleteOneAsync(filter);
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        public async Task<DeleteResult> DeleteOneAsync(FilterDefinition<TEntity> filter)
        {
            return await _mongoSession.GetCollection<TEntity>().DeleteOneAsync(filter);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        public async Task<DeleteResult> DeleteOneAsync(Expression<Func<TEntity, bool>> filterExp)
        {
            return await _mongoSession.GetCollection<TEntity>().DeleteOneAsync(filterExp);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<TEntity> filter)
        {
            return await _mongoSession.GetCollection<TEntity>().DeleteManyAsync(filter);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        public async Task<DeleteResult> DeleteManyAsync(Expression<Func<TEntity, bool>> filterExp)
        {
            return await _mongoSession.GetCollection<TEntity>().DeleteManyAsync(filterExp);
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
