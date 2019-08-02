// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Tests.Visual;

namespace osu.Presentation.Tests
{
    public class PresentationGameTestScene : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var presentation = new PresentationGame();
            presentation.SetHost(host);
            Add(presentation);
        }
    }
}
