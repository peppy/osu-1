// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Database;
using Realms;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/BeatmapSets to the database backing
    /// </summary>
    public class BeatmapStore : MutableDatabaseBackedStoreWithFileIncludes<BeatmapSetInfo, BeatmapSetFileInfo>
    {
        public event Action<RealmWrapper<BeatmapInfo>> BeatmapHidden;
        public event Action<RealmWrapper<BeatmapInfo>> BeatmapRestored;

        public BeatmapStore(IDatabaseContextFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// Hide a <see cref="BeatmapInfo"/> in the database.
        /// </summary>
        /// <param name="beatmap">The beatmap to hide.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapInfo.Hidden"/> was changed.</returns>
        public bool Hide(BeatmapInfo beatmap)
        {
            using (ContextFactory.GetForWrite())
            {
                Refresh(ref beatmap, Beatmaps);

                if (beatmap.Hidden) return false;

                beatmap.Hidden = true;
            }

            BeatmapHidden?.Invoke(new RealmWrapper<BeatmapInfo>(beatmap, ContextFactory));
            return true;
        }

        /// <summary>
        /// Restore a previously hidden <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmap">The beatmap to restore.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapInfo.Hidden"/> was changed.</returns>
        public bool Restore(BeatmapInfo beatmap)
        {
            using (ContextFactory.GetForWrite())
            {
                Refresh(ref beatmap, Beatmaps);

                if (!beatmap.Hidden) return false;

                beatmap.Hidden = false;
            }

            BeatmapRestored?.Invoke(new RealmWrapper<BeatmapInfo>(beatmap, ContextFactory));
            return true;
        }

        protected override void Purge(List<BeatmapSetInfo> items, Realm context)
        {
            items.SelectMany(s => s.Beatmaps.Select(b => b.Metadata).Where(m => m != null)).ForEach(context.Remove);
            items.SelectMany(s => s.Beatmaps.Select(b => b.BaseDifficulty)).ForEach(context.Remove);
            items.SelectMany(s => s.Beatmaps).ForEach(context.Remove);
            items.Select(s => s.Metadata).ForEach(context.Remove);

            base.Purge(items, context);
        }

        public IQueryable<BeatmapInfo> Beatmaps => ContextFactory.Get().All<BeatmapInfo>();
    }
}
