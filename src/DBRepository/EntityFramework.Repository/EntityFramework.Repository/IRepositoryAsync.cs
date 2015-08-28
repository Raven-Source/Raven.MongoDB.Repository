using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Repository
{
    /// <summary>
    /// 仓储模式接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型泛型</typeparam>
    /// <typeparam name="TKey">Key类型泛型</typeparam>
    /// <remarks>add by liangyi on 2012/10/26</remarks>
    public interface IRepositoryAsync<TEntity, TKey> where TEntity : IEntity<TKey>, new()
    {
        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(TKey id);
        
        /// <summary>
        /// 返回数量
        /// </summary>
        /// <param name="filterPredicate"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> filterPredicate = null);

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="filterPredicate">查询条件</param>
        /// <returns></returns>
        Task<bool> IsExistsAsync(Expression<Func<TEntity, bool>> filterPredicate);
    }
}
