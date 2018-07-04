// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.MongoDB;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up MongoDB distributed cache related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class MongoDBCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedMongoDBCache(this IServiceCollection services, Action<MongoDBCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, MongoDBCache>());

            return services;
        }
    }
}
