using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Driver;
using MongoDB.Repository.Test;
using MongoDB.Repository;

namespace MongoDB.Repository.PerformanceTest
{
    class Program
    {
        static UserRepAsync userRepAsync = new UserRepAsync();
        static UserRep userRep = new UserRep();
        static MallCardRepAsync mallCardRep = new MallCardRepAsync();

        /// <summary>
        /// 
        /// </summary>
        //static int index;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int seed = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["seed"]);
            Console.WriteLine("seed:{0}", seed);

            //InsertAsync().Wait();
            //return;

            //Stopwatch sw2 = new Stopwatch();
            //sw2.Restart();

            //for (var i = 0; i < seed; i++)
            //{
            //    var a = new UserRepAsync();
            //}

            //Console.WriteLine("{0}ms", sw2.ElapsedMilliseconds);
            //sw2.Stop();

            //return;

            Stopwatch sw = new Stopwatch();
            Task[] tasks = new Task[seed];

            sw.Restart();
            for (var i = 0; i < seed; i++)
            {
                //tasks[i] = InsertAsync();
                tasks[i] = IncAsync(132);
            }

            Task.WaitAll(tasks);
            sw.Stop();

            Console.WriteLine("for:Insert:{0}ms", sw.ElapsedMilliseconds);
            Console.WriteLine("qps:{0}", seed / sw.Elapsed.TotalSeconds);

            Thread.SpinWait(10);
            sw.Restart();
            for (var i = 0; i < seed; i++)
            {
                tasks[i] = GetAsync(1);
            }

            Task.WaitAll(tasks);
            sw.Stop();

            Console.WriteLine("for:Get:{0}ms", sw.ElapsedMilliseconds);
            Console.WriteLine("qps:{0}", seed / sw.Elapsed.TotalSeconds);
            //sw.Restart();
            //for (var i = 0; i < speed; i++)
            //{
            //    tasks[i] = GetAsync(50);
            //}
            //Task.WaitAll(tasks);
            //sw.Stop();
            //Console.WriteLine("for:Get:" + sw.ElapsedMilliseconds);

            //sw.Restart();
            //for (var i = 0; i < speed; i++)
            //{
            //    Insert3();
            //}
            //sw.Stop();
            //Console.WriteLine("for:sync:" + sw.ElapsedMilliseconds);

            //sw.Restart();
            //Parallel.For(0, speed, x =>
            //{
            //    Insert2().Wait();
            //});
            //sw.Stop();
            //Console.WriteLine("parallel:async:" + sw.ElapsedMilliseconds);

            //sw.Restart();
            //Parallel.For(0, speed, x =>
            //{
            //    Insert3();
            //});
            //sw.Stop();
            //Console.WriteLine("parallel:sync:" + sw.ElapsedMilliseconds);

            //Expression<Func<User>> fieldsExp = () => new User { Age = 12, Name = "1111" };

            Console.WriteLine("over...");
            Console.ReadLine();
        }


        public static Task InsertAsync()
        {
            var mall = new MallCard();
            mall.MallID = 132;
            mall.UID = 3241;
            mall.CardTypeID = 24;

            return mallCardRep.InsertAsync(mall);
        }

        public static void Get(long id)
        {
            var user = userRep.Get(id);
            //await userRepAsync.InsertAsync(user).ConfigureAwait(false);
        }

        public static Task<User> GetAsync(long id)
        {
            return userRepAsync.GetAsync(id);
        }

        public static Task IncAsync(long mallId)
        {
            return mallCardRep.FindOneAndUpdateAsync(x => x.MallID == mallId, MallCardRepAsync.Update.Inc(t => t.A, 2).Inc(t => t.B, 4).Set(t =>  t.CardTypeID, 1610));
        }

        public static void Insert()
        {
            var user = new User();
            user.Name = "cc";

            userRep.Insert(user);
            //await userRepAsync.InsertAsync(user).ConfigureAwait(false);
        }

        public static async Task GetAsync()
        {
            Console.WriteLine("GetAsync begin");

            UserRepAsync userRepAsync = new UserRepAsync();
            User user = null;

            long lambda, builders, buildersFun;
            int speed = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["seed"]);

            user = await userRepAsync.GetAsync(x => x.Name == "aa");

            user = await userRepAsync.GetAsync(UserRepAsync.Filter.Eq<string>(nameof(User.Name), "aa"));
            //user = await userRep.Get(x => x.Eq<string>(nameof(User.Name), "aa"));
            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (var i = 0; i < speed; i++)
            {
                user = await userRepAsync.GetAsync(x => x.Name == "aa");
            }
            sw.Stop();
            lambda = sw.ElapsedMilliseconds;



            sw.Reset();
            sw.Start();
            for (var i = 0; i < speed; i++)
            {
                //user = await userRep.Get(x => x.Eq<string>(nameof(User.Name), "aa"));
                //user = await userRep.Get(x => x.Eq<string>(nameof(User.Name), "aa"));
            }
            sw.Stop();
            buildersFun = sw.ElapsedMilliseconds;



            sw.Reset();
            sw.Start();
            for (var i = 0; i < speed; i++)
            {
                user = await userRepAsync.GetAsync(Builders<User>.Filter.Eq<string>(nameof(User.Name), "aa"));
            }
            sw.Stop();
            builders = sw.ElapsedMilliseconds;

            Console.WriteLine("lambda:{0},builders:{1},buildersFun:{2}", lambda, builders, buildersFun);
            Console.WriteLine("GetAsync end");

            ;
        }
    }
}
