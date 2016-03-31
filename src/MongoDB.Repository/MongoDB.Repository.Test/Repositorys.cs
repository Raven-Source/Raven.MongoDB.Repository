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

    public class UserRep : MongoRepository<User, long>
    {

        public UserRep()
            : base(Repositorys.connString, Repositorys.dbName)
        {

        }

        public UserRep(string connString, string dbName)
            : base(connString, dbName)
        {

        }
    }

    public class UserRepAsync : MongoRepositoryAsync<User, long>
    {
        //public UserRepAsync()
        //    : base(Repositorys.connString, Repositorys.dbName, null, null, new MongoSequence { IncrementID = "IncrementID", CollectionName = "CollectionName", SequenceName = "Sequence" })
        //{

        //}

        public UserRepAsync()
            : base(Repositorys.connString, Repositorys.dbName, null, null)
        {

        }

    }

    public class MallCardRepAsync : MongoRepositoryAsync<MallCard, string>
    {
        public MallCardRepAsync()
            : base(Repositorys.connString, Repositorys.dbName)
        {

        }
    }
}
