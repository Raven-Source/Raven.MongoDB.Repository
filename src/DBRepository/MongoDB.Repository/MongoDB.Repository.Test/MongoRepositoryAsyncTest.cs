using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;

namespace MongoDB.Repository.Test
{
    [TestClass]
    public class MongoRepositoryAsyncTest
    {
        [TestMethod]
        public async Task Insert()
        {
            UserRepAsync userRep = new UserRepAsync();

            User user = new User();
            user.Name = "aa";
            await userRep.Insert(user);

            user = new User();
            user.Name = "bb";
            await userRep.Insert(user);

            user = new User();
            user.Name = "cc";
            await userRep.Insert(user);
        }


        [TestMethod]
        public async Task Get()
        {
            UserRepAsync userRep = new UserRepAsync();
            User user = null;
            user = await userRep.Get(1);
            Assert.AreEqual(user.ID, 1);

            user = await userRep.Get(x => x.Name == "aa");
            Assert.AreNotEqual(user, null);
            try
            {
                user = await userRep.Get(x => x.Name == "aa", x => new { x.Name });
            }
            catch (Exception ex)
            {

            }
            Assert.AreNotEqual(user, null);
            user = await userRep.Get(x => x.Name == "aa", x => new { x.CreateTime });
            Assert.AreNotEqual(user, null);

            user = await userRep.Get(x => x.Name == "aa" && x.CreateTime > DateTime.Parse("2015/10/20"));
            Assert.AreNotEqual(user, null);

            user = await userRep.Get(Builders<User>.Filter.Eq("Name", "aa"), Builders<User>.Sort.Descending("_id"));
            Assert.AreNotEqual(user, null);
            user = await userRep.Get(filter: Builders<User>.Filter.Eq("Name", "aa"), projection: Builders<User>.Projection.Include(x => x.Name));
            Assert.AreNotEqual(user, null);

        }


        [TestMethod]
        public async Task GetList()
        {
            UserRepAsync userRep = new UserRepAsync();
            List<User> userList = null;
            userList = await userRep.GetList(null);

            userList = await userRep.GetList(x => x.ID > 3 && x.Name == "aa");

            userList = await userRep.GetList(filterExp: x => x.Name == "aa", includeFieldExp: x => new { x.CreateTime });
            userList = await userRep.GetList(filter:Builders<User>.Filter.Eq("Name", "aa"), sort: Builders<User>.Sort.Descending("_id"));
            userList = await userRep.GetList(filter: Builders<User>.Filter.Eq("Name", "aa"), projection: Builders<User>.Projection.Include(x => x.Name));
        }
    }
}
