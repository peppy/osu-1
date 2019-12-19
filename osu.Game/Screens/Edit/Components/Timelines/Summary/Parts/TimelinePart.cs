// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// Represents a part of the summary timeline..
    /// </summary>
    public abstract class TimelinePart : Container
    {
        private readonly WorkingBeatmap beatmap;

        private readonly Container timeline;

        protected override Container<Drawable> Content => timeline;

        protected TimelinePart(WorkingBeatmap beatmap)
        {
            this.beatmap = beatmap;
            AddInternal(timeline = new Container { RelativeSizeAxes = Axes.Both });
        }

        protected override void LoadComplete()
        {
            updateRelativeChildSize();
            base.LoadComplete();
        }

        private void updateRelativeChildSize()
        {
            // the track may not be loaded completely (only has a length once it is).
            if (!beatmap.Track.IsLoaded)
            {
                timeline.RelativeChildSize = Vector2.One;
                Schedule(updateRelativeChildSize);
                return;
            }

            timeline.RelativeChildSize = new Vector2((float)Math.Max(1, beatmap.Track.Length), 1);
        }
    }
}
