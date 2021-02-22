// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Overlays;

namespace osu.Game.Screens.Menu
{
    public class OsuScreenWithExitConfirmation : OsuScreen
    {
        [Resolved(canBeNull: true)]
        private DialogOverlay dialogOverlay { get; set; }

        private bool exitConfirmed;

        public override bool OnExiting(IScreen next)
        {
            if (!exitConfirmed && dialogOverlay != null)
            {
                if (dialogOverlay.CurrentDialog is ConfirmExitDialog exitDialog)
                {
                    exitConfirmed = true;
                    exitDialog.Buttons.First().Click();
                }
                else
                {
                    dialogOverlay.Push(new ConfirmExitDialog(confirmAndExit, () => { }));
                    return true;
                }
            }

            return false;
        }

        private void confirmAndExit()
        {
            exitConfirmed = true;
            this.Exit();
        }
    }
}
