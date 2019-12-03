// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using AutoMapper;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Skinning;
using Realms;

namespace osu.Game.Database
{
    public class DatabaseContextFactory : IDatabaseContextFactory
    {
        private readonly Storage storage;
        private readonly Scheduler scheduler;

        private const string database_name = @"client";

        private ThreadLocal<Realm> threadContexts;

        private readonly object writeLock = new object();

        private bool currentWriteDidWrite;
        private bool currentWriteDidError;

        private int currentWriteUsages;

        private Transaction currentWriteTransaction;

        public DatabaseContextFactory(Storage storage, Scheduler scheduler)
        {
            this.storage = storage;
            this.scheduler = scheduler;
            recreateThreadContexts();
        }

        private static readonly GlobalStatistic<int> reads = GlobalStatistics.Get<int>("Database", "Get (Read)");
        private static readonly GlobalStatistic<int> writes = GlobalStatistics.Get<int>("Database", "Get (Write)");
        private static readonly GlobalStatistic<int> commits = GlobalStatistics.Get<int>("Database", "Commits");
        private static readonly GlobalStatistic<int> rollbacks = GlobalStatistics.Get<int>("Database", "Rollbacks");
        private static readonly GlobalStatistic<int> contexts = GlobalStatistics.Get<int>("Database", "Contexts");

        /// <summary>
        /// Get a context for the current thread for read-only usage.
        /// If a <see cref="DatabaseWriteUsage"/> is in progress, the existing write-safe context will be returned.
        /// </summary>
        public Realm Get()
        {
            threadContexts.Value.Refresh();

            reads.Value++;
            return threadContexts.Value;
        }

        /// <summary>
        /// Request a context for write usage. Can be consumed in a nested fashion (and will return the same underlying context).
        /// This method may block if a write is already active on a different thread.
        /// </summary>
        /// <param name="withTransaction">Whether to start a transaction for this write.</param>
        /// <returns>A usage containing a usable context.</returns>
        public DatabaseWriteUsage GetForWrite(bool withTransaction = true)
        {
            writes.Value++;
            Monitor.Enter(writeLock);
            Realm context;

            try
            {
                if (currentWriteTransaction == null && withTransaction)
                {
                    context = threadContexts.Value;
                    currentWriteTransaction = context.BeginWrite();
                }
                else
                {
                    // we want to try-catch the retrieval of the context because it could throw an error (in CreateContext).
                    context = threadContexts.Value;
                }
            }
            catch
            {
                // retrieval of a context could trigger a fatal error.
                Monitor.Exit(writeLock);
                throw;
            }

            Interlocked.Increment(ref currentWriteUsages);

            return new DatabaseWriteUsage(context, usageCompleted) { IsTransactionLeader = currentWriteTransaction != null && currentWriteUsages == 1 };
        }

        public void Schedule(Action action) => scheduler.Add(action);

        private void usageCompleted(DatabaseWriteUsage usage)
        {
            int usages = Interlocked.Decrement(ref currentWriteUsages);

            try
            {
                currentWriteDidWrite |= usage.PerformedWrite;
                currentWriteDidError |= usage.Errors.Any();

                if (usages == 0)
                {
                    if (currentWriteDidError)
                    {
                        rollbacks.Value++;
                        currentWriteTransaction?.Rollback();
                    }
                    else
                    {
                        commits.Value++;
                        currentWriteTransaction?.Commit();
                    }

                    if (currentWriteDidWrite || currentWriteDidError)
                    {
                        // explicitly dispose to ensure any outstanding flushes happen as soon as possible (and underlying resources are purged).
                        //usage.Context.Dispose();

                        // once all writes are complete, we want to refresh thread-specific contexts to make sure they don't have stale local caches.
                        //recycleThreadContexts();
                    }

                    currentWriteTransaction = null;
                    currentWriteDidWrite = false;
                    currentWriteDidError = false;
                }
            }
            finally
            {
                Monitor.Exit(writeLock);
            }
        }

