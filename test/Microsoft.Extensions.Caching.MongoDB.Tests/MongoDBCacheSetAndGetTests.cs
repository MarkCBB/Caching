// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using Xunit;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class MongoDBCacheSetAndGetTests : BaseMongoDbTests
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
                ToUnixTimeMilliseconds(itemInDb.EffectiveExpirationTimeUtc));

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
            // EffectiveExpirationTimeUtc should be a bit greather than the initial expiration date
            // and the difference should minor than two seconds
            Assert.True(
                (ToUnixTimeMilliseconds(item.EffectiveExpirationTimeUtc) >=
                (utcNow + options.SlidingExpiration).Value.ToUnixTimeMilliseconds()));
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
            var value = new byte[] { 15, 16, 17, 18, 20, 5, 6, 7, 8, 9, 10 };

            // Act
            mongoCache.Set(key, value, options, utcNow);
            var itemValue = mongoCache.Get(key);
            var item = collection.Find(f => f.Key == key).FirstOrDefault();

            // Assert
            Assert.Equal((utcNow + options.AbsoluteExpirationRelativeToNow).Value.ToUnixTimeMilliseconds(),
                ToUnixTimeMilliseconds(item.EffectiveExpirationTimeUtc));
            Assert.True(EqualsByteArray(value, itemValue));
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
            var value = new byte[] { 15, 16, 17, 18, 20, 5, 6, 7, 8, 9, 10 };

            // Act
            mongoCache.Set(key, value, options, utcNow);
            // In this case a get will update the AbsoluteExpirationRelativeToNow
            var itemValue = mongoCache.Get(key);
            var item = collection.Find(f => f.Key == key).FirstOrDefault();

            // Assert
            
            // EffectiveExpirationTimeUtc should be a bit greather than the initial expiration date
            // and the difference should minor than two seconds
            Assert.True(
                (ToUnixTimeMilliseconds(item.EffectiveExpirationTimeUtc) >=
                (utcNow + options.SlidingExpiration).Value.ToUnixTimeMilliseconds()));
            Assert.True(EqualsByteArray(value, itemValue));
        }

        // Se tiene que hacer un test case para demostrar que el comportamiento es correcto
        // en caso de que se intente obtener un valor con una clave inexistente

        [Fact(Skip = SkipReason)]
        public void MongoDBCache_Getting_a_non_existing_key_should_be_null()
        {
            // Arrange
            var key = "MongoDBCache_Getting_a_non_existing_key_should_be_null" + GetRandomNumber();

            // Act
            var itemValue = mongoCache.Get(key);

            // Assert
            Assert.True(itemValue == null);
        }

        [Fact(Skip = SkipReason)]
        public void MongoDBCache_Set_the_same_key_twice_should_get_the_last_value()
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
            var value = new byte[] { 15, 16, 17, 18, 20, 5, 6, 7, 8, 9, 10 };
            var value2 = new byte[] { 21, 22, 23, 18, 20, 5, 6, 7, 8, 9, 10 };

            // Act
            mongoCache.Set(key, value, options, utcNow);
            mongoCache.Set(key, value2, options, utcNow);
            // In this case a get will update the AbsoluteExpirationRelativeToNow
            var itemValue = mongoCache.Get(key);

            // Assert
            Assert.True(EqualsByteArray(value2, itemValue));
        }
    }
}
