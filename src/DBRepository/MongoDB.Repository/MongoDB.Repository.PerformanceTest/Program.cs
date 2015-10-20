using MongoDB.Driver;
using MongoDB.Repository.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository.PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            GetAsync().Wait();

            Console.ReadLine();
        }


        public static async Task GetAsync()
        {
            Console.WriteLine("GetAsync begin");

            UserRepAsync userRep = new UserRepAsync();
            User user = null;

            long linq, b;
            int speed = 10000;

            user = await userRep.Get(x => x.Name == "aa");
            user = await userRep.Get(Builders<User>.Filter.Eq<string>(nameof(User.Name), "aa"));
            Stopwatch sw = new Stopwatch();


            sw.Reset();
            sw.Start();
            for (var i = 0; i < speed; i++)
            {
                user = await userRep.Get(Builders<User>.Filter.Eq<string>(nameof(User.Name), "aa"));
            }
            sw.Stop();
            b = sw.ElapsedMilliseconds;

            sw.Reset();
            sw.Start();
            for (var i = 0; i < speed; i++)
            {
                user = await userRep.Get(x => x.Name == "aa");
            }
            sw.Stop();
            linq = sw.ElapsedMilliseconds;

            Console.WriteLine("lambda:{0},builders:{1}", linq, b);
            Console.WriteLine("GetAsync end");

            ;
        }
    }
}
