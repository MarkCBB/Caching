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
    public class MongoDBCacheTests
    {
        private MongoDBCache _mongoCache = null;

        public MongoDBCacheTests()
        {
            var options = new MongoDBCacheOptions()
            {
                ConnectionString = "mongodb://mongo1:27018,mongo1:27019,mongo1:27020",
                DatabaseName = "CachingTestsDB",
                CollectionName = "CachingEntriesTestCollection"
            };
            _mongoCache = new MongoDBCache(options);

            //Uncomment to clean the test database before to run the test cases.
            //var db = _mongoCache.GetClient();
            //db.DropDatabase(options.DatabaseName);
        }

        [Fact]
        public void MongoDBCache_InsertingOneCacheItem()
        {
            // Arrange
            var collection = MongoDBCacheExtensions.GetCollection(_mongoCache);
            var keyNameValue = "InsertingOneCacheItem" + Guid.NewGuid().ToString();
            var absolutExpirationTime = System.DateTime.UtcNow.AddHours(1);

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

        [Fact]
        public void MongoDBCache_DefaultSlidingTimeTicksIsZero()
        {
            // Arrange
            var collection = MongoDBCacheExtensions.GetCollection(_mongoCache);
            var keyNameValue = "DefaultSlidingTimeTicksIsZero" + Guid.NewGuid().ToString();
            var absolutExpirationTime = System.DateTime.UtcNow.AddHours(1);

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


        [Fact]
        public void MongoDBCache_NoValueInAbsoluteExpirationIsDateTimeMinValue()
        {
            // Arrange
            var collection = MongoDBCacheExtensions.GetCollection(_mongoCache);
            var keyNameValue = "NoValueInAbsoluteExpiration" + Guid.NewGuid().ToString();
            var absolutExpirationTime = System.DateTime.UtcNow.AddHours(1);

            // Act
            collection.InsertOne(new CacheItemModel()
            {
                Key = keyNameValue,
                Value = Encoding.ASCII.GetBytes("Test MongoDBCache_InsertingOneCacheItem"),                
                EffectiveExpirationTimeUtc = absolutExpirationTime
            });
            //Encoding.ASCII.GetString(
            var result = collection.Find(f => f.Key == keyNameValue).FirstOrDefault();

            // Assert
            Assert.Equal(DateTime.MinValue, result.AbsoluteExpirationTimeUtc);
        }        
    }
}
