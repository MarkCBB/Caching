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

            // If there is some Sliding time specified the real Expiration time will be updated 
            if (item.SlidingTimeTicks > 0)
            {
                var effectiveExpirationWithSlidingAdded =
                     item.EffectiveExpirationTimeUtc.AddMilliseconds(item.SlidingTimeTicks);

                if (effectiveExpirationWithSlidingAdded < item.AbsoluteExpirationTimeUtc)
                {
                    this.TryUpdateEffectiveExpirationTimeUtc(key, effectiveExpirationWithSlidingAdded);
                }
                else
                {
                    if (effectiveExpirationWithSlidingAdded > item.AbsoluteExpirationTimeUtc)
                    {
                        this.TryUpdateEffectiveExpirationTimeUtc(key, item.AbsoluteExpirationTimeUtc);
                    }
                }
            }

            return item.Value;
        }

        public void Dispose()
        {
            
        }
    }
}