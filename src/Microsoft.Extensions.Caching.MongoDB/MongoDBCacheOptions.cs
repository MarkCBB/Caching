// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
            DeleteOldValues = false;
            MongoClientSettings = null;
        }

        /// <summary>
        /// Connection string to the database. Not used if MongoClientSettings are provided.
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
        /// True if the process will send a query in every request to the database
        /// to delete all expired elements. Strongly recommended to keep as false (default)
        /// and use the built-in TTL index. Learn more: https://github.com/MarkCBB/Caching/wiki#usage
        /// </summary>
        public bool DeleteOldValues { get; set; }


        /// <summary>
        /// MongoClientSettings to establish a connection.
        /// If a value is provided in this property the ConnectionString is not used
        /// and all information regarding servers must be provided in this object.
        /// </summary>
        public MongoClientSettings MongoClientSettings { get; set; }


        MongoDBCacheOptions IOptions<MongoDBCacheOptions>.Value
        {
            get { return this; }
        }
    }
}