// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
<<<<<<< Updated upstream
using osu.Framework.Graphics;
using osu.Framework.Screens;
||||||| merged common ancestors
=======
using osu.Framework.Screens;
>>>>>>> Stashed changes
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi;

namespace osu.Game.Screens.Select
{
    public class MatchSongSelect : SongSelect, IMultiplayerSubScreen
    {
        public Action<PlaylistItem> Selected;

        public string ShortTitle => "song selection";
        public override string Title => ShortTitle.Humanize();

        public MatchSongSelect()
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
        }

        protected override bool OnStart()
        {
            var item = new PlaylistItem
            {
                Beatmap = Beatmap.Value.BeatmapInfo,
                Ruleset = Ruleset.Value,
                RulesetID = Ruleset.Value.ID ?? 0
            };

            item.RequiredMods.AddRange(SelectedMods.Value);

            Selected?.Invoke(item);

            if (this.IsCurrentScreen())
                this.Exit();

            return true;
        }

        protected override bool OnExiting(Screen next)
        {
            Beatmap.Disabled = true;
            Ruleset.Disabled = true;

            return base.OnExiting(next);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Beatmap.Disabled = false;
            Ruleset.Disabled = false;
        }
    }
}
