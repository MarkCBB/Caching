using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Extensions.Caching.MongoDB.Tests
{
    public class MongoDBCacheCreateIndexInDbTests : BaseMongoDbTests
    {   
        // Check that the index is created if specified in the parameters
        // And also check that no index is created if no specified.
        // Create a blank database to do it and avoid colision with 
        // other databases.
        [Fact(Skip = SkipReason)]
        public void MongoDBCache_CreateAllIndexsDetailedInParameters()
        {
            // Arrange
            var databaseName = "CreateAllIndexs_" + GetRandomNumber();
            var mongoOptions = new MongoDBCacheOptions()
            {
                ConnectionString = "mongodb://mongo1:27018,mongo1:27019,mongo1:27020",
                DatabaseName = databaseName,
                CollectionName = "CacheCollectionTests",
                CreateCoverIndex = true,
                CreateTTLIndex = true,
                CreateIndexesInBackground = false
            };
            // We must create a new mongoDB cache in every test becasue this parameters and
            // the index creation are set only once
            var cache = new MongoDBCache(mongoOptions);
            var key = GetRandomNumber();
            var value = Encoding.ASCII.GetBytes("MongoDBCache_CreateIndexIfIsDetailedInParameters");
            var cacheOptions = new Distributed.DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.UtcNow
            };

            // Act
            cache.Set(key, value, cacheOptions);

            // Assert
            var indexList = cache.GetCollection().Indexes.List().ToList();
            Assert.True(ExistsIndexInListHelper(indexList, "fullItemIndex"));
            Assert.True(ExistsIndexInListHelper(indexList, "TTLItemIndex"));
        }

        [Fact(Skip = SkipReason)]
        public void MongoDBCache_CreateOnlyTTLIndexDetailedInParameters()
        {
            // Arrange
            var databaseName = "CreateOnlyTTLIndex_" + GetRandomNumber();
            var mongoOptions = new MongoDBCacheOptions()
            {
                ConnectionString = "mongodb://mongo1:27018,mongo1:27019,mongo1:27020",
                DatabaseName = databaseName,
                CollectionName = "CacheCollectionTests",
                CreateCoverIndex = false,
                CreateTTLIndex = true,
                CreateIndexesInBackground = false
            };
            // We must create a new mongoDB cache in every test becasue this parameters and
            // the index creation are set only once
            var cache = new MongoDBCache(mongoOptions);
            var key = GetRandomNumber();
            var value = Encoding.ASCII.GetBytes("MongoDBCache_CreateIndexIfIsDetailedInParameters");
            var cacheOptions = new Distributed.DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.UtcNow
            };

            // Act
            cache.Set(key, value, cacheOptions);

            // Assert
            var indexList = cache.GetCollection().Indexes.List().ToList();
            Assert.False(ExistsIndexInListHelper(indexList, "fullItemIndex"));
            Assert.True(ExistsIndexInListHelper(indexList, "TTLItemIndex"));
        }

        [Fact(Skip = SkipReason)]
        public void MongoDBCache_DoNotCreateAnyIndexDetailedInParameters()
        {
            // Arrange
            var databaseName = "DoNotCreateAnyIndex_" + GetRandomNumber();
            var mongoOptions = new MongoDBCacheOptions()
            {
                ConnectionString = "mongodb://mongo1:27018,mongo1:27019,mongo1:27020",
                DatabaseName = databaseName,
                CollectionName = "CacheCollectionTests",
                CreateCoverIndex = false,
                CreateTTLIndex = false,
                CreateIndexesInBackground = false
            };
            // We must create a new mongoDB cache in every test becasue this parameters and
            // the index creation are set only once
            var cache = new MongoDBCache(mongoOptions);
            var key = GetRandomNumber();
            var value = Encoding.ASCII.GetBytes("MongoDBCache_CreateIndexIfIsDetailedInParameters");
            var cacheOptions = new Distributed.DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.UtcNow
            };

            // Act
            cache.Set(key, value, cacheOptions);

            // Assert
            var indexList = cache.GetCollection().Indexes.List().ToList();
            Assert.False(ExistsIndexInListHelper(indexList, "fullItemIndex"));
            Assert.False(ExistsIndexInListHelper(indexList, "TTLItemIndex"));
        }
    }
}
