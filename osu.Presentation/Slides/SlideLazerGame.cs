// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game;
using osuTK;
using osuTK.Input;

namespace osu.Presentation.Slides
{
    internal class SlideManyLazerGame : SlideWithTitle
    {
        private FillFlowContainer fill;
        private GameHost host;

        public SlideManyLazerGame()
            : base("lazer")
        {
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            this.host = host;
            Content.Add(fill = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Full,
            });
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Slash:
                    addNewOsu();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        private void addNewOsu()
        {
            OsuGame game = new OsuGame();
            game.SetHost(host);

            var container = new Container
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Child = game
            };

            LoadComponentAsync(container, loaded =>
            {
                fill.Add(loaded);

                Vector2 targetScale = findTargetScale();
                foreach (var child in fill.Children)
                    child.ResizeTo(targetScale, 1000, Easing.OutQuint);
            });
        }

        private Vector2 findTargetScale()
        {
            Vector2 ratio = Vector2.One;

            float count = fill.Children.Count;
            bool vertical = false;

            while ((count /= 2) > 0.5f)
            {
                if (vertical)
                    ratio.Y *= 0.5f;
                else
                    ratio.X *= 0.5f;

                vertical = !vertical;
            }

            return ratio - new Vector2(0.01f);
        }
    }
}
