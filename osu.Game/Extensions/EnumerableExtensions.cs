// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Extensions
{
    public static class EnumerableExtensions
    {
        // sourced from https://stackoverflow.com/a/37569259
        public static Task ForEachAsync<TIn>(this IEnumerable<TIn> enumerable, Func<TIn, Task> asyncProcessor, int? maxDegreeOfParallelism = null)
        {
            int maxAsyncThreadCount = maxDegreeOfParallelism ?? Environment.ProcessorCount;

            // should be safe to not dispose as the unmanaged components have finalizers.
            SemaphoreSlim throttler = new SemaphoreSlim(maxAsyncThreadCount, maxAsyncThreadCount);

            IEnumerable<Task> tasks = enumerable.Select(async input =>
            {
                await throttler.WaitAsync().ConfigureAwait(false);

                try
                {
                    await asyncProcessor(input).ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            });

            return Task.WhenAll(tasks);
        }
    }
}
