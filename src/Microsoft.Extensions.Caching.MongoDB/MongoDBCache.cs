// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Microsoft.Extensions.Caching.MongoDB
{
    public class MongoDBCache : IDisposable
    {
        private MongoDBCacheOptions _options;

        public MongoDBCache(IOptions<MongoDBCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _options = optionsAccessor.Value;
        }

        public MongoClient GetClient()
        {
            return new MongoClient(_options.ConnectionString);
        }

        public IMongoCollection<CaheItemModel> GetTestCollection()
        {
            return GetClient()
                .GetDatabase(_options.DatabaseName)
                .GetCollection<CaheItemModel>(_options.CollectionName);
        }

        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return null;
        }

        public void Dispose()
        {
            
        }
    }
}