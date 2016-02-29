
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MongoDB.Repository
{
    public class MongoSession
    {
        private const string DEFAULT_CONFIG_NODE = "MongoDB";
        public MongoDatabase mongoDatabase;

        public MongoSession(string dbName, string configNode = "MongoDB", WriteConcern writeConcern = null, MongoSequence sequence = null, bool isSlaveOK = false, ReadPreference readPreference = null)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[configNode].ConnectionString;
            this._writeConcern = writeConcern ?? WriteConcern.Unacknowledged;
            this._sequence = sequence ?? new MongoSequence();
            MongoServerSettings settings = MongoServerSettings.FromUrl(new MongoUrl(connectionString));
            this._mongoServer = new MongoServer(settings);
            if (isSlaveOK)
            {
                MongoDatabaseSettings settings2 = new MongoDatabaseSettings
                {
                    WriteConcern = this._writeConcern,
                    ReadPreference = readPreference ?? ReadPreference.SecondaryPreferred
                };
                this.mongoDatabase = this._mongoServer.GetDatabase(dbName, settings2);
            }
            else
            {
                this.mongoDatabase = this._mongoServer.GetDatabase(dbName, this._writeConcern);
            }
        }

        public WriteConcernResult AddToSet<T>(IMongoQuery query, string name, BsonValue val, UpdateFlags updateFlag = 0) where T : class, new()
        {
            return this.Update<T>(query, MongoDB.Driver.Builders.Update.AddToSet(name, val), updateFlag);
        }

        public long Count<T>(IMongoQuery query) where T : class, new()
        {
            return this.GetCollection<T>().Count(query);
        }

        public void Create2DIndex<T>(string indexKey) where T : class, new()
        {
            if (!string.IsNullOrEmpty(indexKey))
            {
                this.GetCollection<T>().EnsureIndex(IndexKeys.GeoSpatial(indexKey));
            }
        }

        public long CreateIncID<T>() where T : class, new()
        {
            long num = 1L;
            MongoCollection<BsonDocument> collection = this.mongoDatabase.GetCollection(this._sequence.Sequence);
            string name = typeof(T).Name;
            if (collection.Exists() && (collection.Find(MongoDB.Driver.Builders.Query.EQ(this._sequence.CollectionName, (BsonValue)name)).Count() > 0L))
            {
                FindAndModifyResult result = collection.FindAndModify(MongoDB.Driver.Builders.Query.EQ(this._sequence.CollectionName, (BsonValue)name), null, MongoDB.Driver.Builders.Update.Inc(this._sequence.IncrementID, 1), true);
                if (result.Ok && (result.ModifiedDocument != null))
                {
                    long.TryParse(result.ModifiedDocument.GetValue(this._sequence.IncrementID).ToString(), out num);
                }
                return num;
            }
            BsonDocument document = new BsonDocument();
            document.Add(this._sequence.CollectionName, (BsonValue)name);
            document.Add(this._sequence.IncrementID, (BsonValue)num);
            collection.Insert(document, this._writeConcern);
            return num;
        }

        public void CreateIndex<T>(string[] indexKeyArray) where T : class, new()
        {
            if (indexKeyArray.Length > 0)
            {
                this.GetCollection<T>().CreateIndex(indexKeyArray);
            }
        }

        public IEnumerable<BsonValue> Distinct<T>(string key, IMongoQuery query) where T : class, new()
        {
            return this.GetCollection<T>().Distinct(key, query);
        }

        public T FindAndModify<T>(IMongoQuery query, IMongoSortBy sortBy = null, IMongoUpdate update = null) where T : class, new()
        {
            T modifiedDocumentAs = default(T);
            FindAndModifyResult result = this.GetCollection<T>().FindAndModify(query, sortBy, update, true);
            if (result.Ok && (result.ModifiedDocument != null))
            {
                modifiedDocumentAs = result.GetModifiedDocumentAs<T>();
            }
            return modifiedDocumentAs;
        }

        public GeoNearResult<T> GeoNear<T>(IMongoQuery query, double x, double y, int limit = 1, IMongoGeoNearOptions geoNearOptions = null) where T : class, new()
        {
            return this.GetCollection<T>().GeoNear(query, x, y, limit, geoNearOptions);
        }

        public T Get<T>(IMongoQuery query, IMongoSortBy sortBy = null, IMongoFields fields = null) where T : class, new()
        {
            T local = default(T);
            return this.Top<T>(query, sortBy, 1, fields).FirstOrDefault<T>();
        }

        private MongoCollection<T> GetCollection<T>() where T : class, new()
        {
            return this.mongoDatabase.GetCollection<T>(typeof(T).Name);
        }

        public DateTime GetSysDateTime()
        {
            return this.mongoDatabase.Eval("new Date()", null).ToUniversalTime();
        }

        public WriteConcernResult Inc<T>(IMongoQuery query, string name, long val = 1L, UpdateFlags updateFlag = 0) where T : class, new()
        {
            return this.Update<T>(query, MongoDB.Driver.Builders.Update.Inc(name, val), updateFlag);
        }

        public WriteConcernResult Insert<T>(T item) where T : class, new()
        {
            return this.GetCollection<T>().Insert(item, this._writeConcern);
        }

        public IEnumerable<WriteConcernResult> InsertBatch<T>(IEnumerable<T> items) where T : class, new()
        {
            return this.GetCollection<T>().InsertBatch(items, this._writeConcern);
        }

        public MapReduceResult Mapreduce<T>(IMongoQuery query, BsonJavaScript map, BsonJavaScript reduce) where T : class, new()
        {
            MapReduceArgs args = new MapReduceArgs();
            args.Query = query;
            args.MapFunction = map;
            args.ReduceFunction = reduce;
            return this.GetCollection<T>().MapReduce(args);
        }

        public MapReduceResult Mapreduce<T>(IMongoQuery query, BsonJavaScript map, BsonJavaScript reduce, MapReduceArgs args) where T : class, new()
        {
            return this.GetCollection<T>().MapReduce(args);
        }

        public WriteConcernResult Pull<T>(IMongoQuery query, string name, BsonValue val, UpdateFlags updateFlag = 0) where T : class, new()
        {
            return this.Update<T>(query, MongoDB.Driver.Builders.Update.Pull(name, val), updateFlag);
        }

        public WriteConcernResult Push<T>(IMongoQuery query, string name, BsonValue val, UpdateFlags updateFlag = 0) where T : class, new()
        {
            return this.Update<T>(query, MongoDB.Driver.Builders.Update.Push(name, val), updateFlag);
        }

        public MongoCursor<T> Query<T>(IMongoQuery query, IMongoSortBy sortBy = null, int pageIndex = 0, int pageSize = 0, IMongoFields fields = null) where T : class, new()
        {
            MongoCursor<T> cursor = this.GetCollection<T>().Find(query);
            if (fields != null)
            {
                cursor = cursor.SetFields(fields);
            }
            if (sortBy != null)
            {
                cursor = cursor.SetSortOrder(sortBy);
            }
            if (pageSize != 0)
            {
                cursor.SetSkip(pageIndex * pageSize).SetLimit(pageSize);
            }
            return cursor;
        }

        public MongoCursor<T> Query<T>(int start, IMongoQuery query, IMongoSortBy sortBy = null, int pageIndex = 0, int pageSize = 0, IMongoFields fields = null) where T : class, new()
        {
            MongoCursor<T> cursor = this.GetCollection<T>().Find(query);
            if (fields != null)
            {
                cursor = cursor.SetFields(fields);
            }
            if (sortBy != null)
            {
                cursor = cursor.SetSortOrder(sortBy);
            }
            if (pageSize != 0)
            {
                if (pageIndex != 0)
                {
                    int num = ((pageIndex - 1) * pageSize) + start;
                    cursor.SetSkip(num).SetLimit(pageSize);
                    return cursor;
                }
                cursor.SetSkip(0).SetLimit(start);
            }
            return cursor;
        }

        public WriteConcernResult Remove<T>(IMongoQuery query, RemoveFlags removeFlag = 0) where T : class, new()
        {
            return this.GetCollection<T>().Remove(query, removeFlag, this._writeConcern);
        }

        public MongoCursor<T> Top<T>(IMongoQuery query, IMongoSortBy sortBy = null, int topCount = 10, IMongoFields fields = null) where T : class, new()
        {
            return this.Query<T>(query, sortBy, 0, topCount, fields);
        }

        public WriteConcernResult Update<T>(T item) where T : class, new()
        {
            return this.GetCollection<T>().Save<T>(item, this._writeConcern);
        }

        public WriteConcernResult Update<T>(IMongoQuery query, IMongoUpdate update, UpdateFlags updateFlag = 0) where T : class, new()
        {
            return this.GetCollection<T>().Update(query, update, updateFlag, this._writeConcern);
        }

        private MongoServer _mongoServer { get; set; }

        private MongoSequence _sequence { get; set; }

        private WriteConcern _writeConcern { get; set; }
    }
}

