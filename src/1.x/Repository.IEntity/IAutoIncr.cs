using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IEntity
{
    public interface IEntity<TKey>
    {
        TKey ID { get; set; }
    }

    public interface IAutoIncr
    {
    }

    public interface IAutoIncr<T> : IAutoIncr, IEntity<T>
    {
    }
}
