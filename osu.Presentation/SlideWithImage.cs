// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Presentation
{
    public abstract class SlideWithImages : SlideWithTitle
    {
        private readonly string imageName;

        protected SlideWithImages(string title, string imageName)
            : base(title)
        {
            this.imageName = imageName;
        }

        private LargeTextureStore textures;

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            this.textures = textures;
            loadImage();
        }

        private bool loadImage()
        {
            var texture = textures.Get(imageName + (index == 0 ? string.Empty : (index + 1).ToString()));

            if (texture == null) return false;

            Content.Child = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Texture = texture,
                FillMode = FillMode.Fit
            };

            return true;
        }

        private int index;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            switch (e.Key)
            {
                case Key.Left:
                    index--;
                    if (loadImage())
                        return true;

                    index++;
                    return false;

                case Key.Right:
                    index++;
                    if (loadImage())
                        return true;

                    index--;
                    return false;
            }

            return base.OnKeyDown(e);
        }
    }
}
