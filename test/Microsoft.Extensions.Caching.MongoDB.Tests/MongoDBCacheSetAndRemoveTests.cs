// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using Xunit;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class MongoDBCacheSetAndRemoveTests : BaseMongoDbTests
    {
        [Fact(Skip = SkipReason)]
        public void MongoDBCache_AbsoluteExpiration_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = utcNow.AddMinutes(1),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
            var collection = MongoDBCacheExtensions.GetCollection(mongoCache);
            var key = "MongoDBCache_AbsoluteExpiration_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration" + GetRandomNumber();
            var value = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // Act
            mongoCache.Set(key, value, options, utcNow);
            var itemValue = mongoCache.Get(key);
            var itemInDb = collection.Find(f => f.Key == key).FirstOrDefault();

            // Assert
            
            // Note that MongoDB will store the data as Unix Time Milliseconds
            Assert.Equal(
                options.AbsoluteExpiration.Value.ToUnixTimeMilliseconds(),
                itemInDb.EffectiveExpirationTimeUtc.ToUnixTimeMilliseconds());

            Assert.True(EqualsByteArray(value, itemValue));
        }

        [Fact(Skip = SkipReason)]
        public void MongoDBCache_AbsoluteExpiration_Is_After_Sliding_So_Effective_Should_Be_Sliding()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = utcNow.AddMinutes(2),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };
            var collection = MongoDBCacheExtensions.GetCollection(mongoCache);
            var key = "MongoDBCache_AbsoluteExpiration_Is_After_Sliding_So_Effective_Should_Be_Sliding" + GetRandomNumber();
            var value = new byte[] { 11, 12, 13, 14, 4, 5, 6, 7, 8, 9, 10 };

            // Act
            mongoCache.Set(key, value, options, utcNow);
            var itemValue = mongoCache.Get(key);
            var item = collection.Find(f => f.Key == key).FirstOrDefault();

            // Assert
            Assert.Equal((utcNow + options.SlidingExpiration.Value).ToUnixTimeMilliseconds(),
                item.EffectiveExpirationTimeUtc.ToUnixTimeMilliseconds());
            Assert.True(EqualsByteArray(value, itemValue));
        }

        [Fact(Skip = SkipReason)]
        public void MongoDBCache_AbsoluteExpirationRelativeToNow_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
            var collection = MongoDBCacheExtensions.GetCollection(mongoCache);
            var key = "MongoDBCache_AbsoluteExpirationRelativeToNow_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration" + GetRandomNumber();

            // Act
            mongoCache.Set(key, new byte[0], options, utcNow);            

            // Assert
            var item = collection.Find(f => f.Key == key).FirstOrDefault();
            Assert.Equal((utcNow + options.AbsoluteExpirationRelativeToNow).Value.ToUnixTimeMilliseconds(),
                item.EffectiveExpirationTimeUtc.ToUnixTimeMilliseconds());
        }

        [Fact(Skip = SkipReason)]
        public void MongoDBCache_AbsoluteExpirationRelativeToNow_Is_After_Sliding_So_Effective_Should_Be_Sliding()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };
            var collection = MongoDBCacheExtensions.GetCollection(mongoCache);
            var key = "MongoDBCache_AbsoluteExpirationRelativeToNow_Is_After_Sliding_So_Effective_Should_Be_Sliding" + GetRandomNumber();

            // Act
            mongoCache.Set(key, new byte[0], options, utcNow);            

            // Assert
            var item = collection.Find(f => f.Key == key).FirstOrDefault();
            Assert.Equal((utcNow + options.SlidingExpiration).Value.ToUnixTimeMilliseconds(),
                item.EffectiveExpirationTimeUtc.ToUnixTimeMilliseconds());
        }
    }
}
