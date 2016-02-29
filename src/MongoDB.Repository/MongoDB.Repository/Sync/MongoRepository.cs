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
    public class MongoRepository<TEntity, TKey>: MongoReaderRepository<TEntity, TKey>, IRepository<TEntity, TKey>
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
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public void Insert(TEntity entity
            , WriteConcern writeConcern = null)
        {
            if (entity is IAutoIncr)
            {
                CreateIncID(entity);
            }
            base.GetCollection(writeConcern).InsertOne(entity);
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="entitys">待添加数据集合</param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public void InsertBatch(IEnumerable<TEntity> entitys
            , WriteConcern writeConcern = null)
        {
            //需要自增的实体
            if (entitys.First() is IAutoIncr)
            {
                int count = entitys.Count();
                //自增ID值
                long id = CreateIncID(count);
                id = id - count;
                foreach (var entity in entitys)
                {
                    AssignmentEntityID(entity, ++id);
                }
            }

            //await base.InsertBatchAsync(entitys);
            base.GetCollection(writeConcern).InsertMany(entitys);
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
            bsDoc.Remove(Util.PrimaryKeyName);
            if (isUpsert && updateEntity is IAutoIncr)
            {
                id = CreateIncID();
            }
            UpdateDefinition<TEntity> update = new UpdateDocument("$set", bsDoc);// string.Concat("{$set:", bsDoc.ToJson(), "}");
            if (isUpsert && updateEntity is IAutoIncr)
            {
                update = update.SetOnInsert(Util.PrimaryKeyName, id);
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
        /// <param name="writeConcern">访问设置</param>
        public UpdateResult UpdateOne(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            var filter = Filter.Eq(x => x.ID, updateEntity.ID);
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = CreateUpdateDefinition(updateEntity, isUpsert);

            return base.GetCollection(writeConcern).UpdateOne(filterExp, update, option);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="updateEntity">更新实体（不是replace，updateEntity不会减少原实体字段）</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public UpdateResult UpdateOne(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = CreateUpdateDefinition(updateEntity, isUpsert);

            return base.GetCollection(writeConcern).UpdateOne(filter, update, option);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public UpdateResult UpdateOne(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateOne(filterExp, update, option);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="updateExp">更新内容表达式</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public UpdateResult UpdateOne(Expression<Func<TEntity, bool>> filterExp, Func<UpdateDefinitionBuilder<TEntity>, UpdateDefinition<TEntity>> updateExp, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            var update = updateExp(Update);

            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateOne(filterExp, update, option);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public UpdateResult UpdateOne(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateOne(filter, update, option);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public UpdateResult UpdateMany(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateMany(filterExp, update, option);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="updateExp">更新内容表达式</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public UpdateResult UpdateMany(Expression<Func<TEntity, bool>> filterExp, Func<UpdateDefinitionBuilder<TEntity>, UpdateDefinition<TEntity>> updateExp, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            var update = updateExp(Update);

            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateMany(filterExp, update, option);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public UpdateResult UpdateMany(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateMany(filter, update, option);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="update"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public TEntity FindOneAndUpdate(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;

            option.Sort = base.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;
            return base.GetCollection(writeConcern).FindOneAndUpdate(filterExp, update, option);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="updateEntity">更新实体</param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public TEntity FindOneAndUpdate(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = base.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            UpdateDefinition<TEntity> update = CreateUpdateDefinition(updateEntity, isUpsert);

            return base.GetCollection(writeConcern).FindOneAndUpdate(filterExp, update, option);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="update"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public TEntity FindOneAndUpdate(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            return base.GetCollection(writeConcern).FindOneAndUpdate(filter, update, option);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="updateExp"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public TEntity FindOneAndUpdate(Expression<Func<TEntity, bool>> filterExp, Func<UpdateDefinitionBuilder<TEntity>, UpdateDefinition<TEntity>> updateExp, bool isUpsert = false
            , SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            var update = updateExp(Update);

            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;
            return base.GetCollection(writeConcern).FindOneAndUpdate(filterExp, update, option);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="updateEntity">更新实体</param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public TEntity FindOneAndUpdate(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false
            , SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;
            UpdateDefinition<TEntity> update = CreateUpdateDefinition(updateEntity, isUpsert);

            return base.GetCollection(writeConcern).FindOneAndUpdate(filter, update, option);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="entity"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public TEntity FindOneAndReplace(Expression<Func<TEntity, bool>> filterExp, TEntity entity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , WriteConcern writeConcern = null)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = base.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && entity is IAutoIncr)
            {
                CreateIncID(entity);
            }
            return base.GetCollection(writeConcern).FindOneAndReplace(filterExp, entity, option);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="entity"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public TEntity FindOneAndReplace(FilterDefinition<TEntity> filter, TEntity entity, bool isUpsert = false, SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && entity is IAutoIncr)
            {
                CreateIncID(entity);
            }
            return base.GetCollection(writeConcern).FindOneAndReplace(filter, entity, option);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public TEntity FindOneAndDelete(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = sort;
            return base.GetCollection(writeConcern).FindOneAndDelete(filter, option);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public TEntity FindOneAndDelete(Expression<Func<TEntity, bool>> filterExp
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , WriteConcern writeConcern = null)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = base.CreateSortDefinition(sortExp, sortType);
            return base.GetCollection(writeConcern).FindOneAndDelete(filterExp, option);
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="writeConcern">访问设置</param>
        public DeleteResult DeleteOne(TKey id
            , WriteConcern writeConcern = null)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.ID, id);
            return base.GetCollection(writeConcern).DeleteOne(filter);
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public DeleteResult DeleteOne(FilterDefinition<TEntity> filter
            , WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteOne(filter);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public DeleteResult DeleteOne(Expression<Func<TEntity, bool>> filterExp
            , WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteOne(filterExp);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public DeleteResult DeleteMany(FilterDefinition<TEntity> filter
            , WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteMany(filter);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public DeleteResult DeleteMany(Expression<Func<TEntity, bool>> filterExp
            , WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteMany(filterExp);
        }

    }
}
