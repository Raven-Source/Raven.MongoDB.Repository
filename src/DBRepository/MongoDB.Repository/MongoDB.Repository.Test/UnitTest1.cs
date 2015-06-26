using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repository.IEntity;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Repository.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            ;
        }
    }

    public class User : IAutoIncr<long>
    {
        [BsonId]
        public long ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
    }

    public class UserRep : MongoRepository<User, long>
    {
        public UserRep()
            : base("MallcooTest", "MongoDB_test") 
        {

        }
    }

}
