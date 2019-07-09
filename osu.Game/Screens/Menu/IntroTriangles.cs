// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;
using osu.Game.Screens.Backgrounds;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public class IntroTriangles : IntroScreen
    {
        private const string menu_music_beatmap_hash = "a1556d0801b3a6b175dda32ef546f0ec812b400499f575c44fccbe9c67f9b1e5";

        private SampleChannel welcome;

        protected override BackgroundScreen CreateBackground() => background = new BackgroundScreenDefault(false);

        private const double flash_length = 1000;

        [Resolved]
        private AudioManager audio { get; set; }

        private Bindable<bool> menuMusic;
        private Track track;
        private WorkingBeatmap introBeatmap;

        private BackgroundScreenDefault background;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, BeatmapManager beatmaps, Framework.Game game)
        {
            menuMusic = config.GetBindable<bool>(OsuSetting.MenuMusic);

            BeatmapSetInfo setInfo = null;

            if (!menuMusic.Value)
            {
                var sets = beatmaps.GetAllUsableBeatmapSets();
                if (sets.Count > 0)
                    setInfo = beatmaps.QueryBeatmapSet(s => s.ID == sets[RNG.Next(0, sets.Count - 1)].ID);
            }

            if (setInfo == null)
            {
                setInfo = beatmaps.QueryBeatmapSet(b => b.Hash == menu_music_beatmap_hash);

                if (setInfo == null)
                {
                    // we need to import the default menu background beatmap
                    setInfo = beatmaps.Import(new ZipArchiveReader(game.Resources.GetStream(@"Tracks/triangles.osz"), "triangles.osz")).Result;

                    setInfo.Protected = true;
                    beatmaps.Update(setInfo);
                }
            }

            introBeatmap = beatmaps.GetWorkingBeatmap(setInfo.Beatmaps[0]);
            track = introBeatmap.Track;

            track.Stop();
            track.Reset();

            if (config.Get<bool>(OsuSetting.MenuVoice) && !menuMusic.Value)
                // triangles has welcome sound included in the track. only play this if the user doesn't want menu music.
                welcome = audio.Samples.Get(@"welcome");
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                Beatmap.Value = introBeatmap;
                introBeatmap = null;

                welcome?.Play();

                // Only start the current track if it is the menu music. A beatmap's track is started when entering the Main Manu.
                if (menuMusic.Value)
                    track.Start();

                AddInternal(new TrianglesIntroSequence(logo, background)
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = new FramedClock(track),
                    LoadMenu = LoadMenu
                });

                PrepareMenuLoad();
            }
        }

        public override void OnSuspending(IScreen next)
        {
            track = null;

            this.FadeOut(flash_length);
            base.OnSuspending(next);
        }

        private class TrianglesIntroSequence : CompositeDrawable
        {
            private readonly OsuLogo logo;
            private readonly BackgroundScreenDefault background;
            private readonly OsuSpriteText welcomeText;

            private readonly RulesetFlow rulesets;
            private readonly Container rulesetsScale;
            private readonly Sprite logoLineArt;

            private readonly GlitchingTriangles triangles;

            public Action LoadMenu;

            private const double text_0 = 200;
            private const double text_1 = 400;
            private const double text_2 = 700;
            private const double text_3 = 900;
            private const double text_glitch = 1060;

            private const double rulesets_0 = 1450;
            private const double rulesets_1 = 1650;
            private const double rulesets_2 = 1850;

            private const double logo_0 = 2100;
            private const double logo_1 = 3000;

            public TrianglesIntroSequence(OsuLogo logo, BackgroundScreenDefault background)
            {
                this.logo = logo;
                this.background = background;

                InternalChildren = new Drawable[]
                {
                    triangles = new GlitchingTriangles
                    {
                        Alpha = 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(0.4f, 0.16f)
                    },
                    welcomeText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Padding = new MarginPadding { Bottom = 10 },
                        Font = OsuFont.GetFont(weight: FontWeight.Light, size: 42),
                        Alpha = 1,
                        Spacing = new Vector2(5),
                    },
                    rulesetsScale = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            rulesets = new RulesetFlow()
                        }
                    },
                    logoLineArt = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                };
            }

            private OsuGameBase game;

            [BackgroundDependencyLoader]
            private void load(TextureStore textures, OsuGameBase game)
            {
                logoLineArt.Texture = textures.Get(@"Menu/logo");
                this.game = game;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                using (BeginAbsoluteSequence(0, true))
                {
                    welcomeText.Delay(text_3).TransformTo(nameof(welcomeText.Spacing), new Vector2(50, 0), 5000);
                    welcomeText.Delay(rulesets_0).FadeOut();

                    rulesetsScale.Delay(rulesets_0).ScaleTo(0.8f, 1000);
                    rulesetsScale.Delay(rulesets_2).ScaleTo(1.3f, 1000);

                    rulesets.Hide();
                    rulesets.Delay(rulesets_0).FadeIn().ScaleTo(1).TransformSpacingTo(new Vector2(200, 0));
                    rulesets.Delay(rulesets_1).ScaleTo(2).TransformSpacingTo(new Vector2(30, 0));
                    rulesets.Delay(rulesets_2).ScaleTo(4).TransformSpacingTo(new Vector2(10, 0));

                    rulesets.Delay(logo_0).FadeOut();

                    logoLineArt.Hide();
                    logoLineArt.Delay(logo_0).FadeIn().ScaleTo(3f).Then().ScaleTo(0, 980, Easing.InQuint);

                    triangles.Delay(text_glitch).FadeIn();
                    triangles.Delay(rulesets_0).FadeOut();

                    logoLineArt.Delay(logo_1).FadeOut();
                    logo.Delay(logo_1).FadeIn().OnComplete(_ =>
                    {
                        Box flash;

                        game.Add(flash = new Box
                        {
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingMode.Additive,
                        });

                        flash.FadeOutFromOne(flash_length, Easing.Out);

                        LoadMenu();
                    });

                    background.Hide();
                    background.Delay(logo_1).FadeIn();
                }
            }

            protected override void Update()
            {
                base.Update();

                if (Clock.CurrentTime > text_3)
                    welcomeText.Text = "welcome to osu!";
                else if (Clock.CurrentTime > text_2)
                    welcomeText.Text = "welcome to";
                else if (Clock.CurrentTime > text_1)
                    welcomeText.Text = "welcome";
                else if (Clock.CurrentTime > text_0)
                    welcomeText.Text = "wel";
                else
                    welcomeText.Text = "";
            }

            private class RulesetFlow : FillFlowContainer
            {
                [BackgroundDependencyLoader]
                private void load(RulesetStore rulesets)
                {
                    var modes = new List<Drawable>();

                    foreach (var ruleset in rulesets.AvailableRulesets)
                    {
                        var icon = new ConstrainedIconContainer
                        {
                            Icon = ruleset.CreateInstance().CreateIcon(),
                            Size = new Vector2(30),
                        };

                        modes.Add(icon);
                    }

                    AutoSizeAxes = Axes.Both;
                    Children = modes;

                    Anchor = Anchor.Centre;
                    Origin = Anchor.Centre;
                }
            }

            private class GlitchingTriangles : CompositeDrawable
            {
                public GlitchingTriangles()
                {
                    RelativeSizeAxes = Axes.Both;
                }

                private double? lastGenTime;

                private const double time_between_triangles = 22;

                protected override void Update()
                {
                    base.Update();

                    if (lastGenTime == null || Time.Current - lastGenTime > time_between_triangles)
                    {
                        lastGenTime = (lastGenTime ?? Time.Current) + time_between_triangles;

                        Drawable triangle = new OutlineTriangle(RNG.NextBool(), (RNG.NextSingle() + 0.2f) * 80)
                        {
                            RelativePositionAxes = Axes.Both,
                            Position = new Vector2(RNG.NextSingle(), RNG.NextSingle()),
                        };

                        AddInternal(triangle);

                        triangle.FadeOutFromOne(120);
                    }
                }

                /// <summary>
                /// Represents a sprite that is drawn in a triangle shape, instead of a rectangle shape.
                /// </summary>
                public class OutlineTriangle : BufferedContainer
                {
                    public OutlineTriangle(bool outlineOnly, float size)
                    {
                        Size = new Vector2(size);

                        InternalChildren = new Drawable[]
                        {
                            new Triangle { RelativeSizeAxes = Axes.Both },
                        };

                        if (outlineOnly)
                        {
                            AddInternal(new Triangle
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = Color4.Black,
                                Size = new Vector2(size - 5),
                                Blending = BlendingMode.None,
                            });
                        }

                        Blending = BlendingMode.Additive;
                        CacheDrawnFrameBuffer = true;
                    }
                }
            }
        }
    }
}
