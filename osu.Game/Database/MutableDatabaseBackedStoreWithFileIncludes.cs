// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using Realms;

namespace osu.Game.Database
{
    public abstract class MutableDatabaseBackedStoreWithFileIncludes<T, TFileInfo> : MutableDatabaseBackedStore<T>
        where T : RealmObject, IHasPrimaryKey, ISoftDelete, IHasFiles<TFileInfo>
        where TFileInfo : INamedFileInfo
    {
        protected MutableDatabaseBackedStoreWithFileIncludes(IDatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }
    }
}
