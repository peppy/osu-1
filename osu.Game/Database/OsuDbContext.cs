// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Dapper;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Skinning;
using Unosquare.Labs.LiteLib;

namespace osu.Game.Database
{
    public class OsuDbContext : LiteDbContext
    {
        static OsuDbContext()
        {
            // required to initialise native SQLite libraries on some platforms.
            SQLitePCL.Batteries_V2.Init();

            // https://github.com/aspnet/EntityFrameworkCore/issues/9994#issuecomment-508588678
            SQLitePCL.raw.sqlite3_config(2 /*SQLITE_CONFIG_MULTITHREAD*/);

            SqlMapper.RemoveTypeMap(typeof(DateTimeOffset));
            SqlMapper.AddTypeHandler(DateTimeHandler.DEFAULT);

            SqlMapper.RemoveTypeMap(typeof(long?));
            SqlMapper.AddTypeHandler(NullableLongHandler.DEFAULT);

            SqlMapper.RemoveTypeMap(typeof(int?));
            SqlMapper.AddTypeHandler(NullableIntHandler.DEFAULT);
        }

        public OsuDbContext(string filename)
            : base(filename, true)
        {
            Logger.Log($"Found {BeatmapInfo.Count()} beatmaps");
        }

        public LiteDbSet<BeatmapInfo> BeatmapInfo { get; set; }
        public LiteDbSet<BeatmapDifficulty> BeatmapDifficulty { get; set; }
        public LiteDbSet<BeatmapMetadata> BeatmapMetadata { get; set; }
        public LiteDbSet<BeatmapSetInfo> BeatmapSetInfo { get; set; }
        public LiteDbSet<BeatmapSetFileInfo> BeatmapSetFileInfo { get; set; }
        public LiteDbSet<DatabasedKeyBinding> DatabasedKeyBinding { get; set; }
        public LiteDbSet<DatabasedSetting> DatabasedSetting { get; set; }
        public LiteDbSet<FileInfo> FileInfo { get; set; }
        public LiteDbSet<RulesetInfo> RulesetInfo { get; set; }
        public LiteDbSet<SkinInfo> SkinInfo { get; set; }
        public LiteDbSet<SkinFileInfo> SkinFileInfo { get; set; }
        public LiteDbSet<ScoreInfo> ScoreInfo { get; set; }
        public LiteDbSet<ScoreFileInfo> ScoreFileInfo { get; set; }

        public IDbTransaction BeginTransaction() => Connection.BeginTransaction();

        public void Migrate()
        {
        }
    }

    public class DatabaseContextFactory : IDatabaseContextFactory
    {
        private readonly Storage storage;

        private const string database_name = @"client";

        private ThreadLocal<OsuDbContext> threadContexts;

        private readonly object writeLock = new object();

        private bool currentWriteDidWrite;
        private bool currentWriteDidError;

        private int currentWriteUsages;

        private IDbTransaction currentWriteTransaction;

        public DatabaseContextFactory(Storage storage)
        {
            this.storage = storage;
            recycleThreadContexts();
        }

        private static readonly GlobalStatistic<int> reads = GlobalStatistics.Get<int>("Database", "Get (Read)");
        private static readonly GlobalStatistic<int> writes = GlobalStatistics.Get<int>("Database", "Get (Write)");
        private static readonly GlobalStatistic<int> commits = GlobalStatistics.Get<int>("Database", "Commits");
        private static readonly GlobalStatistic<int> rollbacks = GlobalStatistics.Get<int>("Database", "Rollbacks");

