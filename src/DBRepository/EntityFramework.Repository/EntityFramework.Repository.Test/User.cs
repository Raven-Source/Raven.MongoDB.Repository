using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Repository.Test
{
    public class User : IEntity<int>
    {
        public int ID { get; set; }
    }

    public class UserRepository : EFRepository<User, int>
    {
        public UserRepository(DbContext context)
            : base(context)
        { }
    }
}
