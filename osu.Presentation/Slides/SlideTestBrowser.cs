// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Tests;

namespace osu.Presentation.Slides
{
    internal class SlideTestBrowser : SlideWithTitle
    {
        public SlideTestBrowser()
            : base("testing")
        {
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var testBrowser = new OsuTestBrowser();
            testBrowser.SetHost(host);
            Content.Add(testBrowser);
        }
    }
}
