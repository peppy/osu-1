// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.GameTypes;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.Multi.Match
{
    public class MatchSubScreen : MultiplayerSubScreen
    {
<<<<<<< Updated upstream
        public override bool AllowBeatmapRulesetChange => false;
||||||| merged common ancestors
        public override bool AllowBeatmapRulesetChange => false;
        public override string Title => room.RoomID.Value == null ? "New room" : room.Name.Value;
        public override string ShortTitle => "room";
=======
        protected override bool DisallowExternalBeatmapRulesetChanges => true;

        public override string Title => room.RoomID.Value == null ? "New room" : room.Name.Value;
        public override string ShortTitle => "room";
>>>>>>> Stashed changes

        public override string Title { get; }

        public override string ShortTitle => "room";

        [Resolved(typeof(Room), nameof(Room.RoomID))]
        private Bindable<int?> roomId { get; set; }

        [Resolved(typeof(Room), nameof(Room.Name))]
        private Bindable<string> name { get; set; }

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        private BindableList<PlaylistItem> playlist { get; set; }

        public MatchSubScreen(Room room, Action<Screen> pushGameplayScreen)
        {
            Title = room.RoomID.Value == null ? "New room" : room.Name;

            InternalChild = new Match(pushGameplayScreen)
            {
                RelativeSizeAxes = Axes.Both,
                RequestBeatmapSelection = () => this.Push(new MatchSongSelect
                {
                    Selected = item =>
                    {
                        playlist.Clear();
                        playlist.Add(item);
                    },
                }),
                RequestExit = () =>
                {
                    if (this.IsCurrentScreen())
                        this.Exit();
                }
            };
        }

        public override bool OnExiting(IScreen next)
        {
            Manager?.PartRoom();
            return base.OnExiting(next);
        }

        private class Match : MultiplayerComposite
        {
            public Action RequestBeatmapSelection;
            public Action RequestExit;

            private readonly Action<Screen> pushGameplayScreen;

            private MatchLeaderboard leaderboard;

            [Resolved]
            private IBindableBeatmap gameBeatmap { get; set; }

            [Resolved]
            private BeatmapManager beatmapManager { get; set; }

            [Resolved(CanBeNull = true)]
            private OsuGame game { get; set; }

            public Match(Action<Screen> pushGameplayScreen)
            {
                this.pushGameplayScreen = pushGameplayScreen;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                MatchChatDisplay chat;
                Components.Header header;
                Info info;
                GridContainer bottomRow;
                MatchSettingsOverlay settings;

                InternalChildren = new Drawable[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                header = new Components.Header
                                {
                                    Depth = -1,
                                    RequestBeatmapSelection = () => RequestBeatmapSelection?.Invoke()
                                }
                            },
                            new Drawable[] { info = new Info { OnStart = onStart } },
                            new Drawable[]
                            {
                                bottomRow = new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            leaderboard = new MatchLeaderboard
                                            {
                                                Padding = new MarginPadding
                                                {
                                                    Left = 10 + OsuScreen.HORIZONTAL_OVERFLOW_PADDING,
                                                    Right = 10,
                                                    Vertical = 10,
                                                },
                                                RelativeSizeAxes = Axes.Both
                                            },
                                            new Container
                                            {
                                                Padding = new MarginPadding
                                                {
                                                    Left = 10,
                                                    Right = 10 + OsuScreen.HORIZONTAL_OVERFLOW_PADDING,
                                                    Vertical = 10,
                                                },
                                                RelativeSizeAxes = Axes.Both,
                                                Child = chat = new MatchChatDisplay
                                                {
                                                    RelativeSizeAxes = Axes.Both
                                                }
                                            },
                                        },
                                    },
                                }
                            },
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Distributed),
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = Components.Header.HEIGHT },
                        Child = settings = new MatchSettingsOverlay { RelativeSizeAxes = Axes.Both },
                    },
                };

<<<<<<< Updated upstream
                header.Tabs.Current.BindValueChanged(t =>
||||||| merged common ancestors
            header.OnRequestSelectBeatmap = () => Push(new MatchSongSelect { Selected = addPlaylistItem });
            header.Tabs.Current.ValueChanged += t =>
            {
                const float fade_duration = 500;
                if (t is SettingsMatchPage)
=======
            header.OnRequestSelectBeatmap = () =>
            {
                Push(new MatchSongSelect { Selected = addPlaylistItem });
            };
            header.Tabs.Current.ValueChanged += t =>
            {
                const float fade_duration = 500;
                if (t is SettingsMatchPage)
>>>>>>> Stashed changes
                {
                    const float fade_duration = 500;
                    if (t is SettingsMatchPage)
                    {
                        settings.Show();
                        info.FadeOut(fade_duration, Easing.OutQuint);
                        bottomRow.FadeOut(fade_duration, Easing.OutQuint);
                    }
                    else
                    {
                        settings.Hide();
                        info.FadeIn(fade_duration, Easing.OutQuint);
                        bottomRow.FadeIn(fade_duration, Easing.OutQuint);
                    }
                }, true);

                chat.Exit += () => RequestExit?.Invoke();

                beatmapManager.ItemAdded += beatmapAdded;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

<<<<<<< Updated upstream
                CurrentBeatmap.BindValueChanged(setBeatmap, true);
                CurrentRuleset.BindValueChanged(setRuleset, true);
            }
||||||| merged common ancestors
            game?.ForcefullySetBeatmap(beatmapManager.GetWorkingBeatmap(localBeatmap));
        }
=======
            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
        }
>>>>>>> Stashed changes

            private void setBeatmap(BeatmapInfo beatmap)
            {
                // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
                var localBeatmap = beatmap == null ? null : beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == beatmap.OnlineBeatmapID);

<<<<<<< Updated upstream
                game?.ForcefullySetBeatmap(beatmapManager.GetWorkingBeatmap(localBeatmap));
            }
||||||| merged common ancestors
            game?.ForcefullySetRuleset(ruleset);
        }
=======
            Ruleset.Value = ruleset;
        }
>>>>>>> Stashed changes

            private void setRuleset(RulesetInfo ruleset)
            {
                if (ruleset == null)
                    return;

                game?.ForcefullySetRuleset(ruleset);
            }

            private void beatmapAdded(BeatmapSetInfo model, bool existing, bool silent) => Schedule(() =>
            {
                if (gameBeatmap.Value != beatmapManager.DefaultBeatmap)
                    return;

<<<<<<< Updated upstream
                if (CurrentBeatmap.Value == null)
                    return;
||||||| merged common ancestors
            if (localBeatmap != null)
                game?.ForcefullySetBeatmap(beatmapManager.GetWorkingBeatmap(localBeatmap));
        });
=======
            if (localBeatmap != null)
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
        });
>>>>>>> Stashed changes

                // Try to retrieve the corresponding local beatmap
                var localBeatmap = beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == CurrentBeatmap.Value.OnlineBeatmapID);

                if (localBeatmap != null)
                    game?.ForcefullySetBeatmap(beatmapManager.GetWorkingBeatmap(localBeatmap));
            });

            private void onStart()
            {
                gameBeatmap.Value.Mods.Value = CurrentMods.Value.ToArray();

                switch (Type.Value)
                {
                    default:
                    case GameTypeTimeshift _:
                        pushGameplayScreen?.Invoke(new PlayerLoader(() => new TimeshiftPlayer(Playlist.First())
                        {
                            Exited = () => leaderboard.RefreshScores()
                        }));
                        break;
                }
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (beatmapManager != null)
                    beatmapManager.ItemAdded -= beatmapAdded;
            }
        }
    }
}
