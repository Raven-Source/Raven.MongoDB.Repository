using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository.Test
{
    public class Repositorys
    {
        public static string dbName = "RepositoryTest";
        public static string connString = System.Configuration.ConfigurationManager.ConnectionStrings["RepositoryTest"].ConnectionString;
    }

    //public class UserRep : MongoRepository<User, long>
    //{

    //    public UserRep()
    //        : base(Repositorys.connString, Repositorys.dbName)
    //    {

    //    }
    //}

    public class UserRepAsync : MongoRepositoryAsync<User, long>
    {
        public UserRepAsync()
            : base(Repositorys.connString, Repositorys.dbName)
        {

        }
    }

    //public class MallCardRep : MongoRepository<MallCard, long>
    //{
    //    public MallCardRep()
    //        : base(Repositorys.connString, Repositorys.dbName)
    //    {

    //    }
    //}
}
