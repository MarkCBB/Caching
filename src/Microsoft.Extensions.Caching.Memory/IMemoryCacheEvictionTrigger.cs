﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.Caching.Memory
{
    public interface IMemoryCacheEvictionTrigger : IDisposable
    {
        // TODO: doc comments
        Func<bool> EvictionCallback { get; set; }

        void Resume();

        void Stop();
    }
}