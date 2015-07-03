using System;
using System.Linq;
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
            UserRep up = new UserRep();
            var list = up.GetList(x => x.Name == "xxx").ToList();
            Assert.AreNotEqual(list.Count, 0);
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
            : base("Test", "MongoDB_test") 
        {

        }
    }

}
