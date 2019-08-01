using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics;

namespace osu.Presentation
{
    public class SlideWithTitle : Screen
    {
        protected Container Content;

        public SlideWithTitle(string title)
        {
            const float header_text_size = 120;

            InternalChildren = new Drawable[]
            {
                new SpriteText
                {
                    Text = title,
                    Font = OsuFont.Default.With(size: header_text_size),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(50) { Top = header_text_size + 50 },
                    Children = new Drawable[]
                    {
                        Content = new Container
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                },
            };
        }
    }
}
