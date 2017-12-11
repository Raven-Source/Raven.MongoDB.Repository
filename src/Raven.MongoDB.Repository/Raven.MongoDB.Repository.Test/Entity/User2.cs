using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Raven.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.MongoDB.Repository.Test.Entity
{
    /// <summary>
    /// 
    /// </summary>
    [BsonIgnoreExtraElements]
    public class User2 : IEntity<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }

        public string Name
        {
            get;
            set;
        }

        public int Age
        { get; set; }



        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime
        {
            get;
            set;
        }

        public string Remark { get; set; }

        public User2()
        {
            CreateTime = DateTime.Now;
        }
    }
}
