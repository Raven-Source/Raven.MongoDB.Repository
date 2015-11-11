using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

using DB.Repository;
using Repository.IEntity;
using System.Linq.Expressions;

namespace EntityFramework.Repository
{
    /// <summary>
    /// EF仓储模式接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型泛型</typeparam>
    /// <typeparam name="TKey">Key类型泛型</typeparam>
    /// <remarks>add by liangyi on 2012/10/26</remarks>
    public interface IEFRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// 获取指定实体的DbSet
        /// </summary>
        /// <returns></returns>
        DbSet<TEntity> GetDbSet();
        /// <summary>
        /// 数据提交
        /// </summary>
        void Save();
        /// <summary>
        /// 根据sql获取数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IQueryable<TEntity> GetWithDbSetSql(string query, params object[] parameters);

        /// <summary>
        /// 根据id和其他条件获取实体
        /// </summary>
        /// <param name="filterPredicate">条件</param>
        /// <returns></returns>
        TEntity Get(Expression<Func<TEntity, bool>> filterPredicate);

        /// <summary>
        /// 获取可取相应的字段
        /// </summary>
        /// <param name="filterPredicate">条件</param>
        /// <param name="fieldPredicate">字段</param>
        /// <returns></returns>
        TEntity Get(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TEntity>> fieldPredicate);

        /// <summary>
        /// 获取可取相应的字段
        /// </summary>
        /// <param name="filterPredicate">条件</param>
        /// <param name="fieldPredicate">字段</param>
        /// <returns></returns>
        TResult Get<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate);


        /// <summary>
        /// 全部实体更新
        /// </summary>
        /// <param name="entityToUpdate">更新的实体</param>
        /// <param name="fieldPredicate">更新字段表达式</param>
        TEntity Update(TEntity entityToUpdate, System.Linq.Expressions.Expression<Func<TEntity, dynamic>> fieldPredicate);

        /// <summary>
        /// 全部实体更新
        /// </summary>
        /// <param name="entityToUpdate">更新的实体</param>
        /// <param name="properties">更新字段</param>
        TEntity Update(TEntity entityToUpdate, string[] properties);

        /// <summary>
        /// 实体更新
        /// </summary>
        /// <param name="filterPredicate">更新条件</param>
        /// <param name="updatePredicate">更新字段值</param>
        int Update(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TEntity>> updatePredicate);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="filterPredicate">条件</param>
        int Delete(Expression<Func<TEntity, bool>> filterPredicate);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        IQueryable<TEntity> GetList(Expression<Func<TEntity, bool>> filterPredicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null, string includeProperties = "");

        /// <summary>
        /// 获取数据(只获取部分字段)
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="fieldPredicate">取需求字段匿名类</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="includeProperties">包含virtual字段</param>
        /// <returns></returns>
        IQueryable<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null, string includeProperties = "");

        /// <summary>
        /// 获取数据（分页）
        /// </summary>
        /// <param name="filterPredicate">筛选条件</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数据数</param>
        /// <param name="includeProperties">包含virtual字段</param>
        /// <returns></returns>
        IQueryable<TEntity> GetList(Expression<Func<TEntity, bool>> filterPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby, int pageIndex = 1, int pageSize = 10,
             string includeProperties = "");

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
        IQueryable<TResult> GetList<TResult>(Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, TResult>> fieldPredicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby, int pageIndex = 1, int pageSize = 10,
             string includeProperties = "");
    }
}
