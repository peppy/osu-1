// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
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

            typeof(SlideHistory),

            typeof(SlideCoreFocuses),
            typeof(SlideOpenSource),
            typeof(SlideHighPerformance),
            typeof(SlideCodeQuality),
            typeof(SlideBackwardsCompatibility),
            typeof(SlideExtensibility),
            typeof(SlideCrossPlatform),

            typeof(SlideManyLazerGame),
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
            switch (e.Key)
            {
                case Key.Left:
                    prev();
                    return true;

                case Key.Right:
                    next();
                    return true;
            }

            return base.OnKeyDown(e);
        }
    }
}
