using Raven.MongoDB.Repository.Test.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.MongoDB.Repository.Test.Repositroy
{
    /// <summary>
    /// 同步仓储
    /// </summary>
    public class User2Repository : MongoRepository<User2, string>
    {
        private User2Repository():
            base(Repositorys.connString, Repositorys.dbName)
        {

        }

        private static User2Repository CreateInstance()
        {
            return new User2Repository();
        }
    }

    /// <summary>
    /// 异步仓储
    /// </summary>
    public class User2RepositoryAsync : MongoRepositoryAsync<User2, string>
    {
        private User2RepositoryAsync():
            base(Repositorys.connString, Repositorys.dbName)
        {

        }

        //private User2RepositoryAsync CreateInstance()
        //{
        //    return new User2RepositoryAsync();
        //}
    }
}
