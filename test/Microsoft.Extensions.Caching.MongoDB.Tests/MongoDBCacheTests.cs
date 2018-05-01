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
            var collection = _mongoCache.GetCollection();
            var keyNameValue = "InitTest" + Guid.NewGuid().ToString();

            // Act
            collection.InsertOne(new CacheItemModel()
            {
                Key = keyNameValue,
                Value = "Test MongoDBCache_InsertingOneCacheItem",
                ExpirationTimeUtcInTicks = System.DateTime.UtcNow.AddHours(1).Ticks
            });
            var result = collection.Count(f => f.Key == keyNameValue);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void MongoDBCache_GetsAndUpdatesTheSlidingExpirationInOneRequest()
        {
            // Arrange
            var collection = _mongoCache.GetCollection();
            var keyNameValue = "SlidingExpirationTest" + Guid.NewGuid().ToString();
            var referenceTimeStamp = System.DateTime.UtcNow;            

            //Act
            //SlidingExpiration +5 minutes and AbsoluteExpiration 1 hour
            collection.InsertOne(new CacheItemModel()
            {
                Key = keyNameValue,
                Value = "Test MongoDBCache_GetsAndUpdatesTheSlidingExpiration",
                SlidingTimeUtcTicks = referenceTimeStamp.AddMinutes(5).Ticks,
                ExpirationTimeUtcInTicks = referenceTimeStamp.AddHours(1).Ticks
            });

            var updateBuilder = new UpdateDefinitionBuilder<CacheItemModel>();
            var filterBuilder = new FilterDefinitionBuilder<CacheItemModel>();

            // By accessing the element the SlidingExpiration will be increased
            // in 5 additional minutes (total 10 compared with the original value) in one request
            var update = updateBuilder.Inc(
                x=> x.SlidingTimeUtcTicks,
                TimeSpan.FromMinutes(5).Ticks);
            var filter = filterBuilder.Eq(x => x.Key, keyNameValue);
            var updatedDocument = collection.FindOneAndUpdate(
                filter,
                update,
                new FindOneAndUpdateOptions<CacheItemModel>()
                {
                    //We get the document after the update
                    ReturnDocument = ReturnDocument.After
                });

            //Assert
            Assert.Equal(referenceTimeStamp.AddMinutes(10), DateTime.FromBinary(updatedDocument.SlidingTimeUtcTicks));
            Assert.Equal(referenceTimeStamp.AddHours(1), DateTime.FromBinary(updatedDocument.ExpirationTimeUtcInTicks));
        }
    }
}
