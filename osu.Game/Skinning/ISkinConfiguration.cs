// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.Formats;

namespace osu.Game.Skinning
{
    public interface ISkinConfiguration : IHasComboColours, IHasCustomColours
    {
        string HitCircleFont { get; }
        int HitCircleOverlap { get; }
        float? SliderBorderSize { get; }
        float? SliderPathRadius { get; }
        bool? CursorExpand { get; }
        SkinInfo SkinInfo { get; }
    }
}
