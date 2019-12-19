// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary
{
    /// <summary>
    /// The timeline that sits at the bottom of the editor.
    /// </summary>
    public class SummaryTimeline : BottomBarContainer
    {
        private Container belowContent;
        private Container aboveContent;

        private IBindable<WorkingBeatmap> beatmap { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new MarkerPart() { RelativeSizeAxes = Axes.Both },
                belowContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray5,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreRight,
                            Size = new Vector2(5)
                        },
                        new Box
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 1,
                            EdgeSmoothness = new Vector2(0, 1),
                        },
                        new Circle
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(5)
                        },
                    }
                },
                aboveContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };

            belowContent.Clear();
            aboveContent.Clear();

            LoadComponentAsync(new ControlPointPart
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
                Depth = 1, // guarantee below bookmarks
                Height = 0.35f
            }, belowContent.Add);

            LoadComponentAsync(new BookmarkPart
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                Height = 0.35f
            }, belowContent.Add);

            LoadComponentAsync(new BreakPart
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Height = 0.25f
            }, aboveContent.Add);
        }
    }
}
