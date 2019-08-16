// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DatabasedSkinConfiguration : DatabasedConfigManager<SkinSetting>, ISkinConfiguration
    {
        public DatabasedSkinConfiguration(SkinInfo skin, SettingsStore settings)
            : base(settings, null, null, skin)
        {
            SkinInfo = skin;
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            Set<bool?>(SkinSetting.CursorExpand, null);
        }

        protected override void AddBindable<TBindable>(SkinSetting lookup, Bindable<TBindable> bindable)
        {
            base.AddBindable(lookup, bindable);

            bindable.ValueChanged += _ => ConfigurationChanged?.Invoke();
        }

        public List<Color4> ComboColours { get; set; }
        public Dictionary<string, Color4> CustomColours { get; set; }
        public string HitCircleFont { get; set; }
        public int HitCircleOverlap { get; set; }
        public float? SliderBorderSize { get; set; }
        public float? SliderPathRadius { get; set; }

        public bool? CursorExpand => Get<bool?>(SkinSetting.CursorExpand);

        public SkinInfo SkinInfo { get; set; }

        public event Action ConfigurationChanged;
    }
}
