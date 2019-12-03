// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmapSet : CarouselGroupEagerSelect
    {
        public IEnumerable<CarouselBeatmap> Beatmaps => InternalChildren.OfType<CarouselBeatmap>();

        public RealmWrapper<BeatmapSetInfo> BeatmapSet;

        public CarouselBeatmapSet(RealmWrapper<BeatmapSetInfo> beatmapSet)
        {
            BeatmapSet = beatmapSet ?? throw new ArgumentNullException(nameof(beatmapSet));

            beatmapSet.Get().Beatmaps
                      .Where(b => !b.Hidden)
                      .Select(b => new CarouselBeatmap(new RealmWrapper<BeatmapInfo>(b, beatmapSet.ContextFactory)))
                      .ForEach(AddChild);
        }

        protected override DrawableCarouselItem CreateDrawableRepresentation() => new DrawableCarouselBeatmapSet(this);

        public override int CompareTo(FilterCriteria criteria, CarouselItem other)
        {
            if (!(other is CarouselBeatmapSet otherSet))
                return base.CompareTo(criteria, other);

            switch (criteria.Sort)
            {
                default:
                case SortMode.Artist:
                    return string.Compare(BeatmapSet.Get().Metadata.Artist, otherSet.BeatmapSet.Get().Metadata.Artist, StringComparison.InvariantCultureIgnoreCase);

                case SortMode.Title:
                    return string.Compare(BeatmapSet.Get().Metadata.Title, otherSet.BeatmapSet.Get().Metadata.Title, StringComparison.InvariantCultureIgnoreCase);

                case SortMode.Author:
                    return string.Compare(BeatmapSet.Get().Metadata.Author?.Username, otherSet.BeatmapSet.Get().Metadata.Author?.Username, StringComparison.InvariantCultureIgnoreCase);

                case SortMode.DateAdded:
                    return otherSet.BeatmapSet.Get().DateAdded.CompareTo(BeatmapSet.Get().DateAdded);

                case SortMode.BPM:
                    return compareUsingAggregateMax(otherSet, b => b.BPM);

                case SortMode.Length:
                    return compareUsingAggregateMax(otherSet, b => b.Length);

                case SortMode.Difficulty:
                    return compareUsingAggregateMax(otherSet, b => b.StarDifficulty);
            }
        }

        /// <summary>
        /// All beatmaps which are not filtered and valid for display.
        /// </summary>
        private IEnumerable<BeatmapInfo> validBeatmaps => Beatmaps.Where(b => !b.Filtered.Value).Select(b => b.Beatmap.Get());

        private int compareUsingAggregateMax(CarouselBeatmapSet other, Func<BeatmapInfo, double> func)
        {
            var ourBeatmaps = validBeatmaps.Any();
            var otherBeatmaps = other.validBeatmaps.Any();

            if (!ourBeatmaps && !otherBeatmaps) return 0;
            if (!ourBeatmaps) return -1;
            if (!otherBeatmaps) return 1;

            return validBeatmaps.Max(func).CompareTo(other.validBeatmaps.Max(func));
        }

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);
            Filtered.Value = InternalChildren.All(i => i.Filtered.Value);
        }

        public override string ToString() => BeatmapSet.ToString();
    }
}
