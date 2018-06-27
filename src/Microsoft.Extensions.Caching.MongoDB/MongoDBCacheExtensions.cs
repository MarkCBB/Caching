using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public static class MongoDBCacheExtensions
    {
        static private readonly FilterDefinitionBuilder<CacheItemModel> __filterBuilder;
        static private readonly UpdateDefinitionBuilder<CacheItemModel> __updateBuilder;        
        static private readonly ProjectionDefinition<CacheItemModel> __fullItemProjectionDefinition;
        static private readonly ProjectionDefinition<CacheItemModel> __refreshItemProjectionDefinition;
        static private readonly FindOptions<CacheItemModel> __findOptions;
        static private readonly FindOptions<CacheItemModel> __findRefreshOptions;
        static private readonly InsertOneOptions __insertOneOptions;
        static private bool __commandForCreatingIndexAlreadySent;
        static private object __lockIndexCreation;

        static MongoDBCacheExtensions()
        {
            __lockIndexCreation = new object();
            __filterBuilder = new FilterDefinitionBuilder<CacheItemModel>();
            __updateBuilder = new UpdateDefinitionBuilder<CacheItemModel>();
            var projectionBuilder = new ProjectionDefinitionBuilder<CacheItemModel>();
            __fullItemProjectionDefinition = projectionBuilder
                .Include(x => x.Key)
                .Include(x => x.SlidingTimeTicks)
                .Include(x => x._absoluteExpirationDateTimeUtc)
                .Include(x => x._effectiveExpirationDateTimeUtc)
                .Include(x => x.Value);
            __refreshItemProjectionDefinition = projectionBuilder
                .Include(x => x.Key)
                .Include(x => x.SlidingTimeTicks)
                .Include(x => x._absoluteExpirationDateTimeUtc)
                .Include(x => x._effectiveExpirationDateTimeUtc);                

            __findOptions = new FindOptions<CacheItemModel>()
            {
                Projection = __fullItemProjectionDefinition,
                Limit = 1
            };
            __findRefreshOptions = new FindOptions<CacheItemModel>()
            {
                Projection = __refreshItemProjectionDefinition,
                Limit = 1
            };
            __insertOneOptions = new InsertOneOptions();
            lock (__lockIndexCreation)
            {
                __commandForCreatingIndexAlreadySent = false;
            }
        }

        public static IMongoCollection<CacheItemModel> GetCollection(this MongoDBCache obj)
        {
            return new MongoClient(obj.Options.ConnectionString)
                .GetDatabase(obj.Options.DatabaseName)
                .GetCollection<CacheItemModel>(obj.Options.CollectionName);
        }

        public static void CreateIndexes(this MongoDBCache obj)
        {
            if (!__commandForCreatingIndexAlreadySent)
            {
                lock (__lockIndexCreation)
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
                                    .Ascending(x => x.SlidingTimeTicks)
                                    .Ascending(x => x._absoluteExpirationDateTimeUtc)
                                    .Ascending(x => x._effectiveExpirationDateTimeUtc)
                                    .Ascending(x => x.Value);
                                var indexCoverQueryOptions = new CreateIndexOptions<CacheItemModel>()
                                {
                                    Background = obj.Options.CreateIndexesInBackground,
                                    Name = "fullItemIndex"
                                };
                                models.Add(new CreateIndexModel<CacheItemModel>(indexCoverQueryColumns, indexCoverQueryOptions));
                            }

                            if (obj.Options.CreateTTLIndex)
                            {
                                var TTLIndexColumns = indexBuilder
                                    .Ascending(x => x._effectiveExpirationDateTimeUtc);
                                var TTLIndexOptions = new CreateIndexOptions<CacheItemModel>()
                                {
                                    Background = obj.Options.CreateIndexesInBackground,
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
                            .Project<CacheItemModel>(__fullItemProjectionDefinition)
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

        public static async Task<CacheItemModel> TryGetItemAsync(
            this MongoDBCache obj,
            string key,
            CancellationToken cancellationToken)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    
                    var collection = GetCollection(obj);
                    var cursorAsync = await collection.FindAsync(
                        __filterBuilder.Eq(x => x.Key, key),
                        __findOptions,
                        cancellationToken);
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

        public static async Task<bool> TryInsertItemAsync(
            this MongoDBCache obj,
            CacheItemModel item,
            CancellationToken cancellationToken)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    var collection = GetCollection(obj);
                    await collection.InsertOneAsync(
                        item,
                        __insertOneOptions,
                        cancellationToken);
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
                            x => x._effectiveExpirationDateTimeUtc,
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

        public static async Task<bool> TryUpdateEffectiveExpirationTimeUtcAsync(
            this MongoDBCache obj,
            string key,
            DateTimeOffset newRealExpirationTimeUtc,
            CancellationToken cancellationToken)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    var collection = GetCollection(obj);
                    await collection.UpdateOneAsync(
                        filter: __filterBuilder.Eq(x => x.Key, key),
                        update: __updateBuilder.Set(
                            x => x._effectiveExpirationDateTimeUtc,
                            newRealExpirationTimeUtc),
                        cancellationToken: cancellationToken);
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

        public static bool TryDeleteItem(
            this MongoDBCache obj,
            string key)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    GetCollection(obj)
                        .DeleteOne(
                        __filterBuilder.Eq(x => x.Key, key));
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

        public static async Task<bool> TryDeleteItemAsync(
            this MongoDBCache obj,
            string key,
            CancellationToken cancellationToken)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    await GetCollection(obj)
                            .DeleteOneAsync(
                            __filterBuilder.Eq(x => x.Key, key),
                            cancellationToken);
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

        public static bool TryGetItemForRefresh(this MongoDBCache obj, string key, ref CacheItemModel item)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    item = GetCollection(obj)
                        .Find(
                        __filterBuilder.Eq(x => x.Key, key))
                            .Project<CacheItemModel>(__refreshItemProjectionDefinition)
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

        public static async Task<CacheItemModel> TryGetItemForRefreshAsync(
            this MongoDBCache obj,
            string key,
            CancellationToken cancellationToken)
        {
            var retries = 0;

            while (retries <= obj.Options.MaxRetries)
            {
                try
                {
                    var collection = GetCollection(obj);
                    var cursorAsync = await collection.FindAsync(
                        __filterBuilder.Eq(x => x.Key, key),
                        __findRefreshOptions,
                        cancellationToken);
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

        public static void CheckAndUpdateEffectiveExpirationTime(
            this MongoDBCache obj,
            string key,
            CacheItemModel item)
        {
            var effectiveExpirationTimeUtc = CacheItemModel.GetEffectiveExpirationTimeUtc(item, DateTimeOffset.UtcNow);
            if (effectiveExpirationTimeUtc != item.EffectiveExpirationTimeUtc)
            {
                obj.TryUpdateEffectiveExpirationTimeUtc(key, effectiveExpirationTimeUtc);
                item.EffectiveExpirationTimeUtc = effectiveExpirationTimeUtc;
            }
        }

        public static async Task CheckAndUpdateEffectiveExpirationTimeAsync(
            this MongoDBCache obj,
            string key,
            CacheItemModel item,
            CancellationToken token)
        {
            var effectiveExpirationTimeUtc = CacheItemModel.GetEffectiveExpirationTimeUtc(item, DateTimeOffset.UtcNow);
            if (effectiveExpirationTimeUtc != item.EffectiveExpirationTimeUtc)
            {
                await obj.TryUpdateEffectiveExpirationTimeUtcAsync(key, effectiveExpirationTimeUtc, token);
                item.EffectiveExpirationTimeUtc = effectiveExpirationTimeUtc;
            }
        }
    }
}
