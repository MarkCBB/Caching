// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class MongoDbCacheItemModelTests : BaseMongoDbTests
    {
        [Fact]
        public void MongoDBCache_ArgumentsWithNoValidValuesShouldReturnAnException()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = null,
                AbsoluteExpirationRelativeToNow = null,
                SlidingExpiration = null
            };
            var exceptionThrown = false;

            // Act
            try
            {
                _mongoCache.Set("someKey", new byte[0], options);
            }
            catch
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.True(exceptionThrown);
        }

        [Fact]
        public void MongoDBCache_AbsoluteExpiration_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = utcNow.AddMinutes(1),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            // Act
            var item = MongoDBCache.CreateNewItem("someKey", new byte[0], options, utcNow);

            // Assert
            Assert.Equal(options.AbsoluteExpiration, item.EffectiveExpirationTimeUtc);
        }

        [Fact]
        public void MongoDBCache_AbsoluteExpiration_Is_After_Sliding_So_Effective_Should_Be_Sliding()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = utcNow.AddMinutes(2),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };

            // Act
            var item = MongoDBCache.CreateNewItem("someKey", new byte[0], options, utcNow);

            // Assert
            Assert.Equal(utcNow + options.SlidingExpiration.Value, item.EffectiveExpirationTimeUtc);
        }

        [Fact]
        public void MongoDBCache_AbsoluteExpirationRelativeToNow_Is_Before_Sliding_So_Effective_Should_Be_AbsoluteExpiration()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            // Act
            var item = MongoDBCache.CreateNewItem("someKey", new byte[0], options, utcNow);

            // Assert
            Assert.Equal(utcNow + options.AbsoluteExpirationRelativeToNow, item.EffectiveExpirationTimeUtc);
        }

        [Fact]
        public void MongoDBCache_AbsoluteExpirationRelativeToNow_Is_After_Sliding_So_Effective_Should_Be_Sliding()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };

            // Act
            var item = MongoDBCache.CreateNewItem("someKey", new byte[0], options, utcNow);

            // Assert
            Assert.Equal(utcNow + options.SlidingExpiration, item.EffectiveExpirationTimeUtc);
        }
    }
}
