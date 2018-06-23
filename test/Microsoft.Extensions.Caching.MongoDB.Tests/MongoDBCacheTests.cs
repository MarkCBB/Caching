// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using MongoDB.Driver;
using System.Text;
using MongoDB.Bson;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class MongoDBCacheTests : BaseMongoDbTests
    {
        private static bool ExistsIndexInListHelper(List<BsonDocument> indexes, string indexName)
        {
            foreach (var item in indexes)
            {
                if (item["name"] == indexName)
                {
                    return true;
                }
            }
            return false;
        }

        [Fact(Skip = SkipReason)]
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
                EffectiveExpirationTimeUtc = absolutExpirationTime
            });
            var result = collection.Count(f => f.Key == keyNameValue);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact(Skip = SkipReason)]
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
                EffectiveExpirationTimeUtc = absolutExpirationTime
            });
            //Encoding.ASCII.GetString(
            var result = collection.Find(f => f.Key == keyNameValue).FirstOrDefault();

            // Assert
            Assert.Equal(0, result.SlidingTimeTicks);
        }


        [Fact(Skip = SkipReason)]
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
                EffectiveExpirationTimeUtc = effectiveExpirationTimeUtc
            });
            var result = collection.Find(f => f.Key == keyNameValue).FirstOrDefault();

            // Assert
            Assert.Equal(DateTimeOffset.MinValue, result.AbsoluteExpirationTimeUtc);
        }

        [Fact(Skip = SkipReason)]
        public void MongoDBCache_A_Full_Cover_Index_Is_Created()
        {
            // Arrange
            var collection = MongoDBCacheExtensions.GetCollection(_mongoCache);

            // Act
            _mongoCache.CreateIndexes(background: false);

            // Assert
            var indexList = _mongoCache.GetCollection().Indexes.List().ToList();
            Assert.True(ExistsIndexInListHelper(indexList, "fullItemIndex"));
            Assert.True(ExistsIndexInListHelper(indexList, "TTLItemIndex"));
        }
    }
}
