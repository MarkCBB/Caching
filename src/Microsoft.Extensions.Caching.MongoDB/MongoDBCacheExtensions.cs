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
        static private readonly ProjectionDefinition<CacheItemModel> __shortProjectionDefinition;
        static private readonly FindOptions<CacheItemModel> __findOptions;
        static private bool __commandForCreatingIndexAlreadySent;

        static MongoDBCacheExtensions()
        {
            __filterBuilder = new FilterDefinitionBuilder<CacheItemModel>();
            __updateBuilder = new UpdateDefinitionBuilder<CacheItemModel>();
            var projectionBuilder = new ProjectionDefinitionBuilder<CacheItemModel>();
            __shortProjectionDefinition = projectionBuilder
                .Include(x => x.Key)
                .Include(x => x.Value)
                .Include(x => x.SlidingTimeTicks)
                .Include(x => x._absoluteExpirationTimeUtc);
            __findOptions = new FindOptions<CacheItemModel>()
            {
                Projection = __shortProjectionDefinition,
                Limit = 1
            };
            __commandForCreatingIndexAlreadySent = false;
        }

        public static IMongoCollection<CacheItemModel> GetCollection(this MongoDBCache obj)
        {
            return new MongoClient(obj.Options.ConnectionString)
                .GetDatabase(obj.Options.DatabaseName)
                .GetCollection<CacheItemModel>(obj.Options.CollectionName);
        }

        public static void CreateCoverIndex(this MongoDBCache obj, bool background = true)
        {
            if (!__commandForCreatingIndexAlreadySent)
            {
                if (!obj.Options.CreateCoverIndex && !obj.Options.CreateTTLIndex)
                {
                    __commandForCreatingIndexAlreadySent = true;
                }
                else
                {
                    var models = new List<CreateIndexModel<CacheItemModel>>();
                    var indexBuilder = new IndexKeysDefinitionBuilder<CacheItemModel>();
                    if (obj.Options.CreateCoverIndex)
                    {
                        var indexCoverQueryColumns = indexBuilder
                            .Ascending(x => x.Key)
                            .Ascending(x => x.Value)
                            .Ascending(x => x.SlidingTimeTicks)
                            .Ascending(x => x._absoluteExpirationTimeUtc);
                        var indexCoverQueryOptions = new CreateIndexOptions<CacheItemModel>()
                        {
                            Background = background,
                            Name = "fullItemIndex"
                        };
                        models.Add(new CreateIndexModel<CacheItemModel>(indexCoverQueryColumns, indexCoverQueryOptions));
                    }

                    if (obj.Options.CreateTTLIndex)
                    {
                        var TTLIndexColumns = indexBuilder
                            .Ascending(x => x._effectiveExpirationTimeUtc);
                        var TTLIndexOptions = new CreateIndexOptions<CacheItemModel>()
                        {
                            Background = background,
                            ExpireAfter = TimeSpan.Zero,
                            Name = "TTLItemIndex"
                        };
                        models.Add(new CreateIndexModel<CacheItemModel>(TTLIndexColumns, TTLIndexOptions));
                    }
                    GetCollection(obj).Indexes.CreateMany(models);
                    __commandForCreatingIndexAlreadySent = true;
                }
            }
        }

        public static bool TryGetItem(this MongoDBCache obj, string key, ref CacheItemModel item)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    item = GetCollection(obj)
                        .Find(
                        __filterBuilder.Eq(x => x.Key, key))
                            .Project<CacheItemModel>(__shortProjectionDefinition)
                            .Limit(1)
                            .FirstOrDefault();
                    return true;
                }
                catch
                {
                    System.Threading.Thread.CurrentThread.Join(obj.Options.MillisToWait);
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
                        __filterBuilder.Eq(x => x.Key, key),
                        __findOptions);
                    return await cursorAsync.FirstOrDefaultAsync();                    
                }
                catch
                {
                    await Task.Delay(obj.Options.MillisToWait);
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
                    System.Threading.Thread.CurrentThread.Join(obj.Options.MillisToWait);
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
                    await Task.Delay(obj.Options.MillisToWait);
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
                    System.Threading.Thread.CurrentThread.Join(obj.Options.MillisToWait);
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
                    await Task.Delay(obj.Options.MillisToWait);
                    retries++;
                }
            }

            return false;
        }

    }
}
