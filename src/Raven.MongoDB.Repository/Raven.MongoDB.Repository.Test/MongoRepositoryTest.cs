using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Data.Entity;
using MongoDB.Bson.Serialization.Attributes;

namespace Raven.MongoDB.Repository.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Insert()
        {
            UserRep userRep = new UserRep();
            
            userRep.Insert(new User() { Name = "ggg" });
            userRep.Insert(new User() { Name = "BBB" });
            userRep.Insert(new User() { Name = "CCC" });

            var list = userRep.GetList(x => x.Name == "ggg").ToList();
            UserRep up = new UserRep();
            list = up.GetList(x => x.Name == "ggg").ToList();
            Assert.AreNotEqual(list.Count, 0);
        }


        [TestMethod]
        public void TestMethod1()
        {

        }
    }

}
