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
    public class MongoDBCache : IDisposable
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
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            CacheItemModel item = null;
            if (!this.TryGetItem(key, ref item))
            {
                return null;
            }

            var effectiveExpirationTimeUtc = CacheItemModel.GetEffectiveExpirationTimeUtc(item, DateTimeOffset.UtcNow);
            if (effectiveExpirationTimeUtc != item.EffectiveExpirationTimeUtc)
            {
                this.TryUpdateEffectiveExpirationTimeUtc(key, effectiveExpirationTimeUtc);
            }

            return item.Value;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
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

            var utcNow = DateTimeOffset.UtcNow;
            if (!options.SlidingExpiration.HasValue
                && !options.AbsoluteExpiration.HasValue
                && !options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                throw new ArgumentException("At least one of the following options: SlidingExpiration, AbsoluteExpiration or AbsoluteExpirationRelativeToNow "
                    + "must contain a valid value. If SlidingExpiration or AbsoluteExpirationRelativeToNow are detailed must be greather than 0. "
                    + "If AbsoluteExpiration is detailed must be greather than current time");
            }

            CacheItemModel newItem = CacheItemModel.CreateNewItem(
                key,
                value,
                options.AbsoluteExpirationRelativeToNow,
                options.AbsoluteExpiration,
                options.SlidingExpiration,
                utcNow);

            this.TryInsertItem(newItem);
        }

        public void Dispose()
        {
            
        }
    }
}