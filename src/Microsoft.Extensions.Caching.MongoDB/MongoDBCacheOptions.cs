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

        MongoDBCacheOptions IOptions<MongoDBCacheOptions>.Value
        {
            get { return this; }
        }
    }
}