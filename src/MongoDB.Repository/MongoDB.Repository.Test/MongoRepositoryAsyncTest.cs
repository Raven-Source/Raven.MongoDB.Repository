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


            MallCardRepAsync mallcardRep = new MallCardRepAsync();
            //await mallcardRep.InsertAsync(new MallCard() { UID = 1 });
            //await mallcardRep.InsertAsync(new MallCard() { UID = 2 });
            //await mallcardRep.InsertAsync(new MallCard() { UID = 3 });
            //await mallcardRep.InsertAsync(new MallCard() { UID = 4 });

            List<InsertOneModel<MallCard>> list = new List<InsertOneModel<MallCard>>();
            //list.Add(new InsertOneModel<MallCard>(new MallCard() { UID = 5 }));
            list.Add(new InsertOneModel<MallCard>(new MallCard() { UID = 6 }));
            list.Add(new InsertOneModel<MallCard>(new MallCard() { UID = 10 }));
            list.Add(new InsertOneModel<MallCard>(new MallCard() { UID = 1 }));
            list.Add(new InsertOneModel<MallCard>(new MallCard() { UID = 2 }));

            //var res = await mallcardRep.BulkInsertAsync(list);


        }

        [TestMethod]
        public async Task InsertBatch()
        {
            UserRepAsync userRep = new UserRepAsync();

            var r = new Random();
            List<User> userList = new List<User>();
            for (var i = 0; i < 5; i++)
            {
                User user = new User();
                user.Age = i;
                user.Name = r.Next().ToString();
                userList.Add(user);
            }

            await userRep.InsertBatchAsync(userList);

        }

        [TestMethod]
        public async Task UpdateOne()
        {
            UserRepAsync userRep = new UserRepAsync();
            await userRep.UpdateOneAsync(x => x.ID == 4, UserRepAsync.Update.Set(nameof(User.CreateTime), DateTime.Now));

            //UserRepAsync.Update.Set("_id", 1);
            //UserRepAsync.Update.Set(x => x.ID, 1);


            await userRep.UpdateOneAsync(x => x.Name == "bb", UserRepAsync.Update.Set(nameof(User.CreateTime), DateTime.Now));

            long id = await userRep.CreateIncIDAsync();
            var update = UserRepAsync.Update.Set(nameof(User.Name), "xyz");
            update = update.SetOnInsert(x => x.ID, id).SetOnInsert(x => x.CreateTime, DateTime.Now);
            await userRep.UpdateOneAsync(x => x.Name == "abc", update, true);

            //MongoCollectionSettings setting = new MongoCollectionSettings() { ReadPreference = ReadPreference.PrimaryPreferred, WriteConcern = WriteConcern.Acknowledged };
            var res = await userRep.UpdateOneAsync(x => x.Name == "xyz", update, true, WriteConcern.Acknowledged);
            Assert.AreEqual(res.IsAcknowledged, true);

            MallCardRepAsync mallcardRep = new MallCardRepAsync();
            MallCard mc = new MallCard();
            mc.UID = 2;
            mc.MallID = 455;
            await mallcardRep.UpdateOneAsync(filterExp: x => x.UID == 1, updateEntity: mc);

            await mallcardRep.UpdateOneAsync(filterExp: x => x.UID == 1, updateExp: u => u.Set(x => x.UID, 1).Set(x => x.MallID, 1245263));

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

            user = await userRep.FindOneAndUpdateAsync(filterExp: x => x.ID == user.ID, updateEntity: user, isUpsert: false);
        }


        [TestMethod]
        public async Task UpdateMany()
        {
            UserRepAsync userRep = new UserRepAsync();

            var update = UserRepAsync.Update.Set(nameof(User.CreateTime), DateTime.Now);
            System.Diagnostics.Trace.WriteLine("update:" + userRep.Render(update));

            await userRep.UpdateManyAsync(x => x.Name == "cc", update, true);
        }

        [TestMethod]
        public async Task Get()
        {
            UserRepAsync userRep = new UserRepAsync();
            User user = null;
            ProjectionDefinition<User, User> projection;
            FilterDefinition<User> filter;

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
            Builders<User>.Filter.Eq("Name", "aa");

            filter = UserRepAsync.Filter.Eq(x => x.Name, "aa") & UserRepAsync.Filter.Eq(x => x.ID, 123);
            System.Diagnostics.Trace.WriteLine("filter:" + userRep.Render(filter));

            UserRepAsync.Sort.Descending("_id");

            user = await userRep.GetAsync(Builders<User>.Filter.Eq("Name", "aa"), null, Builders<User>.Sort.Descending("_id"));
            Assert.AreNotEqual(user, null);

            filter = Builders<User>.Filter.Eq("Name", "aa");
            projection = Builders<User>.Projection.Include(x => x.Name);
            user = await userRep.GetAsync(filter: filter, projection: projection);

            Assert.AreNotEqual(user, null);

        }

        [TestMethod]
        public async Task Get2()
        {
            UserRepAsync userRep = new UserRepAsync();

            User user = null;
            //user = await userRep.GetAsync(filterExp: x => x.Name == "aa", sortExp: x => x.ID, sortType: SortType.Descending, hint: "Name_1");
            //Assert.AreEqual(user.Name, "aa");

            user = await userRep.GetAsync(filterExp: x => x.Name == "aa", sortExp: x => x.ID, sortType: SortType.Descending);
            Assert.AreEqual(user.Name, "aa");

        }


        [TestMethod]
        public async Task GetList()
        {
            UserRepAsync userRep = new UserRepAsync();
            List<User> userList = null;
            userList = await userRep.GetListAsync(null);
            userList = await userRep.GetListAsync(UserRepAsync.Filter.Empty);

            userList = await userRep.GetListAsync(x => x.ID > 3 && x.Name == "aa");
            userList = await userRep.GetListAsync(x => x.ID > 3 && x.Name == "aa", null, s => s.ID, SortType.Ascending);

            userList = await userRep.GetListAsync(filterExp: x => x.Name == "aa", includeFieldExp: x => new { x.CreateTime });

            userList = await userRep.GetListAsync(filter: Builders<User>.Filter.Eq("Name", "aa"), sort: Builders<User>.Sort.Descending("_id"));
            userList = await userRep.GetListAsync(filter: Builders<User>.Filter.Eq("Name", "aa"), projection: Builders<User>.Projection.Include(x => x.Name));
        }

        [TestMethod]
        public async Task Distinct()
        {
            UserRepAsync userRep = new UserRepAsync();
            var list = await userRep.DistinctAsync(x => x.Name, null);

        }


        [TestMethod]
        public async Task Count_Exists()
        {
            UserRepAsync userRep = new UserRepAsync();
            var res = await userRep.ExistsAsync(x => x.Name == "aa");
            Assert.AreEqual(res, true);

            res = await userRep.ExistsAsync(x => x.Name == "------");
            Assert.AreEqual(res, false);

            var res2 = await userRep.CountAsync(x => x.Name == "aa");
            Assert.AreNotEqual(res2, 0);

            res2 = await userRep.CountAsync(x => x.Name == "------");
            Assert.AreEqual(res2, 0);
        }
    }
}
