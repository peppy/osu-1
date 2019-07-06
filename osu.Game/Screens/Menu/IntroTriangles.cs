// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
    public class IntroTriangles : StartupScreen
    {
        private const string menu_music_beatmap_hash = "a1556d0801b3a6b175dda32ef546f0ec812b400499f575c44fccbe9c67f9b1e5";

        /// <summary>
        /// Whether we have loaded the menu previously.
        /// </summary>
        public bool DidLoadMenu;

        private MainMenu mainMenu;
        private SampleChannel welcome;
        private SampleChannel seeya;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBlack();

        private readonly BindableDouble exitingVolumeFade = new BindableDouble(1);

        [Resolved]
        private AudioManager audio { get; set; }

        private Bindable<bool> menuVoice;
        private Bindable<bool> menuMusic;
        private Track track;
        private WorkingBeatmap introBeatmap;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, BeatmapManager beatmaps, Framework.Game game)
        {
            menuVoice = config.GetBindable<bool>(OsuSetting.MenuVoice);
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
            track.Restart();

            welcome = audio.Samples.Get(@"welcome");
            seeya = audio.Samples.Get(@"seeya");
        }

        private const double delay_step_one = 2800;
        private const double delay_step_two = 600;

        public const int EXIT_DELAY = 3000;

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                Beatmap.Value = introBeatmap;
                introBeatmap = null;

                if (menuVoice.Value && !menuMusic.Value)
                    // triangles has welcome sound included in the track. only play this if the user doesn't want menu music.
                    welcome.Play();

                Scheduler.AddDelayed(delegate
                {
                    // Only start the current track if it is the menu music. A beatmap's track is started when entering the Main Manu.
                    if (menuMusic.Value)
                    {
                        track.Start();
                        track = null;
                    }

                    LoadComponentAsync(mainMenu = new MainMenu());

                    Scheduler.AddDelayed(delegate
                    {
                        DidLoadMenu = true;
                        this.Push(mainMenu);
                    }, delay_step_one);
                }, delay_step_two);
            }

            logo.Colour = Color4.White;
            logo.Ripple = false;

            const int quick_appear = 350;

            int initialMovementTime = logo.Alpha > 0.2f ? quick_appear : 0;

            if (!resuming)
            {
                logo.Hide();

                AddInternal(new TrianglesIntroSequence(logo)
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = new FramedClock(track)
                });
            }
            else
            {
                logo.MoveTo(new Vector2(0.5f), initialMovementTime, Easing.OutQuint);

                logo.Triangles = false;

                logo
                    .ScaleTo(1, initialMovementTime, Easing.OutQuint)
                    .FadeIn(quick_appear, Easing.OutQuint)
                    .Then()
                    .RotateTo(20, EXIT_DELAY * 1.5f)
                    .FadeOut(EXIT_DELAY);
            }
        }

        public override void OnSuspending(IScreen next)
        {
            this.FadeOut(300);
            base.OnSuspending(next);
        }

        public override bool OnExiting(IScreen next)
        {
            //cancel exiting if we haven't loaded the menu yet.
            return !DidLoadMenu;
        }

        public override void OnResuming(IScreen last)
        {
            this.FadeIn(300);

            double fadeOutTime = EXIT_DELAY;
            //we also handle the exit transition.
            if (menuVoice.Value)
                seeya.Play();
            else
                fadeOutTime = 500;

            audio.AddAdjustment(AdjustableProperty.Volume, exitingVolumeFade);
            this.TransformBindableTo(exitingVolumeFade, 0, fadeOutTime).OnComplete(_ => this.Exit());

            //don't want to fade out completely else we will stop running updates.
            Game.FadeTo(0.01f, fadeOutTime);

            base.OnResuming(last);
        }

        private class TrianglesIntroSequence : CompositeDrawable
        {
            private readonly OsuLogo logo;
            private readonly OsuSpriteText welcomeText;

            private readonly RulesetFlow rulesets;
            private readonly Container rulesetsScale;
            private readonly Sprite logoLineArt;

            private const double text_0 = 200;
            private const double text_1 = 400;
            private const double text_2 = 700;
            private const double text_3 = 900;

            private const double rulesets_0 = 1400;
            private const double rulesets_1 = 1600;
            private const double rulesets_2 = 1800;

            private const double logo_0 = 2100;

            public TrianglesIntroSequence(OsuLogo logo)
            {
                this.logo = logo;

                InternalChildren = new Drawable[]
                {
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
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                logoLineArt.Texture = textures.Get(@"Menu/logo");
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
                    logoLineArt.Delay(logo_0).FadeIn().ScaleTo(2.4f).Then().ScaleTo(1, 1000, Easing.InQuint);
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
        }
    }
}
