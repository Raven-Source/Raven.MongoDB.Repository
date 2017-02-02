using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository.Sequence
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISequence
    {
        /// <summary>
        /// 存储数据的序列
        /// </summary>
        string SequenceName { get; set; }
        /// <summary>
        /// 对应的Collection名称,默认为_id
        /// </summary>
        string CollectionName { get; set; }
        /// <summary>
        /// 对应Collection的自增长ID
        /// </summary>
        string IncrementID { get; set; }
    }
}
