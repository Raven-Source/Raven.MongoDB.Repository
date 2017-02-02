using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository.Funcs
{
    /// <summary>
    /// 
    /// </summary>
    internal static class FunctionScriptManager
    {
        //private static ConcurrentDictionary<string, Func<IMongoDatabase>> databaseFuncs;
        private static HashSet<string> _hs = new HashSet<string>();
        internal const string SystemJsCollectionName = "system.js";
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="funcsInit"></param>
        public static void Init(string dbName, IMongoDatabase funcsInit, MongoSequence sequence)
        {
            if (!_hs.Contains(dbName))
            {
                lock (_hs)
                {
                    if (!_hs.Contains(dbName))
                    {
                        var systemJsCollection = funcsInit.GetCollection<BsonDocument>(SystemJsCollectionName, new MongoCollectionSettings() { WriteConcern = WriteConcern.Acknowledged });

                        InitGetNextSequence(systemJsCollection, sequence);

                        _hs.Add(dbName);

                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="sequence"></param>
        public static void InitGetNextSequence(IMongoCollection<BsonDocument> collection, MongoSequence sequence)
        {
            string key = sequence.GetMongoDBFunctionName()
                , value = FunctionScript.getNextSequence
                .Replace("$[SequenceName]$", sequence.SequenceName)
                .Replace("$[CollectionName]$", sequence.CollectionName)
                .Replace("$[IncrementID]$", sequence.IncrementID);

            var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
            var update = Builders<BsonDocument>.Update.Set("value", new MongoDB.Bson.BsonJavaScript(value));
            var options = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>();
            options.IsUpsert = true;
            options.ReturnDocument = ReturnDocument.After;

            var res = collection.Find(filter).FirstOrDefault();
            if (res == null)
            {
                collection.FindOneAndUpdate(filter, update, options);
            }

        }

    }
}
