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
        static private readonly UpdateOptions __updateOptions;

        static MongoDBCacheExtensions()
        {
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
            __updateOptions = new UpdateOptions()
            {
                IsUpsert = true
            };
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
                if (cancellationToken != default)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
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
                    var filter = __filterBuilder.Eq(x => x.Key, item.Key);
                    var updateDefinition = __updateBuilder
                        .Set(x => x.Key, item.Key)
                        .Set(x => x.SlidingTimeTicks, item.SlidingTimeTicks)
                        .Set(x => x._absoluteExpirationDateTimeUtc, item._absoluteExpirationDateTimeUtc)
                        .Set(x => x._effectiveExpirationDateTimeUtc, item._effectiveExpirationDateTimeUtc)
                        .Set(x => x.Value, item.Value);
                    collection.UpdateOne(filter, updateDefinition, __updateOptions);
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
                if (cancellationToken != default)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                try
                {
                    var collection = GetCollection(obj);
                    var filter = __filterBuilder.Eq(x => x.Key, item.Key);
                    var updateDefinition = __updateBuilder
                        .Set(x => x.Key, item.Key)
                        .Set(x => x.SlidingTimeTicks, item.SlidingTimeTicks)
                        .Set(x => x._absoluteExpirationDateTimeUtc, item._absoluteExpirationDateTimeUtc)
                        .Set(x => x._effectiveExpirationDateTimeUtc, item._effectiveExpirationDateTimeUtc)
                        .Set(x => x.Value, item.Value);
                    await collection.UpdateOneAsync(
                        filter,
                        updateDefinition,
                        __updateOptions,
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
                            new DateTime(
                                ticks: newRealExpirationTimeUtc.UtcTicks,
                                kind: DateTimeKind.Utc)));
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
                if (cancellationToken != default)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                try
                {
                    var collection = GetCollection(obj);
                    await collection.UpdateOneAsync(
                        filter: __filterBuilder.Eq(x => x.Key, key),
                        update: __updateBuilder.Set(
                            x => x._effectiveExpirationDateTimeUtc,
                            new DateTime(
                                    ticks: newRealExpirationTimeUtc.UtcTicks,
                                    kind: DateTimeKind.Utc)),
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
                if (cancellationToken != default)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
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
                if (cancellationToken != default)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
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
