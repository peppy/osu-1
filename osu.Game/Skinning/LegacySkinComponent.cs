// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class LegacySkinComponent : ISkinComponent
    {
        private readonly string name;

        public LegacySkinComponent(string name)
        {
            this.name = name;
        }

        public string LookupName => name;
    }
}
