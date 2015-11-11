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
            await userRep.InsertAsync(user);

            user = new User();
            user.Name = "bb";
            await userRep.InsertAsync(user);

            user = new User();
            user.Name = "cc";
            await userRep.InsertAsync(user);
        }
        
        [TestMethod]
        public async Task InsertBatch()
        {
            UserRepAsync userRep = new UserRepAsync();

            List<User> userList = new List<User>();
            for (var i = 0; i < 5; i++)
            {
                User user = new User();
                user.Name = new Random(1).ToString();
                userList.Add(user);
            }

            await userRep.InsertBatchAsync(userList);

        }

        [TestMethod]
        public async Task UpdateOne()
        {
            UserRepAsync userRep = new UserRepAsync();
            await userRep.UpdateOneAsync(x => x.ID == 4, UserRepAsync.Update.Set(nameof(User.CreateTime), DateTime.Now));

            await userRep.UpdateOneAsync(x => x.Name == "bb", UserRepAsync.Update.Set(nameof(User.CreateTime), DateTime.Now));

            long id = await userRep.CreateIncIDAsync();
            var update = UserRepAsync.Update.Set(nameof(User.Name),"xyz");
            update = update.SetOnInsert(x => x.ID, id).SetOnInsert(x => x.CreateTime, DateTime.Now);
            await userRep.UpdateOneAsync(x => x.Name == "abc", update, true);
        }

        [TestMethod]
        public async Task UpdateOne_updateEntity()
        {
            User user = new User();
            UserRepAsync userRep = new UserRepAsync();
            
            user.Age += 1;
            user.CreateTime = DateTime.Now;
            user.Name = "zzz";

            await userRep.UpdateOneAsync(x => x.Name == "xx", user, true);

            //user = await userRep.Get(14);
            //user.Age += 1;
            //user.CreateTime = DateTime.Now;
            //user.Name = "gg";

            //await userRep.UpdateOne(user, false);
        }

        [TestMethod]
        public async Task FindOneAndUpdate()
        {
            User user;
            UserRepAsync userRep = new UserRepAsync();
            user = await userRep.GetAsync(15);
            user.Age += 1;
            user.CreateTime = DateTime.Now;
            //user.Desc = "ggggsdgsa";

            await userRep.FindOneAndUpdateAsync(filterExp:x => x.ID == user.ID, updateEntity: user, isUpsert: false);
        }


        [TestMethod]
        public async Task UpdateMany()
        {
            UserRepAsync userRep = new UserRepAsync();

            var update = UserRepAsync.Update.Set(nameof(User.CreateTime), DateTime.Now);
            
            await userRep.UpdateManyAsync(x => x.Name == "cc", update, true);
        }

        [TestMethod]
        public async Task Get()
        {
            UserRepAsync userRep = new UserRepAsync();
            User user = null;
            user = await userRep.GetAsync(1);
            Assert.AreEqual(user.ID, 1);

            user = await userRep.GetAsync(x => x.Name == "aa");
            Assert.AreNotEqual(user, null);
            user = await userRep.GetAsync(x => x.Name == "aa", x => new { x.Name });

            Assert.AreNotEqual(user, null);
            user = await userRep.GetAsync(x => x.Name == "aa", x => new { x.CreateTime });
            Assert.AreNotEqual(user, null);

            user = await userRep.GetAsync(x => x.Name == "aa" && x.CreateTime > DateTime.Parse("2015/10/20"));
            Assert.AreNotEqual(user, null);

            user = await userRep.GetAsync(Builders<User>.Filter.Eq("Name", "aa"), Builders<User>.Sort.Descending("_id"));
            Assert.AreNotEqual(user, null);
            user = await userRep.GetAsync(filter: Builders<User>.Filter.Eq("Name", "aa"), projection: Builders<User>.Projection.Include(x => x.Name));
            Assert.AreNotEqual(user, null);

        }


        [TestMethod]
        public async Task GetList()
        {
            UserRepAsync userRep = new UserRepAsync();
            List<User> userList = null;
            userList = await userRep.GetListAsync(null);

            userList = await userRep.GetListAsync(x => x.ID > 3 && x.Name == "aa");

            userList = await userRep.GetListAsync(filterExp: x => x.Name == "aa", includeFieldExp: x => new { x.CreateTime });
            userList = await userRep.GetListAsync(filter:Builders<User>.Filter.Eq("Name", "aa"), sort: Builders<User>.Sort.Descending("_id"));
            userList = await userRep.GetListAsync(filter: Builders<User>.Filter.Eq("Name", "aa"), projection: Builders<User>.Projection.Include(x => x.Name));
        }


    }
}
