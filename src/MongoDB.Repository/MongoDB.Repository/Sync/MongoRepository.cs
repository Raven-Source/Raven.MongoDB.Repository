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
    /// 仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MongoRepository<TEntity, TKey>: MongoReaderRepository<TEntity, TKey>
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
        public MongoRepository(string connString, string dbName, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null)
            :base(connString, dbName, writeConcern, readPreference, sequence)
        {
        }        

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="entity">待添加数据</param>
        /// <returns></returns>
        public void Insert(TEntity entity)
        {
            if (entity is IAutoIncr)
            {
                CreateIncIDAsync(entity);
            }
            _mongoSession.GetCollection<TEntity>().InsertOneAsync(entity).Wait();
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="entitys">待添加数据集合</param>
        /// <returns></returns>
        public void InsertBatch(IEnumerable<TEntity> entitys)
        {
            //需要自增的实体
            if (entitys.First() is IAutoIncr)
            {
                int count = entitys.Count();
                //自增ID值
                long id = CreateIncIDAsync(count);

                foreach (var entity in entitys)
                {
                    AssignmentEntityID(entity, id--);
                }
            }

            //await _mongoSession.InsertBatchAsync(entitys);
            _mongoSession.GetCollection<TEntity>().InsertManyAsync(entitys).Wait();
        }

        /// <summary>
        /// 根据实体创建UpdateDefinition
        /// </summary>
        /// <param name="updateEntity"></param>
        /// <param name="isUpsert"></param>
        /// <returns></returns>
        public UpdateDefinition<TEntity> CreateUpdateDefinition(TEntity updateEntity, bool isUpsert = false)
        {
            long id = 0;
            BsonDocument bsDoc = updateEntity.ToBsonDocument();
            if (isUpsert && updateEntity is IAutoIncr)
            {
                id = CreateIncIDAsync();
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
        public UpdateResult UpdateOne(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false)
        {
            var filter = Filter.Eq(x => x.ID, updateEntity.ID);
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = CreateUpdateDefinition(updateEntity, isUpsert);

            return _mongoSession.GetCollection<TEntity>().UpdateOneAsync(filterExp, update, option).Result;
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="updateEntity">更新实体（不是replace，updateEntity不会减少原实体字段）</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public UpdateResult UpdateOne(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = CreateUpdateDefinition(updateEntity, isUpsert);

            return _mongoSession.GetCollection<TEntity>().UpdateOneAsync(filter, update, option).Result;
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public UpdateResult UpdateOne(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return _mongoSession.GetCollection<TEntity>().UpdateOneAsync(filterExp, update, option).Result;
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public UpdateResult UpdateOne(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return _mongoSession.GetCollection<TEntity>().UpdateOneAsync(filter, update, option).Result;
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public UpdateResult UpdateMany(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return _mongoSession.GetCollection<TEntity>().UpdateManyAsync(filterExp, update, option).Result;
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        public UpdateResult UpdateMany(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return _mongoSession.GetCollection<TEntity>().UpdateManyAsync(filter, update, option).Result;
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
        public TEntity FindOneAndUpdate(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;

            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;
            return _mongoSession.GetCollection<TEntity>().FindOneAndUpdateAsync(filterExp, update, option).Result;
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
        public TEntity FindOneAndUpdate(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            UpdateDefinition<TEntity> update = CreateUpdateDefinition(updateEntity, isUpsert);

            return _mongoSession.GetCollection<TEntity>().FindOneAndUpdateAsync(filterExp, update, option).Result;
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="update"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public TEntity FindOneAndUpdate(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , SortDefinition<TEntity> sort = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            return _mongoSession.GetCollection<TEntity>().FindOneAndUpdateAsync(filter, update, option).Result;
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="updateEntity">更新实体</param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public TEntity FindOneAndUpdate(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false
            , SortDefinition<TEntity> sort = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;
            UpdateDefinition<TEntity> update = CreateUpdateDefinition(updateEntity, isUpsert);

            return _mongoSession.GetCollection<TEntity>().FindOneAndUpdateAsync(filter, update, option).Result;
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
        public TEntity FindOneAndReplace(Expression<Func<TEntity, bool>> filterExp, TEntity entity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && entity is IAutoIncr)
            {
                CreateIncIDAsync(entity);
            }
            return _mongoSession.GetCollection<TEntity>().FindOneAndReplaceAsync(filterExp, entity, option).Result;
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="entity"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public TEntity FindOneAndReplace(FilterDefinition<TEntity> filter, TEntity entity, bool isUpsert = false, SortDefinition<TEntity> sort = null)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && entity is IAutoIncr)
            {
                CreateIncIDAsync(entity);
            }
            return _mongoSession.GetCollection<TEntity>().FindOneAndReplaceAsync(filter, entity, option).Result;
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public TEntity FindOneAndDelete(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = sort;
            return _mongoSession.GetCollection<TEntity>().FindOneAndDeleteAsync(filter, option).Result;
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public TEntity FindOneAndDelete(Expression<Func<TEntity, bool>> filterExp
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = _mongoSession.CreateSortDefinition(sortExp, sortType);
            return _mongoSession.GetCollection<TEntity>().FindOneAndDeleteAsync(filterExp, option).Result;
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="id">ID</param>
        public DeleteResult DeleteOne(TKey id)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.ID, id);
            return _mongoSession.GetCollection<TEntity>().DeleteOneAsync(filter).Result;
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        public DeleteResult DeleteOne(FilterDefinition<TEntity> filter)
        {
            return _mongoSession.GetCollection<TEntity>().DeleteOneAsync(filter).Result;
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        public DeleteResult DeleteOne(Expression<Func<TEntity, bool>> filterExp)
        {
            return _mongoSession.GetCollection<TEntity>().DeleteOneAsync(filterExp).Result;
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        public DeleteResult DeleteMany(FilterDefinition<TEntity> filter)
        {
            return _mongoSession.GetCollection<TEntity>().DeleteManyAsync(filter).Result;
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        public DeleteResult DeleteMany(Expression<Func<TEntity, bool>> filterExp)
        {
            return _mongoSession.GetCollection<TEntity>().DeleteManyAsync(filterExp).Result;
        }

    }
}
