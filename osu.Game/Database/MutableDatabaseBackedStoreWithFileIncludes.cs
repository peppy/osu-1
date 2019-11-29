// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.IO;
using Unosquare.Labs.LiteLib;

namespace osu.Game.Database
{
    public abstract class MutableDatabaseBackedStoreWithFileIncludes<T, U> : MutableDatabaseBackedStore<T>
        where T : class, IHasPrimaryKey, ISoftDelete, IHasFiles<U>, ILiteModel
        where U : class, INamedFileInfo
    {
        protected MutableDatabaseBackedStoreWithFileIncludes(IDatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }

        protected override IEnumerable<T> AddIncludesForConsumption(IEnumerable<T> query)
        {
            base.AddIncludesForConsumption(query);

            foreach (var item in query)
            {
                item.Files = ContextFactory.Get().Set<U>().Select($"{typeof(T).Name}ID = @ID", new { item.ID }).ToList();
                foreach (var file in item.Files)
                    file.FileInfo = ContextFactory.Get().Set<FileInfo>().Single(file.FileInfoID);
            }

            return query;
        }

        protected override IEnumerable<T> AddIncludesForDeletion(IEnumerable<T> query)
        {
            base.AddIncludesForConsumption(query);

            foreach (var item in query)
                item.Files = ContextFactory.Get().Set<U>().Select($"{typeof(T).Name}ID = @ID", new { item.ID }).ToList();

            return query;
        }
    }
}
