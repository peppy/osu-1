// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Presentation.Slides;
using osuTK.Input;

namespace osu.Presentation
{
    public class PresentationGame : Framework.Game
    {
        private readonly Type[] slides =
        {
            typeof(SlideTitle),

            typeof(SlideTransitionalVideo),

            typeof(SlideLazerProject),

            typeof(SlideFramework),
            typeof(SlideFrameworkVideo),

            typeof(SlideCoreFocuses),

            typeof(SlideOpenSource),
            typeof(SlideOpenSourceContributors),

            typeof(SlideHighPerformance),

            typeof(SlideCodeQuality),

            typeof(SlideTestBrowser),

            typeof(SlideBackwardsCompatibility),

            typeof(SlideExtensibility),

            typeof(SlideCrossPlatform),

            typeof(SlideManyLazerGame),

            typeof(SlideTournamentVideo),
            typeof(SlideTournament),

            typeof(SlideWhereWeAre),
        };

        private int current = -1;

        private readonly ScreenStack stack;

        public PresentationGame()
        {
            Child = new DrawSizePreservingFillContainer
            {
                Padding = new MarginPadding(50),
                Child = stack = new ScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            next();
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Resources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(@"osu.Presentation.dll"), @"Resources"));

            var largeStore = new LargeTextureStore(Host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures")));
            largeStore.AddStore(Host.CreateTextureLoaderStore(new OnlineStore()));
            dependencies.Cache(largeStore);
        }

        private void next()
        {
            if (current + 1 >= slides.Length)
                return;

            stack.Push((Screen)Activator.CreateInstance(slides[++current]));
        }

        private void prev()
        {
            if (stack.CurrentScreen == null) return;

            stack.CurrentScreen.Exit();
            current--;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            switch (e.Key)
            {
                case Key.Left:
                    prev();
                    return true;

                case Key.Right:
                    next();
                    return true;

                case Key.Number0:
                    Host.Window.CursorState = CursorState.Default;
                    return true;
            }

            return base.OnKeyDown(e);
        }
    }
}
