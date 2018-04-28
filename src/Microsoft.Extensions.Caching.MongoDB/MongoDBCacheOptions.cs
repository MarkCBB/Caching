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
        /// <summary>
        /// The configuration used to connect to MongoDB.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// The MongoDB instance name.
        /// </summary>
        public string InstanceName { get; set; }

        MongoDBCacheOptions IOptions<MongoDBCacheOptions>.Value
        {
            get { return this; }
        }
    }
}