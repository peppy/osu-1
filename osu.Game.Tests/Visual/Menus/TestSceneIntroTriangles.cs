// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public class TestSceneIntroTriangles : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuLogo),
            typeof(StartupScreen),
            typeof(OsuScreen)
        };

        [Cached]
        private OsuLogo logo;

        private Drawable introStack;

        public TestSceneIntroTriangles()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                    Colour = Color4.Black,
                },
                logo = new OsuLogo
                {
                    Alpha = 0,
                    RelativePositionAxes = Axes.Both,
                    Depth = float.MinValue,
                    Position = new Vector2(0.5f),
                }
            };

            AddStep("restart sequence", () =>
            {
                introStack?.Expire();
                Add(introStack = new OsuScreenStack(new IntroTriangles())
                {
                    RelativeSizeAxes = Axes.Both,
                });
            });
        }
    }
}
