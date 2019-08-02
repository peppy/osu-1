// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens;
using osu.Game.Tests.Visual;

namespace osu.Presentation.Tests
{
    public class BulletSlideTestScene : OsuTestScene
    {
        public BulletSlideTestScene()
        {
            Add(new OsuScreenStack(new TestSlide
            {
                RelativeSizeAxes = Axes.Both
            })
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        private class TestSlide : BulletSlide
        {
            public TestSlide()
                : base("Title of slide", new[]
                {
                    "This is the first point",
                    "This is a second point",
                    "Wangs",
                    "Dicks"
                })
            {
            }
        }
    }
}
