// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
                beatmap.RulesetID == criteria.Ruleset.ID ||
                (beatmap.RulesetID == 0 && criteria.Ruleset.ID > 0 && criteria.AllowConvertedBeatmaps);

            match &= criteria.StarDifficulty.IsInRange(beatmap.StarDifficulty);
            match &= criteria.ApproachRate.IsInRange(beatmap.BaseDifficulty.ApproachRate);
            match &= criteria.DrainRate.IsInRange(beatmap.BaseDifficulty.DrainRate);
            match &= criteria.CircleSize.IsInRange(beatmap.BaseDifficulty.CircleSize);
            match &= criteria.Length.IsInRange(beatmap.Length);
            match &= criteria.BPM.IsInRange(beatmap.BPM);

            match &= criteria.BeatDivisor.IsInRange(beatmap.BeatDivisor);
            match &= criteria.OnlineStatus.IsInRange(beatmap.Status);

            match &= criteria.Creator.Matches(beatmap.Metadata.AuthorString);
            match &= criteria.Artist.Matches(beatmap.Metadata.Artist) ||
                     criteria.Artist.Matches(beatmap.Metadata.ArtistUnicode);

            if (match)
            {
                foreach (var criteriaTerm in criteria.SearchTerms)
                {
                    match &=
                        beatmap.Metadata.SearchableTerms.Any(term => term.IndexOf(criteriaTerm, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                        beatmap.Version.IndexOf(criteriaTerm, StringComparison.InvariantCultureIgnoreCase) >= 0;
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
                    var ruleset = Beatmap.Get().RulesetID.CompareTo(otherBeatmap.Beatmap.Get().RulesetID);
                    if (ruleset != 0) return ruleset;

                    return Beatmap.Get().StarDifficulty.CompareTo(otherBeatmap.Beatmap.Get().StarDifficulty);
            }
        }

        public override string ToString() => Beatmap.ToString();
    }
}
