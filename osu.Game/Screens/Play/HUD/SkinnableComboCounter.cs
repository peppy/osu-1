// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableComboCounter : SkinnableDrawable, IComboCounter
    {
        public SkinnableComboCounter()
            : base(new HUDSkinComponent(HUDSkinComponents.ComboCounter), createDefault)
        {
        }

        private IComboCounter skinnedCounter;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            skinnedCounter = (IComboCounter)Drawable;
            skinnedCounter.Current.BindTo(Current);
        }

        private static Drawable createDefault(ISkinComponent skinComponent) => new DefaultComboCounter();

        public Bindable<int> Current { get; } = new Bindable<int>();
    }
}
