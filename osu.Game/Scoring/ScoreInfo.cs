// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using Realms;

namespace osu.Game.Scoring
{
    public class ScoreInfo : RealmObject, IHasFiles<ScoreFileInfo>, IHasPrimaryKey, ISoftDelete, IEquatable<ScoreInfo>
    {
        [PrimaryKey]
        public string ID { get; set; }

        public int RankInt { get; set; }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank
        {
            get => (ScoreRank)RankInt;
            set => RankInt = (int)value;
        }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty("accuracy")]
        [Column(TypeName = "DECIMAL(1,4)")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonIgnore]
        public int Combo { get; set; } // Todo: Shouldn't exist in here

        [JsonProperty("passed")]
        [Ignored]
        public bool Passed { get; set; } = true;

        [JsonIgnore]
        public virtual RulesetInfo Ruleset { get; set; }

        [JsonProperty("mods")]
        [Ignored]
        public Mod[] Mods
        {
            get
            {
                if (ModsString == null)
                    return Array.Empty<Mod>();

                return getModsFromRuleset(JsonConvert.DeserializeObject<DeserializedMod[]>(ModsString));
            }
            set => ModsString = JsonConvert.SerializeObject(value);
        }

        private Mod[] getModsFromRuleset(DeserializedMod[] mods) => Ruleset.CreateInstance().GetAllMods().Where(mod => mods.Any(d => d.Acronym == mod.Acronym)).ToArray();

        [JsonIgnore]
        [Column("Mods")]
        public string ModsString { get; set; }

        [Ignored]
        [JsonProperty("user")]
        public User User
        {
            get => new User { Username = UserString };
            set => UserString = value.Username;
        }

        [JsonIgnore]
        [Column("User")]
        public string UserString { get; set; }

        [JsonIgnore]
        [Column("UserID")]
        public long? UserID
        {
            get => User?.Id ?? 1;
            set
            {
                if (User == null)
                    User = new User();

                User.Id = value ?? 1;
            }
        }

        [JsonIgnore]
        public virtual BeatmapInfo Beatmap { get; set; }

        [JsonIgnore]
        public long? OnlineScoreID { get; set; }

        [JsonIgnore]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("statistics")]
        public Dictionary<HitResult, int> Statistics
        {
            get
            {
                if (StatisticsJson == null)
                    return new Dictionary<HitResult, int>();

                return JsonConvert.DeserializeObject<Dictionary<HitResult, int>>(StatisticsJson);
            }
            set => StatisticsJson = JsonConvert.SerializeObject(value);
        }

        [JsonIgnore]
        [Column("Statistics")]
        public string StatisticsJson { get; set; }

        [JsonIgnore]
        public IList<ScoreFileInfo> Files { get; }

        [JsonIgnore]
        public string Hash { get; set; }

        [JsonIgnore]
        public bool DeletePending { get; set; }

        [Serializable]
        protected class DeserializedMod : IMod
        {
            public string Acronym { get; set; }

            public bool Equals(IMod other) => Acronym == other?.Acronym;
        }

        public override string ToString() => $"{User} playing {Beatmap}";

        public bool Equals(ScoreInfo other)
        {
            if (other == null)
                return false;

            if (ID != null && other.ID != null)
                return ID == other.ID;

            if (OnlineScoreID.HasValue && other.OnlineScoreID.HasValue)
                return OnlineScoreID == other.OnlineScoreID;

            if (!string.IsNullOrEmpty(Hash) && !string.IsNullOrEmpty(other.Hash))
                return Hash == other.Hash;

            return ReferenceEquals(this, other);
        }
    }
}
