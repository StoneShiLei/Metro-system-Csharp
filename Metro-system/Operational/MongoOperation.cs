/*----------------------------------------------------------------
// Author: ipepha@aliyun.com
// MIT lisence
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Metro_system.Conf;
using Metro_system.Models;

namespace Metro_system.Operations
{
    public static class MongoOperation
    {
        private static MongoClient mongoClient = new MongoClient(Configurations.mongoConn);
        private static readonly ProjectionDefinition<BsonDocument> projection;

        static MongoOperation()
        {
            projection = Builders<BsonDocument>.Projection.Exclude("_id");
        }

        #region general mongo methods

        public static void InsertDocument(MongoCollectionName collectionName, object data)
        {
            try
            {
                var database = mongoClient.GetDatabase(Configurations.mongoDBName);
                var collection = database.GetCollection<BsonDocument>(GetCollectionNameFromCollectionEnum(collectionName));

                var bsonDoc = GenerateBsonDocumentFromObject(data, collection);

                collection.InsertOne(bsonDoc);
                
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public static List<T> FindDocument<T>(MongoCollectionName collectionName, Dictionary<string, string> queryCondition)
        {
            try
            {
                var database = mongoClient.GetDatabase(Configurations.mongoDBName);
                var collection = database.GetCollection<BsonDocument>(GetCollectionNameFromCollectionEnum(collectionName));

                var bsonDoc = GenerateFilterDocumentFromDict(collection, queryCondition);

                FilterDefinition<BsonDocument> filter = bsonDoc;


                var bsonList = collection.Find(filter).Project(projection).ToList();


                List<T> result = new List<T>();
                foreach (var bson in bsonList)
                {
                    string json = DeserializeBsonDocumentToJson(collection, bson);
                    T item = JsonConvert.DeserializeObject<T>(json);
                    result.Add(item);

                }

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void RemoveDocument(MongoCollectionName collectionName, Dictionary<string, string> removeCondition, bool allowEmptyRemove = false)
        {

            var database = mongoClient.GetDatabase(Configurations.mongoDBName);
            var collection = database.GetCollection<BsonDocument>(GetCollectionNameFromCollectionEnum(collectionName));

            var bsonDoc = GenerateFilterDocumentFromDict(collection, removeCondition);

            FilterDefinition<BsonDocument> filter = bsonDoc;


            var result = collection.DeleteMany(filter);

            if (result.IsAcknowledged == true)
            {
                if (result.DeletedCount == 0 && !allowEmptyRemove)
                {
                    throw new ROException("数据删除失败");
                }
            }
            else
            {
                throw new Exception("unknown mongo exception");
            }

        }

        public static void UpdateDocument(MongoCollectionName collectionName, Dictionary<string, string> queryCondition, MongoUpdateModel updateCondition, bool allowEmptyUpdate = false)
        {
            var database = mongoClient.GetDatabase(Configurations.mongoDBName);
            var collection = database.GetCollection<BsonDocument>(GetCollectionNameFromCollectionEnum(collectionName));

            var conditionDoc = GenerateFilterDocumentFromDict(collection, queryCondition);
            var filter = conditionDoc;

            var update = GenerateUpdateDocument(updateCondition, collection);

            var result = collection.UpdateMany(filter, update);

            if (result.IsAcknowledged == true)
            {
                if (result.ModifiedCount == 0 && !allowEmptyUpdate)
                {
                    throw new ROException("数据更新失败");
                }
            }
            else
            {
                throw new Exception("数据更新失败");
            }
        }

        public static void ReplaceDocument(MongoCollectionName collectionName, Dictionary<string, string> queryCondition, object data)
        {
            var database = mongoClient.GetDatabase(Configurations.mongoDBName);
            var collection = database.GetCollection<BsonDocument>(GetCollectionNameFromCollectionEnum(collectionName));

            var conditionDoc = GenerateFilterDocumentFromDict(collection, queryCondition);
            FilterDefinition<BsonDocument> filter = conditionDoc;

            var update = GenerateBsonDocumentFromObject(data, collection);

            var result = collection.ReplaceOne(filter, update);

            if (result.IsAcknowledged == true)
            {
                if (result.ModifiedCount == 0)
                {
                    throw new ROException("数据更新失败");
                }
            }
            else
            {
                throw new Exception("数据更新失败");
            }
        }

        public static List<T> FindDocumentByPage<T>(MongoCollectionName collectionName, Dictionary<string, string> queryCondition, int pageIndex, int pageSize, string sortKey, MongoSortTypeEnum sortType)
        {
            var database = mongoClient.GetDatabase(Configurations.mongoDBName);
            var collection = database.GetCollection<BsonDocument>(GetCollectionNameFromCollectionEnum(collectionName));

            var bsonDoc = GenerateFilterDocumentFromDict(collection, queryCondition);
            FilterDefinition<BsonDocument> filter = bsonDoc;

            return FindDocumentByPage<T>(collectionName, filter, pageIndex, pageSize, sortKey, sortType);
        }

        public static List<T> FindDocumentByPage<T>(MongoCollectionName collectionName, FilterDefinition<BsonDocument> queryCondition, int pageIndex, int pageSize, string sortKey, MongoSortTypeEnum sortType)
        {
            var database = mongoClient.GetDatabase(Configurations.mongoDBName);
            var collection = database.GetCollection<BsonDocument>(GetCollectionNameFromCollectionEnum(collectionName));

            FilterDefinition<BsonDocument> filter = queryCondition;

            SortDefinition<BsonDocument> sort = null;
            if (sortType == MongoSortTypeEnum.Asc)
            {
                sort = Builders<BsonDocument>.Sort.Ascending(sortKey);
            }
            else if (sortType == MongoSortTypeEnum.Desc)
            {
                sort = Builders<BsonDocument>.Sort.Descending(sortKey);
            }

            int startIndex = pageIndex * pageSize;

            var bsonList = collection.Find(filter).Sort(sort).Skip(startIndex).Limit(pageSize).Project(projection).ToList();

            List<T> result = new List<T>();
            foreach (var bson in bsonList)
            {
                string json = DeserializeBsonDocumentToJson(collection, bson);
                T item = JsonConvert.DeserializeObject<T>(json);
                result.Add(item);

            }

            return result;
        }

        public static T FindLastByKey<T>(MongoCollectionName collectionName,string sortKey)
        {
            var database = mongoClient.GetDatabase(Configurations.mongoDBName);
            var collection = database.GetCollection<BsonDocument>(GetCollectionNameFromCollectionEnum(collectionName));

            var filter = Builders<BsonDocument>.Filter.Exists(sortKey);
            SortDefinition<BsonDocument> sort = Builders<BsonDocument>.Sort.Descending(sortKey);

            var bson = collection.Find(filter).Sort(sort).Project(projection).FirstOrDefault();
            if(bson == null)
            {
                return default(T);
            }

            string json = DeserializeBsonDocumentToJson(collection, bson);
            T result = JsonConvert.DeserializeObject<T>(json);

            return result;
        }

        public static int Count(MongoCollectionName collectionName, Dictionary<string, string> queryCondition)
        {
            try
            {
                var database = mongoClient.GetDatabase(Configurations.mongoDBName);
                var collection = database.GetCollection<BsonDocument>(GetCollectionNameFromCollectionEnum(collectionName));

                var bsonDoc = GenerateFilterDocumentFromDict(collection, queryCondition);
                FilterDefinition<BsonDocument> filter = bsonDoc;

                var bsonList = collection.Find(filter).Count();

                return Convert.ToInt32(bsonList);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region support methods

        public static string GenerateDateTimeString(DateTime input)
        {
            return input.ToString("yyyy-MM-ddTHH:mm:ss.00000000+08:00");
        }

        private static BsonDocument GenerateBsonDocumentFromObject(object obj, IMongoCollection<BsonDocument> collection)
        {
            string json = JsonConvert.SerializeObject(obj);

            var jr = new MongoDB.Bson.IO.JsonReader(json);
            var context = BsonDeserializationContext.CreateRoot(jr);
            var bsonDoc = collection.DocumentSerializer.Deserialize(context);

            return bsonDoc;
        }

        private static BsonDocument GenerateBsonDocumentFromListT(object list, string listName, IMongoCollection<BsonDocument> collection)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            obj.Add(listName, list);
            string json = JsonConvert.SerializeObject(obj);

            var jr = new MongoDB.Bson.IO.JsonReader(json);
            var context = BsonDeserializationContext.CreateRoot(jr);
            var bsonDoc = collection.DocumentSerializer.Deserialize(context);

            return bsonDoc;
        }

        private static BsonDocument GenerateFilterDocumentFromDict(IMongoCollection<BsonDocument> collection, Dictionary<string, string> dict)
        {
            string conditionJson = JsonConvert.SerializeObject(dict);
            var jr = new MongoDB.Bson.IO.JsonReader(conditionJson);
            var bdc = BsonDeserializationContext.CreateRoot(jr);
            var bsonDoc = collection.DocumentSerializer.Deserialize(bdc);

            return bsonDoc;
        }

        private static string GetCollectionNameFromCollectionEnum(MongoCollectionName input)
        {
            string result = input.ToString();
            result = result.Replace("_", ".");
            return result;
        }

        private static UpdateDefinition<BsonDocument> GenerateUpdateDocument(MongoUpdateModel param, IMongoCollection<BsonDocument> collection)
        {
            if (param == null)
            {
                throw new Exception("更新数据为空");
            }

            if (param.value.GetType() == typeof(Boolean) || param.value.GetType() == typeof(string))
            {
                if (param.updateType == MongoUpdateTypeEnum.Set)
                {
                    return Builders<BsonDocument>.Update.Set(param.key, param.value);
                }
                else if (param.updateType == MongoUpdateTypeEnum.Inc)
                {
                    return Builders<BsonDocument>.Update.Inc(param.key, param.value);
                }
                else if (param.updateType == MongoUpdateTypeEnum.Push)
                {
                    return Builders<BsonDocument>.Update.Push(param.key, param.value);
                }
            }
            else if (param.value.GetType() == typeof(List<>))
            {

            }
            else
            {
                var valueBson = GenerateBsonDocumentFromObject(param.value, collection);
                if (param.updateType == MongoUpdateTypeEnum.Set)
                {
                    return Builders<BsonDocument>.Update.Set(param.key, valueBson);
                }
                else if (param.updateType == MongoUpdateTypeEnum.Inc)
                {
                    return Builders<BsonDocument>.Update.Inc(param.key, valueBson);
                }
                else if (param.updateType == MongoUpdateTypeEnum.Push)
                {
                    return Builders<BsonDocument>.Update.Push(param.key, valueBson);
                }
            }



            throw new Exception("无法识别的更新类型");
        }

        private static string DeserializeBsonDocumentToJson(IMongoCollection<BsonDocument> collection, BsonDocument doc)
        {
            var stringWriter = new StringWriter();
            var jsonWriter = new MongoDB.Bson.IO.JsonWriter(stringWriter);
            var context = BsonSerializationContext.CreateRoot(jsonWriter);
            collection.DocumentSerializer.Serialize(context, doc);
            string result = stringWriter.ToString();
            return result;
        }

        #endregion

        #region custom methods

        //example.
        public static FilterDefinition<BsonDocument> GenerateFilter_ReadDataByDeviceId(string deviceId, DateTime? minimumCreatedDate = null)
        {
            if (minimumCreatedDate == null)
            {
                return Builders<BsonDocument>.Filter.Eq("deviceId", deviceId);
            }
            else
            {
                var dateString = GenerateDateTimeString((DateTime)minimumCreatedDate);

                return Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("deviceId", deviceId),
                    Builders<BsonDocument>.Filter.Gt("createdDate", dateString));
            }
        }

        #endregion
    }

    public class MongoUpdateModel
    {
        public MongoUpdateTypeEnum updateType;
        public string key;
        public object value;

        public MongoUpdateModel(MongoUpdateTypeEnum type, string key, object value)
        {
            this.updateType = type;
            this.key = key;
            this.value = value;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MongoUpdateTypeEnum
    {
        Set,
        Inc,
        Push,
    }

    public enum MongoCollectionName
    {
        Cards,
    }

    public enum MongoSortTypeEnum
    {
        Asc,
        Desc
    }
}