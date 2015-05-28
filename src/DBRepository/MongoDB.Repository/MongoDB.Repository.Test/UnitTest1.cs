using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repository.IEntity;

namespace MongoDB.Repository.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            UserRep userRep = new UserRep();
        }
    }

    public class User : IAutoIncr
    {
        public long ID
        {
            get;
            set;
        }
    }

    public class UserRep : MongoRepository<User, long>
    {

    }

}