        private void recreateThreadContexts()
        {
            // Contexts for other threads are not disposed as they may be in use elsewhere. Instead, fresh contexts are exposed
            // for other threads to use, and we rely on the finalizer inside OsuDbContext to handle their previous contexts
            threadContexts?.Value.Dispose();
            threadContexts = new ThreadLocal<Realm>(CreateContext, true);
        }

        protected virtual Realm CreateContext()
        {
            contexts.Value++;
            return Realm.GetInstance(new RealmConfiguration(storage.GetFullPath($"{database_name}.realm")));
        }

        public void ResetDatabase()
        {
            lock (writeLock)
            {
                recreateThreadContexts();
                storage.DeleteDatabase(database_name);
            }
        }
    }

    public class RealmWrapper<T> : IEquatable<RealmWrapper<T>>
        where T : RealmObject, IHasPrimaryKey
    {
        public string ID { get; }

        private readonly ThreadLocal<T> threadValues;

        public readonly IDatabaseContextFactory ContextFactory;

        public RealmWrapper(T original, IDatabaseContextFactory contextFactory)
        {
            ContextFactory = contextFactory;
            ID = original.ID;

            var originalContext = original.Realm;

            threadValues = new ThreadLocal<T>(() =>
            {
                var context = ContextFactory?.Get();

                if (context == null || originalContext?.IsSameInstance(context) != false)
                    return original;

                return (T)context.Find(typeof(T).Name, ID);
            });
        }

        public T Get() => threadValues.Value;

        public RealmWrapper<TChild> WrapChild<TChild>(Func<T, TChild> lookup)
            where TChild : RealmObject, IHasPrimaryKey => new RealmWrapper<TChild>(lookup(Get()), ContextFactory);

        public static implicit operator T(RealmWrapper<T> wrapper)
            => wrapper?.Get().Detach();

        public bool Equals(RealmWrapper<T> other) => other != null && other.ID == ID;
    }

    public static class RealmExtensions
    {
        private static readonly IMapper mapper = new MapperConfiguration(c =>
        {
            c.CreateMap<BeatmapDifficulty, BeatmapDifficulty>();
            c.CreateMap<BeatmapInfo, BeatmapInfo>();
            c.CreateMap<BeatmapMetadata, BeatmapMetadata>();
            c.CreateMap<BeatmapSetFileInfo, BeatmapSetFileInfo>();

            c.CreateMap<BeatmapSetInfo, BeatmapSetInfo>()
             .ForMember(s => s.Beatmaps, d => d.MapFrom(s => s.Beatmaps))
             .ForMember(s => s.Files, d => d.MapFrom(s => s.Files))
             .MaxDepth(2);

            c.CreateMap<DatabasedKeyBinding, DatabasedKeyBinding>();
            c.CreateMap<DatabasedSetting, DatabasedSetting>();
            c.CreateMap<FileInfo, FileInfo>();
            c.CreateMap<ScoreFileInfo, ScoreFileInfo>();
            c.CreateMap<SkinInfo, SkinInfo>();
            c.CreateMap<RulesetInfo, RulesetInfo>();
        }).CreateMapper();

        public static T Detach<T>(this T obj) where T : RealmObject
        {
            if (!obj.IsManaged)
                return obj;

            var detached = mapper.Map<T>(obj);

            //typeof(RealmObject).GetField("_realm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(detached, null);

            return detached;
        }

        public static RealmWrapper<T> Wrap<T>(this T obj, IDatabaseContextFactory dbFactory)
            where T : RealmObject, IHasPrimaryKey => new RealmWrapper<T>(obj, dbFactory);

        public static RealmWrapper<T> WrapAsUnmanaged<T>(this T obj)
            where T : RealmObject, IHasPrimaryKey => new RealmWrapper<T>(obj, null);
    }
}
