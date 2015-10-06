using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MongoDB.Repository.Test
{
    [TestClass]
    public class MongoRepositoryAsyncTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            UserRepAsync userRep = new UserRepAsync();

            //User user = new User();
            //user.Name = "aa";
            //userRep.Insert(user).Wait();

            //user = new User();
            //user.Name = "bb";
            //userRep.Insert(user).Wait();
            
            var user = userRep.GetAsync(1).Result;
            var name = nameof(User.Name);

        }
    }


    public class UserRepAsync : MongoRepositoryAsync<User, long>
    {
        private static string connString = System.Configuration.ConfigurationManager.ConnectionStrings["MongoDB_test"].ConnectionString;

        public UserRepAsync()
            : base(connString, "MongoDB_test")
        {

        }
    }
}
