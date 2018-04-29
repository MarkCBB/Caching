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

            //Cleans the test database
            var db = _mongoCache.GetClient();
            db.DropDatabase(options.DatabaseName);
        }

        [Fact]
        public void MongoDBCache_InsertingOneCacheItem()
        {
            var collection = _mongoCache.GetCollection();
            var keyNameValue = "InitTest";

            // Act
            collection.InsertOne(new CaheItemModel()
            {
                Key = keyNameValue,
                Value = "Test Value"
            });
            var result = collection.Count(f => f.Key == keyNameValue);

            // Assert
            Assert.Equal(1, result);
        }
    }
}
