using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

using EntityFramework.Repository;
using DB.Repository;

namespace EntityFramework.Repository.Test
{
    //testContext.Count
    //testContext.Delete() 3  问题
    //testContext.Get() 3
    //testContext.GetAll() 2  未测试 数据太少
    //testContext.GetList()   未测试
    //testContext.Insert()
    //testContext.Save()
    //testContext.Update()4   更新容易带上脏数据 同一上下文再次获取容易出问题。 建议更新的时候 部分更新 其他字段不赋值 
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void ContextIsOK()
        {
            var context = new CarContext();
            var all = context.Car.ToList();
            all.ForEach(r => context.Car.Remove(r));
            context.SaveChanges();
            context.Car.Add(new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            });
            context.SaveChanges();
            var cars = context.Car.ToList();
            Assert.AreEqual(cars.Count, 1);
            Assert.AreEqual(cars.FirstOrDefault().CarName, "Ford");
            Assert.AreEqual(cars.FirstOrDefault().CarPrice, 100000);
        }


        [TestMethod]
        public void GetCountDeleteIsOK()
        {
            var context = new CarContext();
            var testContext = new EFRepository<Car,int>(context);
            var all = context.Car.ToList();
            all.ForEach(r => testContext.Delete(r));
            testContext.Save();
            Assert.AreEqual(context.Car.ToList().Count, 0);
            context.Car.Add(new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            });
            testContext.Save();
            var cars = context.Car.ToList();
            Assert.AreEqual(testContext.Count(), 1);
            Assert.AreEqual(testContext.Get(r => r.CarPrice == 100000).CarName, "Ford");
        }

        [TestMethod]
        public void InsertIsOK()
        {
       
            var context = new CarContext();
            var testContext = new EFRepository<Car,int>(context);
            var all = context.Car.ToList();
            all.ForEach(r => testContext.Delete(r));
            testContext.Save();
            Assert.AreEqual(context.Car.ToList().Count, 0);
            testContext.Insert(new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            });
            testContext.Save();
            var cars = context.Car.ToList();
            Assert.AreEqual(testContext.Count(), 1);
            Assert.AreEqual(testContext.Get(r => r.CarPrice == 100000).CarName, "Ford");
        }


        [TestMethod]
        public void UpdateIsOK()
        {
            var context = new CarContext();
            var testContext = new EFRepository<Car,int>(context);
            var all = context.Car.ToList();
            all.ForEach(r => testContext.Delete(r));
            testContext.Save();
            var car = new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            };
            testContext.Insert(car);
            testContext.Save();

            car.CarName = "Ford1";
            car.CarPrice = 100000 - 1;
            testContext.Update(car);
            testContext.Save();

            var cars = context.Car.ToList();
            Assert.AreEqual(cars.Count, 1);
            Assert.AreEqual(cars.FirstOrDefault().CarName, "Ford1");
            Assert.AreEqual(cars.FirstOrDefault().CarPrice, 100000 - 1);
        }


        [TestMethod]
        public void Update1IsOK()
        {
            var context = new CarContext();
            var testContext = new EFRepository<Car,int>(context);
            var all = context.Car.ToList();
            all.ForEach(r => testContext.Delete(r));
            testContext.Save();
            var car = new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            };
            testContext.Insert(car);
            testContext.Save();
            car.CarPrice = 99999;
            car.CarName = "Ford1";
            testContext.Update(car, x => new { x.CarPrice });
            testContext.Save();
            var cars = testContext.GetDbSet().AsNoTracking().ToList();//OK


            var cars1 = testContext.GetDbSet().ToList();//error            
            Assert.AreEqual(cars.Count, 1);
            Assert.AreEqual(cars.FirstOrDefault().CarName, "Ford");
            Assert.AreEqual(cars.FirstOrDefault().CarPrice, 100000 - 1);
        }


        [TestMethod]
        public void DeleteIsOK()
        {
            var context = new CarContext();
            var testContext = new EFRepository<Car,int>(context);
            var all = context.Car.ToList();
            all.ForEach(r => testContext.Delete(r));
            testContext.Save();
            var car = new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            };
            testContext.Insert(car);
            testContext.Save();
            testContext.Delete(car.ID);
            testContext.Save();
            var cars = context.Car.ToList();
            Assert.AreEqual(cars.Count, 0);
        }

        [TestMethod]
        public void Delete1IsOK()
        {
            var context = new CarContext();
            var testContext = new EFRepository<Car,int>(context);
            var all = context.Car.ToList();
            all.ForEach(r => testContext.Delete(r));
            testContext.Save();
            var car = new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            };
            testContext.Insert(car);
            testContext.Save();
            testContext.Delete(r => r.ID == car.ID);
            testContext.Save();
            var cars = context.Car.ToList();
            Assert.AreEqual(cars.Count, 0);
        }


        [TestMethod]
        public void GetAllIsOK()
        {
            var context = new CarContext();
            var testContext = new EFRepository<Car,int>(context);
            context.Car.ToList().ForEach(r => testContext.Delete(r));
            testContext.Save();
            var car = new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            };
            testContext.Insert(car);
            testContext.Save();
            var cars = testContext.GetList(null, null, "").ToList();
            Assert.AreEqual(cars.Count, 1);
            Assert.AreEqual(cars.FirstOrDefault().CarName, "Ford");
            Assert.AreEqual(cars.FirstOrDefault().CarPrice, 100000);
        }


        [TestMethod]
        public void GetAll1IsOK()
        {
            var context = new CarContext();
            var testContext = new EFRepository<Car,int>(context);
            context.Car.ToList().ForEach(r => testContext.Delete(r));
            testContext.Save();
            var car = new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            };
            var car1 = new Car()
            {
                CarName = "Toyota",
                CarPrice = 100000
            };
            testContext.Insert(car);
            testContext.Insert(car1);
            testContext.Save();
            var cars = testContext.GetList(r => r.CarName == "Ford", null, "").ToList();
            Assert.AreEqual(cars.Count, 1);
            Assert.AreEqual(cars.FirstOrDefault().CarName, "Ford");
            Assert.AreEqual(cars.FirstOrDefault().CarPrice, 100000);
        }



        [TestMethod]
        public void GetAllOrderByIsOK()
        {
            var context = new CarContext();
            var testContext = new EFRepository<Car,int>(context);
            context.Car.ToList().ForEach(r => testContext.Delete(r));
            testContext.Save();
            var car = new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            };
            var car1 = new Car()
            {
                CarName = "Toyota",
                CarPrice = 99999
            };
            testContext.Insert(car);
            testContext.Insert(car1);
            testContext.Save();
            var cars = testContext.GetList(r => r.CarPrice == 100000, null, "").ToList();
            Assert.AreEqual(cars.Count, 1);
            Assert.AreEqual(cars.FirstOrDefault().CarName, "Ford");
            Assert.AreEqual(cars.FirstOrDefault().CarPrice, 100000);
        }


        [TestMethod]
        public void MyTestMethod()
        {
            var context = new CarContext();
            var testContext = new EFRepository<Car, int>(context);
            context.Car.ToList().ForEach(r => testContext.Delete(r));
            testContext.Save();
            var car = new Car()
            {
                CarName = "Ford",
                CarPrice = 100000
            };
            var car1 = new Car()
            {
                CarName = "Toyota",
                CarPrice = 100000
            };
            testContext.Insert(car);
            testContext.Insert(car1);
            testContext.Save();

            //var cars = testContext.GetList();
            //Assert.AreEqual(cars.Count, 1);
            //Assert.AreEqual(cars.FirstOrDefault().CarName, "Ford");
            //Assert.AreEqual(cars.FirstOrDefault().CarPrice, 100000);
        }
    }
}
