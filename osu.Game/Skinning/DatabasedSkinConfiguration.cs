// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Configuration;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DatabasedSkinConfiguration : DatabasedConfigManager<SkinSetting>, ISkinConfiguration
    {
        private readonly SettingsStore settings;

        public DatabasedSkinConfiguration(SkinInfo skin, SettingsStore settings)
            : base(settings, null, null, skin)
        {
            this.settings = settings;
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            Set(SkinSetting.CursorExpand, true);
        }

        public List<Color4> ComboColours { get; set; }
        public Dictionary<string, Color4> CustomColours { get; set; }
        public string HitCircleFont { get; set; }
        public int HitCircleOverlap { get; set; }
        public float? SliderBorderSize { get; set; }
        public bool? CursorExpand { get; set; }
        public SkinInfo SkinInfo { get; set; }
    }
}
