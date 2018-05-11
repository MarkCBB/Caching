// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using MongoDB.Driver;
using System.Text;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class MongoDBCacheTests : BaseMongoDbTests
    {
        [Fact]
        public void MongoDBCache_InsertingOneCacheItem()
        {
            // Arrange
            var collection = MongoDBCacheExtensions.GetCollection(_mongoCache);
            var keyNameValue = "InsertingOneCacheItem" + Guid.NewGuid().ToString();
            var absolutExpirationTime = DateTimeOffset.UtcNow.AddHours(1);

            // Act
            collection.InsertOne(new CacheItemModel()
            {
                Key = keyNameValue,
                Value = Encoding.ASCII.GetBytes("Test MongoDBCache_InsertingOneCacheItem"),
                AbsoluteExpirationTimeUtc = absolutExpirationTime,
                EffectiveExpirationTimeUtc = absolutExpirationTime.ToUniversalTime()
            });
            var result = collection.Count(f => f.Key == keyNameValue);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void MongoDBCache_DefaultSlidingTimeTicksIsZero()
        {
            // Arrange
            var collection = MongoDBCacheExtensions.GetCollection(_mongoCache);
            var keyNameValue = "DefaultSlidingTimeTicksIsZero" + Guid.NewGuid().ToString();
            var absolutExpirationTime = DateTimeOffset.UtcNow.AddHours(1);

            // Act
            collection.InsertOne(new CacheItemModel()
            {
                Key = keyNameValue,
                Value = Encoding.ASCII.GetBytes("Test MongoDBCache_InsertingOneCacheItem"),
                AbsoluteExpirationTimeUtc = absolutExpirationTime,
                EffectiveExpirationTimeUtc = absolutExpirationTime.ToUniversalTime()
            });
            //Encoding.ASCII.GetString(
            var result = collection.Find(f => f.Key == keyNameValue).FirstOrDefault();

            // Assert
            Assert.Equal(0, result.SlidingTimeTicks);
        }


        [Fact]
        public void MongoDBCache_NoValueInAbsoluteExpirationIsDateTimeMinValue()
        {
            // Arrange
            var collection = MongoDBCacheExtensions.GetCollection(_mongoCache);
            var keyNameValue = "NoValueInAbsoluteExpiration" + Guid.NewGuid().ToString();
            var effectiveExpirationTimeUtc = DateTimeOffset.UtcNow.AddHours(1);

            // Act
            collection.InsertOne(new CacheItemModel()
            {
                Key = keyNameValue,
                Value = Encoding.ASCII.GetBytes("Test MongoDBCache_InsertingOneCacheItem"),                
                EffectiveExpirationTimeUtc = effectiveExpirationTimeUtc.ToUniversalTime()
            });
            //Encoding.ASCII.GetString(
            var result = collection.Find(f => f.Key == keyNameValue).FirstOrDefault();

            // Assert
            Assert.Equal(DateTimeOffset.MinValue, result.AbsoluteExpirationTimeUtc);
        }

        [Fact]
        public void MongoDBCache_A_Full_Cover_Index_Is_Created()
        {
            // Arrange
            var collection = MongoDBCacheExtensions.GetCollection(_mongoCache);

            // Act
            _mongoCache.CreateCoverIndex(background: false);

            // Assert
            var indexList = _mongoCache.GetCollection().Indexes.List().ToList();
            Assert.True(indexList.Count(x => x.Names.Count(s => s == "fullItemIndex") > 0) > 0);
            Assert.True(indexList.Count(x => x.Names.Count(s => s == "TTLItemIndex") > 0) > 0);
        }
    }
}
