// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.MongoDB
{
    /// <summary>
    /// Configuration options for <see cref="MongoDBCache"/>.
    /// </summary>
    public class MongoDBCacheOptions : IOptions<MongoDBCacheOptions>
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string CollectionName { get; set; }

        public int MaxRetries { get; set; }

        public int TimeToWait { get; set; }

        public bool CreateKeyAndValueIndex { get; set; }

        public bool CreateSlidingTimeUtcIndex { get; set; }

        public bool CreateExpirationTimeUtcIndex { get; set; }

        MongoDBCacheOptions IOptions<MongoDBCacheOptions>.Value
        {
            get { return this; }
        }
    }
}