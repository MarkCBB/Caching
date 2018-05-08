using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public static class MongoDBCacheExtensions
    {
        static private readonly FilterDefinitionBuilder<CacheItemModel> __filterBuilder;
        static private readonly UpdateDefinitionBuilder<CacheItemModel> __updateBuilder;        
        

        static MongoDBCacheExtensions()
        {
            __filterBuilder = new FilterDefinitionBuilder<CacheItemModel>();
            __updateBuilder = new UpdateDefinitionBuilder<CacheItemModel>();
        }

        public static IMongoCollection<CacheItemModel> GetCollection(this MongoDBCache obj)
        {
            return new MongoClient(obj.Options.ConnectionString)
                .GetDatabase(obj.Options.DatabaseName)
                .GetCollection<CacheItemModel>(obj.Options.CollectionName);
        }

        public static bool TryGetItem(this MongoDBCache obj, string key, ref CacheItemModel item)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    var collection = GetCollection(obj);
                    item = collection.Find(
                        __filterBuilder.Eq(x => x.Key, key))
                        .Limit(1)
                        .FirstOrDefault();
                    return true;
                }
                catch 
                {
                    System.Threading.Thread.CurrentThread.Join(obj.Options.TimeToWait);
                    retries++;
                }
            }

            return false;
        }

        public static async Task<CacheItemModel> TryGetItemAsync(this MongoDBCache obj, string key)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    var collection = GetCollection(obj);
                    var cursorAsync = await collection.FindAsync(
                        __filterBuilder.Eq(x => x.Key, key));
                    return await cursorAsync.FirstOrDefaultAsync();                    
                }
                catch
                {
                    await Task.Delay(obj.Options.TimeToWait);
                    retries++;
                }
            }

            return null;
        }

        public static bool TryInsertItem(this MongoDBCache obj, CacheItemModel item)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    var collection = GetCollection(obj);
                    collection.InsertOne(item);
                    return true;
                }
                catch
                {
                    System.Threading.Thread.CurrentThread.Join(obj.Options.TimeToWait);
                    retries++;
                }
            }

            return false;
        }

        public static async Task<bool> TrySetItemAsync(this MongoDBCache obj, CacheItemModel item)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    var collection = GetCollection(obj);
                    await collection.InsertOneAsync(item);
                    return true;
                }
                catch
                {
                    await Task.Delay(obj.Options.TimeToWait);
                    retries++;
                }
            }

            return false;
        }

        public static bool TryUpdateEffectiveExpirationTimeUtc(this MongoDBCache obj, string key, DateTimeOffset newRealExpirationTimeUtc)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    var collection = GetCollection(obj);
                    collection.UpdateOne(
                        __filterBuilder.Eq(x => x.Key, key),
                        __updateBuilder.Set(
                            x => x.EffectiveExpirationTimeUtc,
                            newRealExpirationTimeUtc));
                    return true;
                }
                catch
                {
                    System.Threading.Thread.CurrentThread.Join(obj.Options.TimeToWait);
                    retries++;
                }
            }

            return false;
        }

        public static async Task<bool> TryUpdateEffectiveExpirationTimeUtcAsync(this MongoDBCache obj, string key, DateTimeOffset newRealExpirationTimeUtc)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    var collection = GetCollection(obj);
                    await collection.UpdateOneAsync(
                        __filterBuilder.Eq(x => x.Key, key),
                        __updateBuilder.Set(
                            x => x.EffectiveExpirationTimeUtc,
                            newRealExpirationTimeUtc));
                    return true;
                }
                catch
                {
                    await Task.Delay(obj.Options.TimeToWait);
                    retries++;
                }
            }

            return false;
        }

    }
}
