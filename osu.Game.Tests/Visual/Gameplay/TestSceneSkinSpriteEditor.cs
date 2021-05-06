// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Skinning.Editor;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinSpriteEditor : SkinnableTestScene
    {
        [Cached(typeof(DrawableHitObject))]
        private DrawableHitObject drawableObject = new DrawableHitCircle(new HitCircle());

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create editor overlay", () =>
            {
                SetContents(() =>
                {
                    LegacyMainCirclePiece circlePiece = new LegacyMainCirclePiece();

                    return new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            circlePiece,
                            new SubComponentEditor(circlePiece),
                        }
                    };
                });
            });
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
