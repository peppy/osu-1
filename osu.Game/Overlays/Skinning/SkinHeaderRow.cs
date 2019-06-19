// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Skinning
{
    public class SkinHeaderRow : SkinSettingsRow
    {
        public SkinHeaderRow(string title, Skin[] sources)
            : base(title, sources)
        {
        }

        protected override Drawable CreateCellContent(Skin skin) => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = $"{skin.SkinInfo.Name}",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
                },
                new OsuSpriteText
                {
                    Text = skin.SkinInfo.Creator,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Y = 14,
                    Font = OsuFont.GetFont(weight: FontWeight.Regular, size: 12),
                }
            }
        };
    }
}
