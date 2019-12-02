// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Database;
using Realms;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetInfo : RealmObject, IHasPrimaryKey, IHasFiles<BeatmapSetFileInfo>, ISoftDelete, IEquatable<BeatmapSetInfo>
    {
        public string ID { get; set; }

        public int? OnlineBeatmapSetID { get; set; }

        public DateTimeOffset DateAdded { get; set; }

        public int StatusInt { get; set; } = (int)BeatmapSetOnlineStatus.None;

        public BeatmapSetOnlineStatus Status
        {
            get => (BeatmapSetOnlineStatus)StatusInt;
            set => StatusInt = (int)value;
        }

        public BeatmapMetadata Metadata { get; set; }

        public IList<BeatmapInfo> Beatmaps { get; }

        [Ignored]
        public BeatmapSetOnlineInfo OnlineInfo { get; set; }

        [Ignored]
        public BeatmapSetMetrics Metrics { get; set; }

        /// <summary>
        /// The maximum star difficulty of all beatmaps in this set.
        /// </summary>
        public double MaxStarDifficulty => Beatmaps?.Max(b => b.StarDifficulty) ?? 0;

        /// <summary>
        /// The maximum playable length in milliseconds of all beatmaps in this set.
        /// </summary>
        public double MaxLength => Beatmaps?.Max(b => b.Length) ?? 0;

        /// <summary>
        /// The maximum BPM of all beatmaps in this set.
        /// </summary>
        public double MaxBPM => Beatmaps?.Max(b => b.BPM) ?? 0;

        public bool DeletePending { get; set; }

        public string Hash { get; set; }

        public string StoryboardFile => Files?.FirstOrDefault(f => f.Filename.EndsWith(".osb"))?.Filename;

        public IList<BeatmapSetFileInfo> Files { get; }

        public override string ToString() => Metadata?.ToString() ?? base.ToString();

        public bool Protected { get; set; }

        public bool Equals(BeatmapSetInfo other)
        {
            if (other == null)
                return false;

            if (ID != null && other.ID != null)
                return ID == other.ID;

            if (OnlineBeatmapSetID.HasValue && other.OnlineBeatmapSetID.HasValue)
                return OnlineBeatmapSetID == other.OnlineBeatmapSetID;

            if (!string.IsNullOrEmpty(Hash) && !string.IsNullOrEmpty(other.Hash))
                return Hash == other.Hash;

            return ReferenceEquals(this, other);
        }
    }
}
