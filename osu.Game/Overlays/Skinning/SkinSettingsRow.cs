// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Skinning
{
    public abstract class SkinSettingsRow : Container
    {
        protected abstract Drawable CreateCellContent(Skin skin);

        protected SkinSettingsRow(string title, Skin[] sources)
        {
            FillFlowContainer flow;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Child = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5),
                AutoSizeAxes = Axes.Y
            };

            flow.Add(new TitleCell(new OsuSpriteText
            {
                Text = title,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            foreach (var s in sources)
                flow.Add(new Cell(CreateCellContent(s)));
        }

        private class TitleCell : Cell
        {
            public TitleCell(Drawable content)
                : base(content)
            {
                Size = new Vector2(SkinningPanel.TITLE_SIZE, 40);
            }
        }

        private class Cell : Container
        {
            public Cell(Drawable content)
            {
                Size = new Vector2(SkinningPanel.CELL_SIZE, 40);

                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(5),
                        Children = new[]
                        {
                            content
                        }
                    },
                };
            }
        }
    }
}
