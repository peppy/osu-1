// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Overlays;
using osu.Game.Overlays.Skinning;

namespace osu.Game.Tests.Visual
{
    public class TestSceneSkinningPanel : OsuTestScene
    {
        private readonly SkinningPanel panel;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SkinCheckboxRow),
            typeof(SkinHeaderRow),
            typeof(SkinSettingsRow),
        };

        public TestSceneSkinningPanel()
        {
            Child = panel = new SkinningPanel();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            panel.Show();
        }
    }
}
