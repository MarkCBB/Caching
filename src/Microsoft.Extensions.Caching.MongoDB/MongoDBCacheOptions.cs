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

        public bool CreateKeyIndex { get; set; }

        public bool CreateTTLIndex { get; set; }

        MongoDBCacheOptions IOptions<MongoDBCacheOptions>.Value
        {
            get { return this; }
        }
    }
}