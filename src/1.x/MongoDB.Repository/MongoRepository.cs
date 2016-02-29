
using DB.Repository;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MongoDB.Repository
{
    public class MongoRepository<TEntity, TKey> : IMongoRepository<TEntity, TKey>, IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        private MongoSession _mongoSession;
        protected const string DEFAULT_CONFIG_NODE = "MongoDB";
        protected const string DEFAULT_DB_NAME = "Mallcoo";
        private static Type tBsonIdType;

        static MongoRepository()
        {
            MongoRepository<TEntity, TKey>.tBsonIdType = typeof(BsonIdAttribute);
        }

        public MongoRepository(string dbName = "Mallcoo", string configNode = "MongoDB", ReadPreference readPreference = null, WriteConcern writeConcern = null)
        {
            this._mongoSession = new MongoSession(dbName, configNode, writeConcern, null, false, readPreference);
        }

        public long Count(IMongoQuery query)
        {
            return this._mongoSession.Count<TEntity>(query);
        }

        public long Count(Expression<Func<TEntity, bool>> filterPredicate = null)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = Query<TEntity>.Where(filterPredicate);
                return this.Count(query);
            }
            return this.Count((Expression<Func<TEntity, bool>>)null);
        }

        public void Delete(TEntity entityToDelete)
        {
            IMongoQuery query = Query<TEntity>.EQ<TKey>(x => x.ID, entityToDelete.ID);
            this._mongoSession.Remove<TEntity>(query, 0);
        }

        public void Delete(TKey id)
        {
            IMongoQuery query = Query<TEntity>.EQ<TKey>(x => x.ID, id);
            this._mongoSession.Remove<TEntity>(query, 0);
        }

        public virtual void Delete(Expression<Func<TEntity, bool>> filterPredicate)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = Query<TEntity>.Where(filterPredicate);
            }
            this._mongoSession.Remove<TEntity>(query, 0);
        }

        public TEntity FindAndModify(Expression<Func<TEntity, bool>> filterPredicate, IMongoUpdate update, params Expression<Func<TEntity, object>>[] sortBy)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = Query<TEntity>.Where(filterPredicate);
            }
            SortByBuilder<TEntity> builder = SortBy<TEntity>.Descending(sortBy);
            return this._mongoSession.FindAndModify<TEntity>(query, builder, update);
        }

        public TEntity FindAndModify(Expression<Func<TEntity, bool>> filterPredicate, Func<UpdateBuilder<TEntity>, IMongoUpdate> updatePredicate, params Expression<Func<TEntity, object>>[] sortBy)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = Query<TEntity>.Where(filterPredicate);
            }
            IMongoUpdate update = updatePredicate(new UpdateBuilder<TEntity>());
            SortByBuilder<TEntity> builder = SortBy<TEntity>.Descending(sortBy);
            return this._mongoSession.FindAndModify<TEntity>(query, builder, update);
        }

        public TEntity Get(TKey id)
        {
            IMongoQuery query = Query<TEntity>.EQ<TKey>(x => x.ID, id);
            return this._mongoSession.Get<TEntity>(query, null, null);
        }

        public TEntity Get(Expression<Func<TEntity, bool>> filterPredicate)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = Query<TEntity>.Where(filterPredicate);
            }
            return this._mongoSession.Get<TEntity>(query, null, null);
        }

        public TEntity Get(TKey id, Expression<Func<TEntity, object>> fieldPredicate, bool revolt = false)
        {
            IMongoQuery query = Query<TEntity>.EQ<TKey>(x => x.ID, id);
            return this._mongoSession.Get<TEntity>(query, null, this.GetFields(fieldPredicate, revolt));
        }

        public TEntity Get(IMongoQuery query, IMongoSortBy sort = null, Expression<Func<TEntity, object>> fieldPredicate = null, bool revolt = false)
        {
            return this._mongoSession.Get<TEntity>(query, sort, this.GetFields(fieldPredicate, revolt));
        }

        public TEntity Get(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, object>> fieldPredicate = null, bool revolt = false, params Expression<Func<TEntity, object>>[] sortBy)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = Query<TEntity>.Where(filterPredicate);
            }
            SortByBuilder<TEntity> builder = SortBy<TEntity>.Descending(sortBy);
            return this._mongoSession.Get<TEntity>(query, builder, this.GetFields(fieldPredicate, revolt));
        }

        protected FieldsDocument GetFields(Expression<Func<TEntity, object>> fieldsPredicate = null, bool revolt = false)
        {
            FieldsDocument document = null;
            int num = revolt ? 0 : 1;
            if (fieldsPredicate != null)
            {
                document = new FieldsDocument();
                foreach (MemberInfo info in (fieldsPredicate.Body as NewExpression).Members)
                {
                    document.Add(new BsonElement(info.Name, num));
                }
            }
            return document;
        }

        public IQueryable<TEntity> GetList(Expression<Func<TEntity, bool>> filterPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null)
        {
            IQueryable<TEntity> arg = null;
            if (filterPredicate != null)
            {
                arg = this.QueryableCollection().Where<TEntity>(filterPredicate);
            }
            else
            {
                arg = this.QueryableCollection();
            }
            if (orderby != null)
            {
                arg = orderby(arg);
            }
            return arg;
        }

        public IQueryable<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null)
        {
            return this.GetList(filterPredicate, orderby).Select<TEntity, TResult>(fieldPredicate);
        }

        public IQueryable<TEntity> GetList(Expression<Func<TEntity, bool>> filterPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby, int pageIndex = 1, int pageSize = 10)
        {
            return this.GetList(filterPredicate, orderby).Skip<TEntity>(((pageIndex - 1) * pageSize)).Take<TEntity>(pageSize);
        }

        public IQueryable<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby, int pageIndex = 1, int pageSize = 10)
        {
            return this.GetList<TResult>(filterPredicate, fieldPredicate, orderby).Skip<TResult>(((pageIndex - 1) * pageSize)).Take<TResult>(pageSize);
        }

        public TEntity Insert(TEntity entity)
        {
            return this.Insert(entity, true);
        }

        public TEntity Insert(TEntity entity, bool isCreateIncID = true)
        {
            if (isCreateIncID)
            {
                long num = 0L;
                if (entity is IAutoIncr)
                {
                    num = this._mongoSession.CreateIncID<TEntity>();
                    if (entity.ID is int)
                    {
                        (entity as IEntity<int>).ID = (int)num;
                    }
                    else
                    {
                        (entity as IEntity<long>).ID = num;
                    }
                }
            }
            this._mongoSession.Insert<TEntity>(entity);
            return entity;
        }

        public virtual bool InsertBatch(IEnumerable<TEntity> entitys, bool isCreateIncID = true)
        {
            foreach (TEntity local in entitys)
            {
                if (isCreateIncID)
                {
                    long num = 0L;
                    if (local is IAutoIncr)
                    {
                        num = this._mongoSession.CreateIncID<TEntity>();
                        if (local.ID is int)
                        {
                            (local as IEntity<int>).ID = (int)num;
                        }
                        else
                        {
                            (local as IEntity<long>).ID = num;
                        }
                    }
                }
            }
            this._mongoSession.InsertBatch<TEntity>(entitys);
            return true;
        }

        public bool IsExists(IMongoQuery query)
        {
            return (this._mongoSession.Count<TEntity>(query) > 0L);
        }

        public bool IsExists(Expression<Func<TEntity, bool>> filterPredicate)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = Query<TEntity>.Where(filterPredicate);
            }
            return this.IsExists(query);
        }

        public IQueryable<TEntity> QueryableCollection()
        {
            return LinqExtensionMethods.AsQueryable<TEntity>(this._mongoSession.mongoDatabase.GetCollection<TEntity>(typeof(TEntity).Name));
        }

        [Obsolete("MongoDB数据库，在分布式系统中，使用全量更新有很大的风险，因此方法已屏蔽", true)]
        public TEntity Update(TEntity entityToUpdate)
        {
            throw new NotImplementedException();
        }

        public void Update(Expression<Func<TEntity, bool>> filterPredicate, Func<UpdateBuilder<TEntity>, IMongoUpdate> updatePredicate)
        {
            this.Update(filterPredicate, updatePredicate, 0);
        }

        public void Update(IMongoQuery query, IMongoUpdate update, UpdateFlags flag = 0)
        {
            this._mongoSession.Update<TEntity>(query, update, flag);
        }

        public void Update(Expression<Func<TEntity, bool>> filterPredicate, Func<UpdateBuilder<TEntity>, IMongoUpdate> updatePredicate, UpdateFlags flag)
        {
            IMongoQuery query = null;
            if (filterPredicate != null)
            {
                query = Query<TEntity>.Where(filterPredicate);
            }
            IMongoUpdate update = updatePredicate(new UpdateBuilder<TEntity>());
            this._mongoSession.Update<TEntity>(query, update, flag);
        }



    }
}
