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
    public class MongoRepositoryAsync<TEntity, TKey>: MongoReaderRepositoryAsync<TEntity, TKey>
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
        public MongoRepositoryAsync(string connString, string dbName, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null)
            :base(connString, dbName, writeConcern, readPreference, sequence)
        {
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
                await CreateIncIDAsync(entity).ConfigureAwait(false);
            }
            await _mongoSession.GetCollection<TEntity>().InsertOneAsync(entity).ConfigureAwait(false);
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="entitys">待添加数据集合</param>
        /// <returns></returns>
        public async Task InsertBatchAsync(IEnumerable<TEntity> entitys)
        {
            //需要自增的实体
            if (entitys.First() is IAutoIncr)
            {
                int count = entitys.Count();
                //自增ID值
                long id = await CreateIncIDAsync(count).ConfigureAwait(false);

                foreach (var entity in entitys)
                {
                    AssignmentEntityID(entity, id--);
                }
            }

            //await _mongoSession.InsertBatchAsync(entitys);
            await _mongoSession.GetCollection<TEntity>().InsertManyAsync(entitys).ConfigureAwait(false);
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
                id = await CreateIncIDAsync().ConfigureAwait(false);
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
        /// <param name="settings">访问设置</param>
        public async Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false
            , MongoCollectionSettings settings = null)
        {
            var filter = Filter.Eq(x => x.ID, updateEntity.ID);
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinition(updateEntity, isUpsert).ConfigureAwait(false);

            return await _mongoSession.GetCollection<TEntity>(settings).UpdateOneAsync(filterExp, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="updateEntity">更新实体（不是replace，updateEntity不会减少原实体字段）</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="settings">访问设置</param>
        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false
            , MongoCollectionSettings settings = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinition(updateEntity, isUpsert).ConfigureAwait(false);

            return await _mongoSession.GetCollection<TEntity>(settings).UpdateOneAsync(filter, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="settings">访问设置</param>
        public async Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , MongoCollectionSettings settings = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return await _mongoSession.GetCollection<TEntity>(settings).UpdateOneAsync(filterExp, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="settings">访问设置</param>
        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , MongoCollectionSettings settings = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return await _mongoSession.GetCollection<TEntity>(settings).UpdateOneAsync(filter, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="settings">访问设置</param>
        public async Task<UpdateResult> UpdateManyAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , MongoCollectionSettings settings = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return await _mongoSession.GetCollection<TEntity>(settings).UpdateManyAsync(filterExp, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="settings">访问设置</param>
        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , MongoCollectionSettings settings = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return await _mongoSession.GetCollection<TEntity>(settings).UpdateManyAsync(filter, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="update"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="settings">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , MongoCollectionSettings settings = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;

            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;
            return await _mongoSession.GetCollection<TEntity>(settings).FindOneAndUpdateAsync(filterExp, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="updateEntity">更新实体</param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="settings">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , MongoCollectionSettings settings = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinition(updateEntity, isUpsert).ConfigureAwait(false);

            return await _mongoSession.GetCollection<TEntity>(settings).FindOneAndUpdateAsync(filterExp, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="update"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="settings">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , SortDefinition<TEntity> sort = null
            , MongoCollectionSettings settings = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;
            return await _mongoSession.GetCollection<TEntity>(settings).FindOneAndUpdateAsync(filter, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="updateEntity">更新实体</param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="settings">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false
            , SortDefinition<TEntity> sort = null
            , MongoCollectionSettings settings = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinition(updateEntity, isUpsert).ConfigureAwait(false);

            return await _mongoSession.GetCollection<TEntity>(settings).FindOneAndUpdateAsync(filter, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="entity"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="settings">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndReplaceAsync(Expression<Func<TEntity, bool>> filterExp, TEntity entity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , MongoCollectionSettings settings = null)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && entity is IAutoIncr)
            {
                await CreateIncIDAsync(entity).ConfigureAwait(false);
            }
            return await _mongoSession.GetCollection<TEntity>(settings).FindOneAndReplaceAsync(filterExp, entity, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="entity"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="settings">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndReplaceAsync(FilterDefinition<TEntity> filter, TEntity entity, bool isUpsert = false, SortDefinition<TEntity> sort = null
            , MongoCollectionSettings settings = null)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && entity is IAutoIncr)
            {
                await CreateIncIDAsync(entity).ConfigureAwait(false);
            }
            return await _mongoSession.GetCollection<TEntity>(settings).FindOneAndReplaceAsync(filter, entity, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="settings">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndDeleteAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null
            , MongoCollectionSettings settings = null)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = sort;
            return await _mongoSession.GetCollection<TEntity>(settings).FindOneAndDeleteAsync(filter, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="settings">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndDeleteAsync(Expression<Func<TEntity, bool>> filterExp
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , MongoCollectionSettings settings = null)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            return await _mongoSession.GetCollection<TEntity>(settings).FindOneAndDeleteAsync(filterExp, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="settings">访问设置</param>
        public async Task<DeleteResult> DeleteOneAsync(TKey id
            , MongoCollectionSettings settings = null)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.ID, id);
            return await _mongoSession.GetCollection<TEntity>(settings).DeleteOneAsync(filter).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="settings">访问设置</param>
        public async Task<DeleteResult> DeleteOneAsync(FilterDefinition<TEntity> filter
            , MongoCollectionSettings settings = null)
        {
            return await _mongoSession.GetCollection<TEntity>(settings).DeleteOneAsync(filter).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        /// <param name="settings">访问设置</param>
        public async Task<DeleteResult> DeleteOneAsync(Expression<Func<TEntity, bool>> filterExp
            , MongoCollectionSettings settings = null)
        {
            return await _mongoSession.GetCollection<TEntity>(settings).DeleteOneAsync(filterExp).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="settings">访问设置</param>
        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<TEntity> filter
            , MongoCollectionSettings settings = null)
        {
            return await _mongoSession.GetCollection<TEntity>(settings).DeleteManyAsync(filter).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        /// <param name="settings">访问设置</param>
        public async Task<DeleteResult> DeleteManyAsync(Expression<Func<TEntity, bool>> filterExp
            , MongoCollectionSettings settings = null)
        {
            return await _mongoSession.GetCollection<TEntity>(settings).DeleteManyAsync(filterExp).ConfigureAwait(false);
        }

    }
}
