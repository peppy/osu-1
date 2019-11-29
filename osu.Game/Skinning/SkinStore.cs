// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class SkinStore : MutableDatabaseBackedStoreWithFileIncludes<SkinInfo, SkinFileInfo>
    {
        public SkinStore(DatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }

        protected override IEnumerable<SkinInfo> AddIncludesForDeletion(IEnumerable<SkinInfo> query)
        {
            base.AddIncludesForConsumption(query);

            foreach (var item in query)
            {
                item.Settings = ContextFactory.Get().Set<DatabasedSetting>().Select("SkinInfoID = @ID", new { item.ID }).ToList();
            }

            return query;
        }
    }
}
