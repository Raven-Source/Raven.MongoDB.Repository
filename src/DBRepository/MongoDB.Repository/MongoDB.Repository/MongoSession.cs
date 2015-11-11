using DB.Repository;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Repository
{
    /// <summary>
    /// MongoSessionAsync
    /// </summary>
    public class MongoSession
    {
        #region 私有方法

        /// <summary>
        /// MongoDB WriteConcern
        /// </summary>
        private WriteConcern _writeConcern;
        /// <summary>
        /// MongoClient
        /// </summary>
        private MongoClient _mongoClient;
        /// <summary>
        /// MongoDatabase
        /// </summary>
        public IMongoDatabase Database { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库链接字符串</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="writeConcern">WriteConcern选项</param>
        /// <param name="isSlaveOK"></param>
        /// <param name="readPreference"></param>
        public MongoSession(string connString, string dbName, WriteConcern writeConcern = null, bool isSlaveOK = false, ReadPreference readPreference = null)
        {
            this._writeConcern = writeConcern ?? WriteConcern.Unacknowledged;

            var databaseSettings = new MongoDatabaseSettings();
            databaseSettings.WriteConcern = this._writeConcern;
            databaseSettings.ReadPreference = readPreference ?? ReadPreference.SecondaryPreferred;

            _mongoClient = new MongoClient(connString);
            Database = _mongoClient.GetDatabase(dbName, databaseSettings);
        }

        #endregion


        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>() where T : class, new()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }

        #region 获取字段

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="fieldsExp"></param>
        /// <returns></returns>
        public ProjectionDefinition<T> IncludeFields<T>(Expression<Func<T, object>> fieldsExp) where T : class, new()
        {
            var builder = Builders<T>.Projection;

            if (fieldsExp != null)
            {
                List<ProjectionDefinition<T>> fieldDocument = new List<ProjectionDefinition<T>>();
                var body = (fieldsExp.Body as NewExpression);
                if (body == null || body.Members == null)
                {
                    throw new Exception("fieldsExp表达式格式错误， eg: x => new { x.Field1, x.Field2 }");
                }
                foreach (var m in body.Members)
                {
                    fieldDocument.Add(builder.Include(m.Name));
                }
                return builder.Combine(fieldDocument);
            }
            return null;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public SortDefinition<T> CreateSortDefinition<T>(Expression<Func<T, object>> sortExp, SortType sortType = SortType.Ascending)
        {
            SortDefinition<T> sort = null;
            if (sort != null)
            {
                if (sortType == SortType.Ascending)
                {
                    sort = Builders<T>.Sort.Ascending(sortExp);
                }
                else
                {
                    sort = Builders<T>.Sort.Descending(sortExp);
                }
            }
            return sort;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projection"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public FindOptions<T, T> CreateFindOptions<T>(ProjectionDefinition<T, T> projection = null
            , SortDefinition<T> sort = null
            , int limit = 0, int skip = 0)
        {
            var option = new FindOptions<T, T>();
            if (limit > 0)
            {
                option.Limit = limit;
            }
            if (skip > 0)
            {
                option.Skip = skip;
            }

            if (projection != null)
            {
                option.Projection = projection;
            }

            if (sort != null)
            {
                option.Sort = sort;
            }

            return option;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projection"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public FindOptions<T, T> CreateFindOptions<T>(ProjectionDefinition<T, T> projection = null
            , Expression<Func<T, object>> sortExp = null, SortType sortType = SortType.Ascending
            , int limit = 0, int skip = 0)
        {
            var option = new FindOptions<T, T>();
            if (limit > 0)
            {
                option.Limit = limit;
            }
            if (skip > 0)
            {
                option.Skip = skip;
            }

            if (projection != null)
            {
                option.Projection = projection;
            }

            SortDefinition<T> sort = CreateSortDefinition(sortExp, sortType);
            if (sort != null)
            {
                option.Sort = sort;
            }

            return option;
        }

    }
}
