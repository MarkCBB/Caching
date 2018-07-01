using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Extensions.Caching.MongoDB.Tests
{
    public class MongoDBCacheCreateIndexInDbTests : BaseMongoDbTests
    {
        // Checks that the index is created if specified in the parameters
        // and also checks that no index is created if no specified.
        // Creates a blank database to do it avoiding colision with 
        // other databases.
        [Trait("Category", "Indexes1")]
        [Fact(Skip = SkipReason)]
        public void MongoDBCache_CreateAllIndexesDetailedInParameters()
        {
            // Arrange
            var databaseName = "CreateAllIndexes_" + GetRandomNumber();
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

        [Trait("Category", "Indexes2")]
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

        [Trait("Category", "Indexes3")]
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
