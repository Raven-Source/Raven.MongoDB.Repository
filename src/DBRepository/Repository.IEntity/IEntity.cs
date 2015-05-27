using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repository.IEntity
{
    public interface IEntity<TKey>
    {
        /// <summary>
        /// 主键
        /// </summary>
        TKey ID { get; set; }
    }
}
