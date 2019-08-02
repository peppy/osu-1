using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osu.Game.Graphics;

namespace osu.Presentation
{
    public abstract class SlideWithImage : SlideWithTitle
    {
        private readonly string image;

        protected SlideWithImage(string title, string image)
            : base(title)
        {
            this.image = image;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            Content.Add(new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Texture = textures.Get(image),
                FillMode = FillMode.Fit
            });
        }
    }

    public abstract class SlideWithTitle : Screen
    {
        protected Container Content;

        protected SlideWithTitle(string title)
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
                        Content = new ConfinedInputContainer
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                },
            };
        }

        public class ConfinedInputContainer : Container
        {
            public override bool PropagateNonPositionalInputSubTree =>
                ReceivePositionalInputAt(GetContainingInputManager().CurrentState.Mouse.Position);
        }
    }
}
