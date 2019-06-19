// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Skinning
{
    public class SkinnableDrawable : SkinnableDrawable<Drawable>
    {
        public SkinnableDrawable(string name, Func<string, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true)
            : base(name, defaultImplementation, allowFallback, restrictSize)
        {
        }
    }

    public class SkinnableDrawable<T> : SkinReloadableDrawable
        where T : Drawable
    {
        private readonly Func<string, T> createDefault;

        private readonly string componentName;

        private readonly bool restrictSize;

        /// <summary>
        ///
        /// </summary>
        /// <param name="name">The namespace-complete resource name for this skinnable element.</param>
        /// <param name="defaultImplementation">A function to create the default skin implementation of this element.</param>
        /// <param name="allowFallback">A conditional to decide whether to allow fallback to the default implementation if a skinned element is not present.</param>
        /// <param name="restrictSize">Whether a user-skin drawable should be limited to the size of our parent.</param>
        public SkinnableDrawable(string name, Func<string, T> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true)
            : base(allowFallback)
        {
            componentName = name;
            createDefault = defaultImplementation;
            this.restrictSize = restrictSize;

            RelativeSizeAxes = Axes.Both;
        }

        private CancellationTokenSource cts;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            cts?.Cancel();

            var loader = new AsyncLoadDrawable(skin, componentName, allowFallback, createDefault, restrictSize);

            LoadComponentAsync(loader, d =>
            {
                DrawableLoaded(d.Drawable);
                InternalChild = d;
            }, (cts = new CancellationTokenSource()).Token);
        }

        protected virtual void DrawableLoaded(Drawable drawable)
        {
        }

        private class AsyncLoadDrawable : CompositeDrawable
        {
            private readonly ISkinSource skin;
            private readonly string componentName;
            private readonly bool allowFallback;
            private readonly Func<string, T> createDefault;
            private readonly bool restrictSize;

            public Drawable Drawable { get; private set; }

            public AsyncLoadDrawable(ISkinSource skin, string componentName, bool allowFallback, Func<string, T> createDefault, bool restrictSize)
            {
                this.skin = skin;
                this.componentName = componentName;
                this.allowFallback = allowFallback;
                this.createDefault = createDefault;
                this.restrictSize = restrictSize;

                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Drawable = skin.GetDrawableComponent(componentName);

                if (Drawable != null)
                {
                    if (restrictSize)
                    {
                        Drawable.RelativeSizeAxes = Axes.Both;
                        Drawable.Size = Vector2.One;
                        Drawable.Scale = Vector2.One;
                        Drawable.FillMode = FillMode.Fit;
                    }
                }
                else if (allowFallback)
                    Drawable = createDefault(componentName);

                if (Drawable != null)
                {
                    Drawable.Origin = Anchor.Centre;
                    Drawable.Anchor = Anchor.Centre;

                    InternalChild = Drawable;
                }
                else
                    Expire();
            }
        }
    }
}
