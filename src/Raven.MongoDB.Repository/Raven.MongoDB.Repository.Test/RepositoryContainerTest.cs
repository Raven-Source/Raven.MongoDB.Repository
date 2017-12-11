using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.MongoDB.Repository.Test.Entity;
using Raven.MongoDB.Repository.Test.Repositroy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Raven.MongoDB.Repository.Test
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class RepositoryContainerTest
    {
        [TestInitialize]
        public void init()
        {

        }

        /// <summary>
        /// 注册测试，测试类别如下：
        /// 1：传入新创建的对象
        /// 2：直接传入指定对象类型
        /// 3：传入Func函数
        /// </summary>
        [TestMethod]
        public void Register()
        {
            UserRep uRep = new UserRep();
            RepositoryContainer.Register(uRep);
            UserRep uRep2 = RepositoryContainer.Resolve<UserRep>();
            Assert.AreEqual(uRep, uRep2);

            RepositoryContainer.Register<UserRepAsync>(() => { return new UserRepAsync(); });
            var uRepAsync = RepositoryContainer.Resolve<UserRepAsync>();
            Assert.IsNotNull(uRepAsync);
        }

        /// <summary>
        /// 全部注册和替换测试，测试类别如下：
        /// 1：一次注册该程序集下所有符合要求的类的实例
        /// 2：替换掉所有原先注册的对象
        /// </summary>
        [TestMethod]
        public void RegisterAll()
        {
            #region 测试一次注册全部
            string assemblyName = GetType().Assembly.GetName().ToString();
            RepositoryContainer.RegisterAll(assemblyName);

            var uRep = RepositoryContainer.Resolve<UserRep>();
            var uRepAsync = RepositoryContainer.Resolve<UserRepAsync>();
            var uRep2 = RepositoryContainer.Resolve<User2Repository>();
            var uRepAsync2 = RepositoryContainer.Resolve<User2RepositoryAsync>();
            Assert.IsNotNull(uRep);
            Assert.IsNotNull(uRepAsync);
            Assert.IsNotNull(uRep2);
            Assert.IsNotNull(uRepAsync2);
            #endregion

            #region 测试replace
            //新注册，应该要跟原对象不同
            RepositoryContainer.RegisterAll(assemblyName);

            Assert.AreNotEqual(uRep, RepositoryContainer.Resolve<UserRep>());
            Assert.AreNotEqual(uRepAsync, RepositoryContainer.Resolve<UserRepAsync>());
            Assert.AreNotEqual(uRep2, RepositoryContainer.Resolve<User2Repository>());
            Assert.AreNotEqual(uRepAsync2, RepositoryContainer.Resolve<User2RepositoryAsync>());
            #endregion
        }

        /// <summary>
        /// 实际访问数据库测试，测试类别如下：
        /// 1：创建 有无参公共构造方法的、同步和异步的仓储，新增并查询数据库测试
        /// 2：创建 私有无参构造方法，但是有CreateInstance方法的、同步和异步的仓储，新增并查询数据库测试
        /// </summary>
        [TestMethod]
        public async Task CallMongoDB()
        {
            #region 无参公共构造方法
            RepositoryContainer.Register<UserRep>();
            RepositoryContainer.Register<UserRepAsync>();

            var uRep = RepositoryContainer.Resolve<UserRep>();
            var uRepAsync = RepositoryContainer.Resolve<UserRepAsync>();

            #region 同步仓储 新增和读取数据测试
            User entity1 = new User()
            {
                Age = 18,
                Name = "uRep测试",
                Stamp = "哈哈"
            };
            uRep.Insert(entity1);
            Assert.IsTrue(entity1.ID > 0);
            var e1 = uRep.Get(x => x.ID == entity1.ID);
            Assert.IsNotNull(e1);
            Assert.IsTrue(e1.Name == "uRep测试");
            #endregion

            #region 异步仓储 新增和读取数据测试
            User entity2 = new User()
            {
                Age = 19,
                Name = "uRepAsync测试",
                Stamp = "哈哈2"
            };
            await uRepAsync.InsertAsync(entity2);
            Assert.IsTrue(entity2.ID > 0);
            var e2 = await uRepAsync.GetAsync(x => x.ID == entity2.ID);
            Assert.IsNotNull(e2);
            Assert.IsTrue(e2.Name == "uRepAsync测试");
            #endregion
            #endregion

            #region 无参私有构造方法
            RepositoryContainer.Register(typeof(User2Repository));
            RepositoryContainer.Register(typeof(User2RepositoryAsync));

            var uRep2 = RepositoryContainer.Resolve<User2Repository>();
            var uRepAsync2 = RepositoryContainer.Resolve<User2RepositoryAsync>();

            #region 同步仓储 新增和读取数据测试
            User2 entity3 = new User2()
            {
                Age = 20,
                Name = "User2Repository测试",
                Remark = "哈哈"
            };
            uRep2.Insert(entity3);
            Assert.IsNotNull(entity3.ID);
            var e3 = uRep2.Get(x => x.ID == entity3.ID);
            Assert.IsNotNull(e3);
            Assert.IsTrue(e3.Name == "User2Repository测试");
            #endregion

            #region 异步仓储 新增和读取数据测试
            User2 entity4 = new User2()
            {
                Age = 21,
                Name = "User2RepositoryAsync测试",
                Remark = "哈哈"
            };
            await uRepAsync2.InsertAsync(entity4);
            Assert.IsNotNull(entity4.ID);
            var e4 = await uRepAsync2.GetAsync(x => x.ID == entity4.ID);
            Assert.IsNotNull(e4);
            Assert.IsTrue(e4.Name == "User2RepositoryAsync测试");
            #endregion
            #endregion
        }
    }
}
