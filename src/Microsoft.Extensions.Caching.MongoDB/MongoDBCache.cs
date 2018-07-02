// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class MongoDBCache : IDisposable, IDistributedCache
    {
        public MongoDBCacheOptions Options;

        public MongoDBCache(IOptions<MongoDBCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            Options = optionsAccessor.Value;

            if (string.IsNullOrEmpty(Options.ConnectionString))
            {
                throw new ArgumentException(nameof(Options.ConnectionString)
                    + " must contain a valid value");
            }
        }

        public byte[] Get(string key)
        {
            ValidateKeyParameter(key);

            CacheItemModel item = null;
            if ((!this.TryGetItem(key, ref item) || item == null))
            {
                return null;
            }

            this.CheckAndUpdateEffectiveExpirationTime(key, item);

            return item.Value;
        }     

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            ValidateKeyParameter(key);

            var item = await this.TryGetItemAsync(key, token);
            if (item == null)
            {
                return null;
            }

            await this.CheckAndUpdateEffectiveExpirationTimeAsync(key, item, token);

            return item.Value; 
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            Set(key, value, options, DateTimeOffset.UtcNow);
        }

        public void Set(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            DateTimeOffset utcNow)
        {
            ValidateArguments(key, value, options);

            this.CreateIndexes();

            ValidateOptions(options);

            CacheItemModel newItem = CacheItemModel.CreateNewItem(
                key,
                value,
                options.AbsoluteExpirationRelativeToNow,
                options.AbsoluteExpiration,
                options.SlidingExpiration,
                utcNow);

            this.TryInsertItem(newItem);
        }

        public Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default(CancellationToken))
        {
            return SetAsync(key, value, options, DateTimeOffset.UtcNow, token);
        }

        public async Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            DateTimeOffset utcNow,
            CancellationToken token = default(CancellationToken))
        {
            ValidateArguments(key, value, options);

            this.CreateIndexes();

            ValidateOptions(options);

            CacheItemModel newItem = CacheItemModel.CreateNewItem(
                key,
                value,
                options.AbsoluteExpirationRelativeToNow,
                options.AbsoluteExpiration,
                options.SlidingExpiration,
                utcNow);

            await this.TryInsertItemAsync(newItem, token);
        }

        public void Remove(string key)
        {
            ValidateKeyParameter(key);
            this.TryDeleteItem(key);
        }

        async Task IDistributedCache.RemoveAsync(string key, CancellationToken token)
        {
            ValidateKeyParameter(key);
            await this.TryDeleteItemAsync(key, token);
        }

        public void Refresh(string key)
        {
            ValidateKeyParameter(key);

            CacheItemModel item = null;

            this.TryGetItemForRefresh(key, ref item);

            this.CheckAndUpdateEffectiveExpirationTime(key, item);
        }

        public async Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            ValidateKeyParameter(key);

            var item = await this.TryGetItemForRefreshAsync(key, token);

            await this.CheckAndUpdateEffectiveExpirationTimeAsync(key, item, token);
        }

        private static void ValidateArguments(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
        }

        private static void ValidateKeyParameter(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
        }

        private void ValidateOptions(DistributedCacheEntryOptions options)
        {
            if (!options.SlidingExpiration.HasValue
                && !options.AbsoluteExpiration.HasValue
                && !options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                throw new ArgumentException("At least one of the following options: SlidingExpiration, AbsoluteExpiration or AbsoluteExpirationRelativeToNow "
                    + "must contain a valid value. If SlidingExpiration or AbsoluteExpirationRelativeToNow are detailed must be greather than 0. "
                    + "If AbsoluteExpiration is detailed must be greather than current time");
            }
        }

        public void Dispose()
        {

        }
    }
}