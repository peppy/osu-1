// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens;
using osu.Game.Tests.Visual;
using osu.Presentation.Slides;

namespace osu.Presentation.Tests
{
    public class ScratchTestScene : OsuTestScene
    {
        public ScratchTestScene()
        {
            Add(new OsuScreenStack(new SlideTitle
            {
                RelativeSizeAxes = Axes.Both
            }));
        }
    }
}
