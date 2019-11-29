// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Database;
using osu.Game.Rulesets;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/BeatmapSets to the database backing
    /// </summary>
    public class BeatmapStore : MutableDatabaseBackedStoreWithFileIncludes<BeatmapSetInfo, BeatmapSetFileInfo>
    {
        public event Action<BeatmapInfo> BeatmapHidden;
        public event Action<BeatmapInfo> BeatmapRestored;

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

            BeatmapHidden?.Invoke(beatmap);
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

            BeatmapRestored?.Invoke(beatmap);
            return true;
        }

        protected override IEnumerable<BeatmapSetInfo> AddIncludesForDeletion(IEnumerable<BeatmapSetInfo> query)
        {
            base.AddIncludesForDeletion(query);

            foreach (var item in query)
            {
                item.Metadata = ContextFactory.Get().Set<BeatmapMetadata>().Single(item.MetadataID);
                item.Beatmaps = ContextFactory.Get().Set<BeatmapInfo>().Select("BeatmapSetInfoID = @ID", new { item.ID }).ToList();

                foreach (var beatmap in item.Beatmaps)
                {
                    beatmap.BaseDifficulty = ContextFactory.Get().Set<BeatmapDifficulty>().Single(beatmap.BaseDifficultyID);
                    if (beatmap.MetadataID != null)
                        beatmap.Metadata = ContextFactory.Get().Set<BeatmapMetadata>().Single(beatmap.MetadataID.Value);
                }
            }

            return query;
        }

        protected override IEnumerable<BeatmapSetInfo> AddIncludesForConsumption(IEnumerable<BeatmapSetInfo> query)
        {
            base.AddIncludesForConsumption(query);

            foreach (var item in query)
            {
                item.Metadata = ContextFactory.Get().Set<BeatmapMetadata>().Single(item.MetadataID);
                item.Beatmaps = ContextFactory.Get().Set<BeatmapInfo>().Select("BeatmapSetInfoID = @ID", new { item.ID }).ToList();

                foreach (var beatmap in item.Beatmaps)
                {
                    beatmap.BaseDifficulty = ContextFactory.Get().Set<BeatmapDifficulty>().Single(beatmap.BaseDifficultyID);
                    beatmap.Ruleset = ContextFactory.Get().Set<RulesetInfo>().Single(beatmap.RulesetID);
                    if (beatmap.MetadataID != null)
                        beatmap.Metadata = ContextFactory.Get().Set<BeatmapMetadata>().Single(beatmap.MetadataID.Value);
                }
            }

            return query;
        }

        protected override void Purge(List<BeatmapSetInfo> items, OsuDbContext context)
        {
            // metadata is M-N so we can't rely on cascades

            //todo: reimplement
            // context.BeatmapMetadata.RemoveRange(items.Select(s => s.Metadata));
            // context.BeatmapMetadata.RemoveRange(items.SelectMany(s => s.Beatmaps.Select(b => b.Metadata).Where(m => m != null)));
            //
            // // todo: we can probably make cascades work here with a FK in BeatmapDifficulty. just make to make it work correctly.
            // context.BeatmapDifficulty.RemoveRange(items.SelectMany(s => s.Beatmaps.Select(b => b.BaseDifficulty)));

            base.Purge(items, context);
        }

        public IEnumerable<BeatmapInfo> Beatmaps
        {
            get
            {
                var beatmaps = ContextFactory.Get().BeatmapInfo.SelectAll();

                foreach (var beatmap in beatmaps)
                {
                    beatmap.BaseDifficulty = ContextFactory.Get().Set<BeatmapDifficulty>().Single(beatmap.BaseDifficultyID);
                    beatmap.Ruleset = ContextFactory.Get().Set<RulesetInfo>().Single(beatmap.RulesetID);
                    if (beatmap.MetadataID != null)
                        beatmap.Metadata = ContextFactory.Get().Set<BeatmapMetadata>().Single(beatmap.MetadataID.Value);

                    beatmap.BeatmapSet = ContextFactory.Get().Set<BeatmapSetInfo>().Single(beatmap.BeatmapSetInfoID);

                    // todo: more efficient.
                    AddIncludesForConsumption(new[] { beatmap.BeatmapSet });
                }

                return beatmaps;
            }
        }
    }
}
