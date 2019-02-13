// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Scoring.Legacy;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual
{
    public class TestCaseReplayVis : OsuTestCase
    {
        private readonly FillFlowContainer fillFlow;

        public TestCaseReplayVis()
        {
            Add(new ScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = fillFlow = new FillFlowContainer
                {
                    Scale = new Vector2(1, 0.1f),
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                }
            });

            addScore("replay-0_1741498_2733059714.osr");
            addScore("replay-0_1741498_2700724083.osr");
            addScore("replay-0_1741498_2646935289.osr");
            addScore("replay-0_1741498_2704540309.osr");
            addScore("replay-0_1741498_2698424979.osr");
            addScore("replay-0_1741498_2732458009.osr");
            addScore("replay-0_1741498_2700098230.osr");
            addScore("replay-0_1741498_2733255641.osr");
        }

        private void addScore(string path)
        {
            var parser = new TestScoreParser();

            var score = parser.Parse(File.OpenRead($"/Users/Dean/Downloads/{path}"));

            var osuFrames = score.Replay.Frames.OfType<OsuReplayFrame>();

            Container timeline = new Container { AutoSizeAxes = Axes.Both, Padding = new MarginPadding { Right = 50 } };

            OsuReplayFrame lastFrame = null;
            foreach (var frame in osuFrames)
            {
                if (lastFrame != null)
                {
                    foreach (var newAction in frame.Actions.Where(a => !lastFrame.Actions.Contains(a)))
                    {
                        timeline.Add(new Box
                        {
                            X = newAction == OsuAction.LeftButton ? 0 : 20,
                            Y = (float)frame.Time,
                            Width = 10,
                            Height = (float)((osuFrames.FirstOrDefault(f => f.Time > frame.Time && !f.Actions.Contains(newAction))?.Time ?? 0) - frame.Time)
                        });
                    }
                }

                lastFrame = frame;
            }

            fillFlow.Add(timeline);
        }

        private class TestScoreParser : LegacyScoreParser
        {
            protected override Ruleset GetRuleset(int rulesetId) => new OsuRuleset();

            protected override WorkingBeatmap GetBeatmap(string md5Hash) => new TestWorkingBeatmap(new Beatmap());
        }
    }
}
