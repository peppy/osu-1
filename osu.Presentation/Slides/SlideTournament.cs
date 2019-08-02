// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Tournament;

namespace osu.Presentation.Slides
{
    internal class SlideTournament : SlideWithTitle
    {
        public SlideTournament()
            : base("tournament")
        {
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var tournament = new TournamentGame();
            tournament.SetHost(host);
            Content.Add(tournament);
        }
    }
}
