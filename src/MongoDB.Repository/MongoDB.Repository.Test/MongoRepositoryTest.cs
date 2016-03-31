using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repository.IEntity;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MongoDB.Repository.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Insert()
        {
            UserRep userRep = new UserRep();

            //userRep.Insert(new User() { Name = "ggg" });
            //userRep.Insert(new User() { Name = "BBB" });
            //userRep.Insert(new User() { Name = "CCC" });

            //var list = userRep.GetList(x => x.Name == "ggg").ToList();
            //UserRep up = new UserRep();
            //list = up.GetList(x => x.Name == "ggg").ToList();
            //Assert.AreNotEqual(list.Count, 0);

            User user = null;
            List<User> list = new List<User>();
            var random = new Random();

            for (var i = 0; i < 10; i++)
            {
                user = new User();
                user.Age = random.Next(501, 800);
                list.Add(user);
            }

            list = list.OrderBy(x => x.Age).ToList();
            userRep.InsertBatch(list);
        }


        [TestMethod]
        public void UpdateOne()
        {
            UserRep up = new UserRep();
            up.UpdateOne(filterExp: x => x.Name == "ggg", updateExp: u => u.Set(x => x.Name, ""));
        }

        [TestMethod]
        public void Get()
        {
            UserRep userRep = RepositoryContainer.Resolve<UserRep>();
            User user = null;
            user = userRep.Get(1);
            //Assert.AreEqual(user.ID, 1);

            userRep = RepositoryContainer.Resolve<UserRep>();
            user = userRep.Get(x => x.Name == "aa");
        }

        [TestMethod]
        public void Aggregate()
        {
            UserRep userRep = RepositoryContainer.Resolve<UserRep>();
            var list = userRep.Aggregate(x => x.Age == 0, x => x.Name, x => new UserGroup() { Name = x.Key }, limit:9);
        }
    }

    public class UserGroup
    {
        public string Name { get; set; }
    }

}
