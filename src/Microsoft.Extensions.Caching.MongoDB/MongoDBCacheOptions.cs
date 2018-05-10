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
        public MongoDBCacheOptions()
        {
            ConnectionString = "";
            DatabaseName = "NetCoreCache";
            CollectionName = "CacheItems";
            MaxRetries = 240;
            MillisToWait = 500;
            CreateCoverIndex = true;
            CreateTTLIndex = true;
        }

        /// <summary>
        /// Connection string to the database
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Database Name
        /// Default NetCoreCache
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Collection Name default CacheItems
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// Max retries in case of cluster exception
        /// Default 240
        /// This default value combined with the millis to wait 500 default
        /// means two minutes with one retry every half second.
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// Millis to wait in every retry in case of retry
        /// Default 500
        /// This default value combined with the MaxRetries to wait 240 default
        /// means two minutes with one retry every half second.
        /// </summary>
        public int MillisToWait { get; set; }

        /// <summary>
        /// True to creates an index to ensure that the queries are index covered
        /// making the queries faster
        /// Recommended for scenarios with more reads than writes.
        /// </summary>
        public bool CreateCoverIndex { get; set; }

        /// <summary>
        /// True to create a TTL index that will delete the old entries
        /// in a background process in the servers.
        /// Default value true (strongly recommended).
        /// </summary>
        public bool CreateTTLIndex { get; set; }

        MongoDBCacheOptions IOptions<MongoDBCacheOptions>.Value
        {
            get { return this; }
        }
    }
}