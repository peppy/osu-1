// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;

namespace osu.Game.Scoring
{
    public class ScoreStore : MutableDatabaseBackedStoreWithFileIncludes<ScoreInfo, ScoreFileInfo>
    {
        public ScoreStore(IDatabaseContextFactory factory, Storage storage)
            : base(factory, storage)
        {
        }

        protected override IEnumerable<ScoreInfo> AddIncludesForConsumption(IEnumerable<ScoreInfo> query)
        {
            base.AddIncludesForConsumption(query);

            foreach (var item in query)
            {
                item.Beatmap = ContextFactory.Get().Set<BeatmapInfo>().Single(item.BeatmapInfoID);
                item.Ruleset = ContextFactory.Get().Set<RulesetInfo>().Single(item.RulesetID);
            }

            return query;
        }
    }
}
