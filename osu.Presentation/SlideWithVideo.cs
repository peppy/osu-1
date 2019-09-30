// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Video;

namespace osu.Presentation
{
    internal abstract class SlideWithVideo : SlideWithTitle
    {
        private readonly string videoFilename;

        [BackgroundDependencyLoader]
        private void load(Framework.Game game)
        {
            var video = game.Resources.GetStream($"{videoFilename}.mp4");

            if (video != null)
            {
                Content.Add(new VideoSprite(game.Resources.GetStream($"{videoFilename}.mp4"))
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fit,
                });
            }
        }

        protected SlideWithVideo(string title, string videoFilename)
            : base(title)
        {
            this.videoFilename = videoFilename;
        }
    }
}
