// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public abstract class SkinComponent<T> : ISkinComponent where T : struct
    {
        private readonly T component;

        protected SkinComponent(T component)
        {
            this.component = component;
        }

        protected abstract string RulesetPrefix { get; }
        protected abstract string ComponentGroup { get; }
        protected abstract string ComponentName { get; }

        public string ComopnentGroup => RulesetPrefix != null ? $"{RulesetPrefix}/{ComponentGroup}" : ComponentGroup;
        public string LookupName => $"{ComponentGroup}/{ComponentName}";
    }
}
