// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmap : CarouselItem
    {
        public readonly RealmWrapper<BeatmapInfo> Beatmap;

        public CarouselBeatmap(RealmWrapper<BeatmapInfo> beatmap)
        {
            Beatmap = beatmap;
            State.Value = CarouselItemState.Collapsed;
        }

        protected override DrawableCarouselItem CreateDrawableRepresentation() => new DrawableCarouselBeatmap(this);

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            var beatmap = Beatmap.Get();

            bool match =
                criteria.Ruleset == null ||
                beatmap.Ruleset.OnlineID == criteria.Ruleset.OnlineID ||
                (beatmap.Ruleset.OnlineID == 0 && criteria.Ruleset.OnlineID > 0 && criteria.AllowConvertedBeatmaps);

            match &= !criteria.StarDifficulty.HasFilter || criteria.StarDifficulty.IsInRange(beatmap.StarDifficulty);
            match &= !criteria.ApproachRate.HasFilter || criteria.ApproachRate.IsInRange(beatmap.BaseDifficulty.ApproachRate);
            match &= !criteria.DrainRate.HasFilter || criteria.DrainRate.IsInRange(beatmap.BaseDifficulty.DrainRate);
            match &= !criteria.CircleSize.HasFilter || criteria.CircleSize.IsInRange(beatmap.BaseDifficulty.CircleSize);
            match &= !criteria.Length.HasFilter || criteria.Length.IsInRange(beatmap.Length);
            match &= !criteria.BPM.HasFilter || criteria.BPM.IsInRange(beatmap.BPM);

            match &= !criteria.BeatDivisor.HasFilter || criteria.BeatDivisor.IsInRange(beatmap.BeatDivisor);
            match &= !criteria.OnlineStatus.HasFilter || criteria.OnlineStatus.IsInRange(beatmap.Status);

            match &= !criteria.Creator.HasFilter || criteria.Creator.Matches(beatmap.Metadata.AuthorString);
            match &= !criteria.Artist.HasFilter || criteria.Artist.Matches(beatmap.Metadata.Artist) ||
                     criteria.Artist.Matches(beatmap.Metadata.ArtistUnicode);

            if (match)
            {
                var terms = new List<string>();

                terms.AddRange(beatmap.Metadata.SearchableTerms);
                terms.Add(beatmap.Version);

                foreach (var criteriaTerm in criteria.SearchTerms)
                {
                    match &= terms.Any(term => term.IndexOf(criteriaTerm, StringComparison.InvariantCultureIgnoreCase) >= 0);
                }
            }

            Filtered.Value = !match;
        }

        public override int CompareTo(FilterCriteria criteria, CarouselItem other)
        {
            if (!(other is CarouselBeatmap otherBeatmap))
                return base.CompareTo(criteria, other);

            switch (criteria.Sort)
            {
                default:
                case SortMode.Difficulty:
                    var ruleset = Beatmap.Get().Ruleset.OnlineID.CompareTo(otherBeatmap.Beatmap.Get().Ruleset.OnlineID);
                    if (ruleset != 0) return ruleset;

                    return Beatmap.Get().StarDifficulty.CompareTo(otherBeatmap.Beatmap.Get().StarDifficulty);
            }
        }

        public override string ToString() => Beatmap.ToString();
    }
}