        /// <summary>
        /// Get a context for the current thread for read-only usage.
        /// If a <see cref="osu.Game.Database.DatabaseWriteUsage"/> is in progress, the existing write-safe context will be returned.
        /// </summary>
        public OsuDbContext Get()
        {
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
            OsuDbContext context;

            try
            {
                if (currentWriteTransaction == null && withTransaction)
                {
                    // this mitigates the fact that changes on tracked entities will not be rolled back with the transaction by ensuring write operations are always executed in isolated contexts.
                    // if this results in sub-optimal efficiency, we may need to look into removing Database-level transactions in favour of running SaveChanges where we currently commit the transaction.
                    if (threadContexts.IsValueCreated)
                        recycleThreadContexts();

                    context = threadContexts.Value;
                    currentWriteTransaction = context.BeginTransaction();
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
                        usage.Context.Dispose();

                        // once all writes are complete, we want to refresh thread-specific contexts to make sure they don't have stale local caches.
                        recycleThreadContexts();
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

        private void recycleThreadContexts()
        {
            // Contexts for other threads are not disposed as they may be in use elsewhere. Instead, fresh contexts are exposed
            // for other threads to use, and we rely on the finalizer inside OsuDbContextNew to handle their previous contexts
            threadContexts?.Value.Dispose();
            threadContexts = new ThreadLocal<OsuDbContext>(CreateContext, true);
        }

        protected virtual OsuDbContext CreateContext() => new OsuDbContext(storage.GetFullPath($@"{database_name}.db", true));

        public void ResetDatabase()
        {
            lock (writeLock)
            {
                recycleThreadContexts();
                storage.DeleteDatabase(database_name);
            }
        }
    }

    public interface IDatabaseContextFactory
    {
        /// <summary>
        /// Get a context for read-only usage.
        /// </summary>
        OsuDbContext Get();

        /// <summary>
        /// Request a context for write usage. Can be consumed in a nested fashion (and will return the same underlying context).
        /// This method may block if a write is already active on a different thread.
        /// </summary>
        /// <param name="withTransaction">Whether to start a transaction for this write.</param>
        /// <returns>A usage containing a usable context.</returns>
        DatabaseWriteUsage GetForWrite(bool withTransaction = true);
    }

    public class DatabaseWriteUsage : IDisposable
    {
        public readonly OsuDbContext Context;
        private readonly Action<DatabaseWriteUsage> usageCompleted;

        public DatabaseWriteUsage(OsuDbContext context, Action<DatabaseWriteUsage> onCompleted)
        {
            Context = context;
            usageCompleted = onCompleted;
        }

        public bool PerformedWrite { get; private set; }

        private bool isDisposed;
        public List<Exception> Errors = new List<Exception>();

        /// <summary>
        /// Whether this write usage will commit a transaction on completion.
        /// If false, there is a parent usage responsible for transaction commit.
        /// </summary>
        public bool IsTransactionLeader;

        protected void Dispose(bool disposing)
        {
            if (isDisposed) return;

            isDisposed = true;

            try
            {
                PerformedWrite = true; // todo: shouldn't always be true?
            }
            catch (Exception e)
            {
                Errors.Add(e);
                throw;
            }
            finally
            {
                usageCompleted?.Invoke(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DatabaseWriteUsage()
        {
            Dispose(false);
        }
    }

    public class NullableLongHandler : SqlMapper.TypeHandler<long?>
    {
        protected NullableLongHandler() { }
        public static readonly NullableLongHandler DEFAULT = new NullableLongHandler();

        public override void SetValue(IDbDataParameter parameter, long? value)
        {
            if (value.HasValue)
            {
                parameter.Value = value.Value;
            }
            else
            {
                parameter.Value = DBNull.Value;
            }
        }

        public override long? Parse(object value)
        {
            if (value == null || value is DBNull) return null;

            return Convert.ToInt64(value);
        }
    }

    public class NullableIntHandler : SqlMapper.TypeHandler<long?>
    {
        protected NullableIntHandler() { }
        public static readonly NullableIntHandler DEFAULT = new NullableIntHandler();

        public override void SetValue(IDbDataParameter parameter, long? value)
        {
            if (value.HasValue)
            {
                parameter.Value = value.Value;
            }
            else
            {
                parameter.Value = DBNull.Value;
            }
        }

        public override long? Parse(object value)
        {
            if (value == null || value is DBNull) return null;

            return Convert.ToInt32(value);
        }
    }

    public class DateTimeHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        private readonly TimeZoneInfo databaseTimeZone = TimeZoneInfo.Local;
        public static readonly DateTimeHandler DEFAULT = new DateTimeHandler();

        public DateTimeHandler()
        {
        }

        public override DateTimeOffset Parse(object value)
        {
            switch (value)
            {
                case string str:
                    return DateTimeOffset.Parse(str);
            }

            return DateTimeOffset.MinValue;
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            DateTime paramVal = value.ToOffset(this.databaseTimeZone.BaseUtcOffset).DateTime;
            parameter.Value = paramVal;
        }
    }
}
