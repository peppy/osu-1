// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Skinning;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays
{
    public class SkinningPanel : SettingsSubPanel
    {
        public const float CELL_SIZE = 100;
        public const float TITLE_SIZE = 140;
        public const float PADDING = 5;

        [BackgroundDependencyLoader]
        private void load(SkinManager skinManager)
        {
            var availableSkins = new[] { skinManager.Default, skinManager.CurrentSkin.Value, skinManager.UserSkin }.Distinct().ToArray();

            ContentContainer.Width = (CELL_SIZE + PADDING) * availableSkins.Length + TITLE_SIZE + PADDING;

            AddSection(new GeneralSettings(availableSkins));
        }

        private class GeneralSettings : SettingsSection
        {
            public override IconUsage Icon => FontAwesome.Regular.Clipboard;
            public override string Header => "General";

            public GeneralSettings(Skin[] availableSkins)
            {
                FlowContent.Spacing = new Vector2(5);

                Children = new Drawable[]
                {
                    new SkinHeaderRow("Information", availableSkins),
                    new SkinCheckboxRow("Expand Mouse", availableSkins),
                };
            }
        }
    }
}
