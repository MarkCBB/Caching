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

            var effectiveExpirationTimeUtc = GetEffectiveExpirationTimeUtc(item, DateTimeOffset.UtcNow);
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

            CacheItemModel newItem = CreateNewItem(key, value, options, utcNow);

            this.TryInsertItem(newItem);
        }

        public static CacheItemModel CreateNewItem(string key, byte[] value, DistributedCacheEntryOptions options, DateTimeOffset utcNow)
        {
            var absoluteExpirationTimeUtc = DateTimeOffset.MinValue;
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration.Value > utcNow)
            {
                absoluteExpirationTimeUtc = options.AbsoluteExpiration.Value.UtcDateTime;
            }
            else
            {
                if (options.AbsoluteExpirationRelativeToNow.HasValue && options.AbsoluteExpirationRelativeToNow.Value.Ticks > 0)
                {
                    absoluteExpirationTimeUtc = (utcNow + options.AbsoluteExpirationRelativeToNow.Value);
                }
            }

            var newItem = new CacheItemModel()
            {
                Key = key,
                Value = value,
                AbsoluteExpirationTimeUtc = absoluteExpirationTimeUtc,
                SlidingTimeTicks = (options.SlidingExpiration.HasValue && options.SlidingExpiration.Value.Ticks > 0) ? (options.SlidingExpiration.Value.Ticks) : 0
            };
            
            newItem.EffectiveExpirationTimeUtc = GetEffectiveExpirationTimeUtc(newItem, utcNow);
            return newItem;
        }

        private static DateTimeOffset GetEffectiveExpirationTimeUtc(CacheItemModel item, DateTimeOffset utcNow)
        {
            if (item.SlidingTimeTicks == 0)
            {
                return item.AbsoluteExpirationTimeUtc;
            }

            var SlidingExpirationDate = utcNow.AddTicks(item.SlidingTimeTicks);

            if ((item.AbsoluteExpirationTimeUtc == DateTimeOffset.MinValue) ||
                (SlidingExpirationDate < item.AbsoluteExpirationTimeUtc))
            {
                return SlidingExpirationDate;
            }
            else
            {
                return item.AbsoluteExpirationTimeUtc;
            }
        }

        public void Dispose()
        {
            
        }
    }
}