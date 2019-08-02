using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.MathUtils;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osuTK;
using osuTK.Input;

namespace osu.Presentation
{
    public abstract class SlideWithTitle : Screen
    {
        protected Container Content;
        private readonly Container paddingContainer;

        private const float header_text_size = 120;
        private const float padding = 30;

        private readonly MarginPadding adjustedPadding = new MarginPadding(padding) { Top = header_text_size + padding };

        private readonly SpriteText titleSprite;

        protected SlideWithTitle(string title)
        {
            InternalChildren = new Drawable[]
            {
                titleSprite = new SpriteText
                {
                    Text = title,
                    Font = OsuFont.Default.With(size: header_text_size),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
                paddingContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = adjustedPadding,
                    Children = new Drawable[]
                    {
                        Content = new ConfinedInputContainer
                        {
                            Masking = true,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
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

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat)
                return false;

            switch (e.Key)
            {
                case Key.Number1:
                    ToggleZoom();
                    return true;

                case Key.Number2:
                    Content.RotateTo(RNG.NextSingle(-360, 360), 1000, Easing.OutElastic);
                    return true;

                case Key.Number3:
                    Content.ScaleTo(RNG.NextSingle(0.5f, 2), 1000, Easing.OutElastic);
                    return true;

                case Key.Number4:
                    Content.TransformTo(nameof(Shear), new Vector2(RNG.NextSingle(-1, 1)), 1000, Easing.OutElastic);
                    return true;

                case Key.Number9:
                    Content.RotateTo(0, 1000, Easing.OutBounce);
                    Content.ScaleTo(1, 1000, Easing.OutBounce);
                    Content.TransformTo(nameof(Shear), new Vector2(0), 1000, Easing.OutBounce);
                    return true;
            }

            return base.OnKeyDown(e);
        }

        public void ToggleZoom()
        {
            if (paddingContainer.Padding.Top == 0)
            {
                titleSprite.FadeIn(500);
                paddingContainer.Padding = adjustedPadding;
            }
            else
            {
                titleSprite.FadeOut(500).OnComplete(_ => paddingContainer.Padding = new MarginPadding());
            }
        }
    }
}
