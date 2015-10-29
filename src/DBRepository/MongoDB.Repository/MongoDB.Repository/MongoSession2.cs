using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
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
    /// 
    /// </summary>
    public class MongoSession2
    {
        #region 私有方法

        /// <summary>
        /// Mongo自增长ID数据序列
        /// </summary>
        private MongoSequence _sequence;
        /// <summary>
        /// MongoDB WriteConcern
        /// </summary>
        private WriteConcern _writeConcern;
        /// <summary>
        /// MongoServer
        /// </summary>
        private MongoServer _mongoServer;
        /// <summary>
        /// MongoDatabase
        /// </summary>
        public MongoDatabase Database { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库链接字符串</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="writeConcern">WriteConcern选项</param>
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        /// <param name="readPreference"></param>
        public MongoSession2(string connString, string dbName, WriteConcern writeConcern = null, MongoSequence sequence = null,ReadPreference readPreference = null)
        {
            this._writeConcern = writeConcern ?? WriteConcern.Unacknowledged;
            this._sequence = sequence ?? new MongoSequence();

            var serverSettings = MongoServerSettings.FromUrl(new MongoUrl(connString));
            this._mongoServer = new MongoServer(serverSettings);

            //this._mongoServer.Settings.WriteConcern = WriteConcern.Unacknowledged;

            var databaseSettings = new MongoDatabaseSettings();
            databaseSettings.WriteConcern = this._writeConcern;
            databaseSettings.ReadPreference = readPreference ?? ReadPreference.SecondaryPreferred;
            Database = this._mongoServer.GetDatabase(dbName, databaseSettings);
        }

        #endregion


        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public MongoCollection<T> GetCollection<T>() where T : class, new()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }
        
        /// <summary>
        /// 创建自增长ID
        /// <remarks>默认自增ID存放 [Sequence] 集合</remarks>
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public long CreateIncID<T>() where T : class, new()
        {
            long id = 1;
            var collection = Database.GetCollection(this._sequence.SequenceName);
            var typeName = typeof(T).Name;

            var args = new FindAndModifyArgs();
            args.Query = MongoDB.Driver.Builders.Query.EQ(this._sequence.CollectionName, typeName);
            args.Update = MongoDB.Driver.Builders.Update.Inc(this._sequence.IncrementID, 1);
            args.VersionReturned = FindAndModifyDocumentVersion.Modified;
            args.Upsert = true;

            var result = collection.FindAndModify(args);
            if (result.Ok && result.ModifiedDocument != null)
                id = result.ModifiedDocument[this._sequence.IncrementID].AsInt64;

            return id;
        }


        #region 获取字段

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="fieldsExp"></param>
        /// <param name="revolt">true|不加载字段,fale|加载字段</param>
        /// <returns></returns>
        protected FieldsDocument IncludeFields<T>(Expression<Func<T, object>> fieldsExp = null)
            where T : class, new()
        {
            FieldsDocument fieldDocument = null;
            //int val = revolt ? 0 : 1;
            if (fieldsExp != null)
            {
                fieldDocument = new FieldsDocument();

                var members = (fieldsExp.Body as NewExpression).Members;
                foreach (var m in members)
                {
                    fieldDocument.Add(new BsonElement(m.Name, 1));
                }

            }
            return fieldDocument;
        }

        #endregion

        
        
    }
}
