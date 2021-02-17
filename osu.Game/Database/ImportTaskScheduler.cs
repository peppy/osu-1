// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Database
{
    public class ImportTaskScheduler : IDisposable
    {
        private readonly BlockingCollection<Task> normalTasks = new BlockingCollection<Task>();

        private readonly BlockingCollection<Task> lowPriorityTasks = new BlockingCollection<Task>();

        private readonly BlockingCollection<Task>[] collections;

        private readonly ImmutableArray<Thread> threads;

        /// <summary>
        /// Initializes a new instance of an ImportTaskScheduler with the specified concurrency level.
        /// </summary>
        /// <param name="numberOfThreads">The number of threads that should be created and used by this scheduler.</param>
        /// <param name="name">The thread name to give threads in this pool.</param>
        public ImportTaskScheduler(int numberOfThreads, string name)
        {
            collections = new[] { normalTasks, lowPriorityTasks };

            if (numberOfThreads < 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfThreads));

            threads = Enumerable.Range(0, numberOfThreads).Select(i =>
            {
                var thread = new Thread(processTasks)
                {
                    Name = $"{nameof(ImportTaskScheduler)} ({name})",
                    IsBackground = true
                };

                thread.Start();

                return thread;
            }).ToImmutableArray();
        }

        /// <summary>
        /// Continually get the next task and try to execute it.
        /// This will continue as a blocking operation until the scheduler is disposed and no more tasks remain.
        /// </summary>
        private void processTasks()
        {
            try
            {
                while (true)
                {
                    BlockingCollection<Task>.TakeFromAny(collections, out var t);
                    t.RunSynchronously();
                }
            }
            catch (ObjectDisposedException)
            {
                // tasks may have been disposed. there's no easy way to check on this other than catch for it.
            }
        }

        /// <summary>
        /// Cleans up the scheduler by indicating that no more tasks will be queued.
        /// This method blocks until all threads successfully shutdown.
        /// </summary>
        public void Dispose()
        {
            normalTasks.CompleteAdding();

            foreach (var thread in threads)
                thread.Join(TimeSpan.FromSeconds(10));

            normalTasks.Dispose();
        }

        /// <summary>
        /// Queues a Task to be executed by this scheduler.
        /// </summary>
        public Task<TModel> Queue<TModel>(Func<Task<TModel>> func,  bool lowPriority)
        {
            Task<TModel> function() => func();

            var wrappedTask = new Task<TModel>(function);

            var queue = lowPriority ? lowPriorityTasks : normalTasks;

            queue.Add(wrappedTask, cancellationToken);

            return wrappedTask;
        }
    }
}
