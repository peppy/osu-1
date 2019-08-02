// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Screens;

namespace osu.Presentation.Slides
{
    internal class SlideFrameworkVideo : SlideWithVideo
    {
        private Track track;

        public SlideFrameworkVideo()
            : base("osu!framework", "framework")
        {
        }

        [BackgroundDependencyLoader]
        private void load(ITrackStore tracks)
        {
            track = tracks.Get("framework");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            track.Start();
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);
            track.Stop();
        }
    }
}
