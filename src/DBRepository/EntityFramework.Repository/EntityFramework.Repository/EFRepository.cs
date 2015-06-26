using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using EntityFramework.Extensions;
using Repository.IEntity;

namespace EntityFramework.Repository
{
    /// <summary>
    /// EF仓储层基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <remarks>add by liangyi on 2012/10/26</remarks>
    public class EFRepository<TEntity, TKey> : IEFRepository<TEntity, TKey>
        where TEntity : class,IEntity<TKey>, new()
    {
        protected DbContext context;
        protected DbSet<TEntity> dbSet;
        protected DbSet db;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cmsContext"></param>
        public EFRepository(DbContext context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        /// <summary>
        /// 获取指定实体的DbSet
        /// </summary>
        /// <returns></returns>
        public System.Data.Entity.DbSet<TEntity> GetDbSet()
        {
            return dbSet;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            context.SaveChanges();
        }

        /// <summary>
        /// 根据sql获取数据
        /// </summary>
        /// <param name="query">sql语句</param>
        /// <param name="parameters">sql参数</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetWithDbSetSql(string query, params object[] parameters)
        {
            return dbSet.SqlQuery(query, parameters).AsNoTracking().AsQueryable();
        }

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Get(TKey id)
        {
            return dbSet.Find(id);
        }

        ///// <summary>
        ///// 根据id获取实体
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="fieldPredicate">字段</param>
        ///// <returns></returns>
        //public TEntity Get(TKey id, Expression<Func<TEntity, TEntity>> fieldPredicate)
        //{
        //    //x => x.ID.Equals(id) error
        //    return dbSet.Where(x => x.ID.Equals(id)).AsNoTracking().Select(fieldPredicate).FirstOrDefault();
        //}

        /// <summary>
        /// 根据其他条件获取实体
        /// </summary>
        /// <param name="filterPredicate">查询条件</param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> filterPredicate)
        {
            return dbSet.Where(filterPredicate).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// 获取可取相应的字段
        /// </summary>
        /// <param name="filterPredicate">条件</param>
        /// <param name="fieldPredicate">字段</param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TEntity>> fieldPredicate)
        {
            return dbSet.Where(filterPredicate).AsNoTracking().Select(fieldPredicate).FirstOrDefault();
        }

        /// <summary>
        /// 获取可取相应的字段
        /// </summary>
        /// <param name="filterPredicate">条件</param>
        /// <param name="fieldPredicate">字段</param>
        /// <returns></returns>
        public TResult Get<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate)
        {
            return dbSet.Where(filterPredicate).AsNoTracking().Select(fieldPredicate).FirstOrDefault();
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="entity"></param>
        public TEntity Insert(TEntity entity)
        {
            return dbSet.Add(entity);
        }

        /// <summary>
        /// 全部实体更新
        /// </summary>
        /// <param name="entityToUpdate">更新的实体</param>
        public TEntity Update(TEntity entityToUpdate)
        {
            var entity = dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
            return entity;
        }

        /// <summary>
        /// 全部实体更新
        /// </summary>
        /// <param name="entityToUpdate">更新的实体</param>
        /// <param name="updatePredicate">更新字段表达式</param>
        public virtual TEntity Update(TEntity entityToUpdate, System.Linq.Expressions.Expression<Func<TEntity, dynamic>> updatePredicate)
        {
            var entity = dbSet.Attach(entityToUpdate);
            var stateEntry = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager.GetObjectStateEntry(entityToUpdate);
            var members = (updatePredicate.Body as System.Linq.Expressions.NewExpression).Members;
            foreach (var m in members)
            {
                stateEntry.SetModifiedProperty(m.Name);
            }
            return entity;
        }

        /// <summary>
        /// 全部实体更新
        /// </summary>
        /// <param name="entityToUpdate">更新的实体</param>
        /// <param name="properties">更新字段</param>
        public virtual TEntity Update(TEntity entityToUpdate, string[] properties)
        {
            var entity = dbSet.Attach(entityToUpdate);
            var stateEntry = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager.GetObjectStateEntry(entityToUpdate);
            foreach (var p in properties)
            {
                stateEntry.SetModifiedProperty(p);
            }
            return entity;
        }

        /// <summary>
        /// 实体更新
        /// </summary>
        /// <param name="filterPredicate">更新条件</param>
        /// <param name="updatePredicate">更新字段值</param>
        public virtual int Update(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TEntity>> updatePredicate)
        {
            return dbSet.Where(filterPredicate).Update(updatePredicate);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        public void Delete(TKey id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            if (entityToDelete != null)
            {
                Delete(entityToDelete);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entityToDelete"></param>
        public void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="filterPredicate"></param>
        public virtual int Delete(Expression<Func<TEntity, bool>> filterPredicate)
        {
            return dbSet.Where(filterPredicate).Delete();
        }

        /// <summary>
        /// 返回数量
        /// </summary>
        /// <param name="filterPredicate">查询条件</param>
        /// <returns></returns>
        public long Count(Expression<Func<TEntity, bool>> filterPredicate = null)
        {
            if (filterPredicate == null)
            {
                return dbSet.Count();
            }
            else
            {
                return dbSet.Count(filterPredicate);
            }
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="filterPredicate">查询条件</param>
        /// <returns></returns>
        public bool IsExists(Expression<Func<TEntity, bool>> filterPredicate)
        {
            if (dbSet.Count(filterPredicate) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public IQueryable<TEntity> GetList(Expression<Func<TEntity, bool>> filterPredicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;
            if (filterPredicate != null)
            {
                query = query.Where(filterPredicate);
            }
            foreach (var includePropertie in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includePropertie);
            }
            if (orderby != null)
            {
                query = orderby(query).AsNoTracking();
            }
            else
            {
                query = query.AsNoTracking();
            }

            return query.AsQueryable();
        }

        /// <summary>
        /// 获取数据(只获取部分字段)
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="fieldPredicate">取需求字段匿名类</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="includeProperties">包含virtual字段</param>
        /// <returns></returns>
        public IQueryable<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = this.GetList(filterPredicate, orderby, includeProperties);
            return query.Select(fieldPredicate).AsQueryable();
        }


        /// <summary>
        /// 获取数据（分页）
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数据数</param>
        /// <param name="includeProperties">包含virtual字段</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetList(Expression<Func<TEntity, bool>> filterPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby, int pageIndex = 1, int pageSize = 10,
             string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;
            if (filterPredicate != null)
            {
                query = query.Where(filterPredicate);
            }
            query = orderby(query);

            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            foreach (var includePropertie in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includePropertie);
            }

            return query.AsNoTracking().AsQueryable();
        }

        /// <summary>
        /// 获取数据（分页）
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="fieldPredicate">取需求字段匿名类</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数据数</param>
        /// <param name="includeProperties">包含virtual字段</param>
        /// <returns></returns>
        public IQueryable<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby, int pageIndex = 1, int pageSize = 10,
             string includeProperties = "")
        {
            IQueryable<TEntity> query = this.GetList(filterPredicate, orderby, pageIndex, pageSize, includeProperties);
            return query.Select(fieldPredicate).AsQueryable();
        }
    }
}
