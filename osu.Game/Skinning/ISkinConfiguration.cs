// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.Formats;

namespace osu.Game.Skinning
{
    public interface ISkinConfiguration : IHasComboColours, IHasCustomColours
    {
        string HitCircleFont { get; set; }
        int HitCircleOverlap { get; set; }
        float? SliderBorderSize { get; set; }
        bool? CursorExpand { get; set; }
        SkinInfo SkinInfo { get; set; }
    }
}
