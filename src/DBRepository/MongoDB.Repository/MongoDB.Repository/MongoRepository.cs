using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;
using Repository.IEntity;
using MongoDB.Bson;
using System.Linq.Expressions;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace MongoDB.Repository
{
    /// <summary>
    /// MongoDB仓储层基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <remarks>add by liangyi on 2015/05/26</remarks>
    public class MongoRepository<TEntity, TKey> : IMongoRepository<TEntity, TKey>
        where TEntity : class,IEntity<TKey>, new()
    {
        MongoSession _mongoSession;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库链接字符串</param>
        /// <param name="dbName">数据库名称</param>
        public MongoRepository(string connString, string dbName, ReadPreference readPreference = null, MongoSequence sequence = null)
        {
            _mongoSession = new MongoSession(connString, dbName, readPreference: readPreference, sequence: sequence);
        }

        /// <summary>
        /// 获取支持linq操作的collection集合
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> QueryableCollection()
        {
            return _mongoSession.mongoDatabase.GetCollection<TEntity>(typeof(TEntity).Name).AsQueryable<TEntity>();
        }

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Get(TKey id)
        {
            var query = MongoDB.Driver.Builders.Query<TEntity>.EQ(x => x.ID, id);
            return _mongoSession.Get<TEntity>(query);
        }

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fieldPredicate">查询字段表达式</param>
        /// <param name="revolt">true|不加载字段,fale|加载字段</param>
        /// <returns></returns>
        public TEntity Get(TKey id, Expression<Func<TEntity, dynamic>> fieldPredicate, bool revolt = false)
        {
            var query = MongoDB.Driver.Builders.Query<TEntity>.EQ(x => x.ID, id);
            return _mongoSession.Get<TEntity>(query, null, GetFields(fieldPredicate, revolt));
        }

        /// <summary>
        /// 根据id和其他条件获取实体
        /// </summary>
        /// <param name="filterPredicate"条件></param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> filterPredicate)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = MongoDB.Driver.Builders.Query<TEntity>.Where(filterPredicate);
            }
            return _mongoSession.Get<TEntity>(query);
        }

        /// <summary>
        /// 获取可取相应的字段
        /// </summary>
        /// <param name="filterPredicate">条件表达式</param>
        /// <param name="fieldExpression">加载字段表达式</param>
        /// <param name="revolt">true|不加载字段,fale|加载字段</param>
        /// <param name="sortBy">默认倒叙</param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, dynamic>> fieldPredicate = null, bool revolt = false, params Expression<Func<TEntity, dynamic>>[] sortBy)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = MongoDB.Driver.Builders.Query<TEntity>.Where(filterPredicate);
            }
            var sort = SortBy<TEntity>.Descending(sortBy);
            return _mongoSession.Get<TEntity>(query, sort, GetFields(fieldPredicate, revolt));
        }

        /// <summary>
        /// 根据条件获取
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <param name="sort"></param>
        /// <param name="fieldPredicate">加载字段表达式</param>
        /// <param name="revolt">true|不加载字段,fale|加载字段</param>
        /// <returns></returns>
        public TEntity Get(IMongoQuery query, IMongoSortBy sort = null, Expression<Func<TEntity, dynamic>> fieldPredicate = null, bool revolt = false)
        {
            return _mongoSession.Get<TEntity>(query, sort, GetFields(fieldPredicate, revolt));
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="entity"></param>
        public TEntity Insert(TEntity entity)
        {
            return Insert(entity, true);
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="entity"></param>
        public TEntity Insert(TEntity entity, bool isCreateIncID = true)
        {
            if (isCreateIncID)
            {
                long _id = 0;
                if (entity is IAutoIncr)
                {
                    _id = _mongoSession.CreateIncID<TEntity>();
                    if ((entity as IEntity<TKey>).ID is int)
                    {
                        (entity as IEntity<int>).ID = (int)_id;
                    }
                    else
                    {
                        (entity as IEntity<long>).ID = _id;
                    }
                }
            }

            _mongoSession.Insert<TEntity>(entity);
            return entity;
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="entitys">实体对象集合</param>
        /// <param name="isCreateIncID">true|进行自增操作;false|不做自增操作</param>
        /// <returns></returns>
        public virtual bool InsertBatch(IEnumerable<TEntity> entitys, bool isCreateIncID = true)
        {
            foreach (var entity in entitys)
            {
                if (isCreateIncID)
                {
                    long _id = 0;
                    if (entity is IAutoIncr)
                    {
                        _id = _mongoSession.CreateIncID<TEntity>();
                        if ((entity as IEntity<TKey>).ID is int)
                        {
                            (entity as IEntity<int>).ID = (int)_id;
                        }
                        else
                        {
                            (entity as IEntity<long>).ID = _id;
                        }
                    }
                }
            }

            _mongoSession.InsertBatch<TEntity>(entitys);
            return true;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entityToUpdate"></param>
        [Obsolete("MongoDB数据库，在分布式系统中，使用全量更新有很大的风险，因此方法已屏蔽", true)]
        public TEntity Update(TEntity entityToUpdate)
        {
            throw new NotImplementedException();
            //_mongoSession.Update<TEntity>(entityToUpdate);
            //return entityToUpdate;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="filterPredicate">查询表达式</param>
        /// <param name="updatePredicate">更新内容</param>
        public void Update(Expression<Func<TEntity, bool>> filterPredicate, Func<MongoDB.Driver.Builders.UpdateBuilder<TEntity>, IMongoUpdate> updatePredicate)
        {
            Update(filterPredicate, updatePredicate, UpdateFlags.None);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="filterPredicate">查询表达式</param>
        /// <param name="updatePredicate">更新内容</param>
        /// <param name="flag">更新方式</param>
        public void Update(Expression<Func<TEntity, bool>> filterPredicate, Func<MongoDB.Driver.Builders.UpdateBuilder<TEntity>, IMongoUpdate> updatePredicate, UpdateFlags flag)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = MongoDB.Driver.Builders.Query<TEntity>.Where(filterPredicate);
            }

            var update = updatePredicate(new UpdateBuilder<TEntity>());
            _mongoSession.Update<TEntity>(query, update, flag);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="flag">更新方式</param>
        public void Update(IMongoQuery query, IMongoUpdate update, UpdateFlags flag = UpdateFlags.None)
        {
            _mongoSession.Update<TEntity>(query, update, flag);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        public void Delete(TKey id)
        {
            var query = MongoDB.Driver.Builders.Query<TEntity>.EQ(x => x.ID, id);
            _mongoSession.Remove<TEntity>(query);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entityToDelete"></param>
        public void Delete(TEntity entityToDelete)
        {
            var query = MongoDB.Driver.Builders.Query<TEntity>.EQ(x => x.ID, entityToDelete.ID);
            _mongoSession.Remove<TEntity>(query);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="filterPredicate">条件表达式</param>
        public virtual void Delete(Expression<Func<TEntity, bool>> filterPredicate)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = MongoDB.Driver.Builders.Query<TEntity>.Where(filterPredicate);
            }
            _mongoSession.Remove<TEntity>(query);
        }

        /// <summary>
        /// 查询数量
        /// </summary>
        /// <param name="filterPredicate">查询表达式</param>
        /// <returns></returns>
        public long Count(Expression<Func<TEntity, bool>> filterPredicate = null)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = MongoDB.Driver.Builders.Query<TEntity>.Where(filterPredicate);
                return Count(query);
            }
            return Count();
        }

        /// <summary>
        /// 查询数量
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <returns></returns>
        public long Count(IMongoQuery query)
        {
            return _mongoSession.Count<TEntity>(query);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="filterPredicate">查询表达式</param>
        /// <returns></returns>
        public bool IsExists(Expression<Func<TEntity, bool>> filterPredicate)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = MongoDB.Driver.Builders.Query<TEntity>.Where(filterPredicate);
            }

            return IsExists(query);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <returns></returns>
        public bool IsExists(IMongoQuery query)
        {
            if (_mongoSession.Count<TEntity>(query) > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="orderby">排序条件</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetList(Expression<Func<TEntity, bool>> filterPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null)
        {
            IQueryable<TEntity> query = null;
            if (filterPredicate != null)
                query = QueryableCollection().Where(filterPredicate);
            else
                query = QueryableCollection();

            if (orderby != null)
            {
                query = orderby(query);
            }

            return query;
        }

        /// <summary>
        /// 获取数据(只获取部分字段)
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="fieldPredicate">取需求字段匿名类</param>
        /// <param name="orderby">排序条件</param>
        /// <returns></returns>
        public IQueryable<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null)
        {
            IQueryable<TEntity> query = this.GetList(filterPredicate, orderby);
            return query.Select(fieldPredicate);
        }

        /// <summary>
        /// 获取数据（分页）
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数据数</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetList(Expression<Func<TEntity, bool>> filterPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby, int pageIndex = 1, int pageSize = 10)
        {
            IQueryable<TEntity> query = this.GetList(filterPredicate, orderby);
            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 获取数据（分页）
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="fieldPredicate">取需求字段匿名类</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数据数</param>
        /// <returns></returns>
        public IQueryable<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby, int pageIndex = 1, int pageSize = 10)
        {
            IQueryable<TResult> query = this.GetList(filterPredicate, fieldPredicate, orderby);
            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterPredicate">查询表达式</param>
        /// <param name="updatePredicate">更新内容</param>
        /// <param name="sortBy">默认倒叙</param>
        /// <returns></returns>
        public TEntity FindAndModify(Expression<Func<TEntity, bool>> filterPredicate, Func<MongoDB.Driver.Builders.UpdateBuilder<TEntity>, IMongoUpdate> updatePredicate, params Expression<Func<TEntity, dynamic>>[] sortBy)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = MongoDB.Driver.Builders.Query<TEntity>.Where(filterPredicate);
            }

            var update = updatePredicate(new UpdateBuilder<TEntity>());
            var sort = SortBy<TEntity>.Descending(sortBy);
            return _mongoSession.FindAndModify<TEntity>(query, sort, update);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterPredicate"></param>
        /// <param name="update"></param>
        /// <param name="sortBy">默认倒叙</param>
        /// <returns></returns>
        public TEntity FindAndModify(Expression<Func<TEntity, bool>> filterPredicate, IMongoUpdate update, params Expression<Func<TEntity, dynamic>>[] sortBy)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = MongoDB.Driver.Builders.Query<TEntity>.Where(filterPredicate);
            }
            var sort = SortBy<TEntity>.Descending(sortBy);
            return _mongoSession.FindAndModify<TEntity>(query, sort, update);
        }

        #region 获取字段

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="fieldsPredicate"></param>
        /// <param name="revolt">true|不加载字段,fale|加载字段</param>
        /// <returns></returns>
        protected FieldsDocument GetFields(Expression<Func<TEntity, dynamic>> fieldsPredicate = null, bool revolt = false)
        {
            FieldsDocument fieldDocument = null;
            int val = revolt ? 0 : 1;
            if (fieldsPredicate != null)
            {
                fieldDocument = new FieldsDocument();

                var members = (fieldsPredicate.Body as NewExpression).Members;
                foreach (var m in members)
                {
                    fieldDocument.Add(new BsonElement(m.Name, val));
                }

            }
            return fieldDocument;
        }

        #endregion
    }
}
