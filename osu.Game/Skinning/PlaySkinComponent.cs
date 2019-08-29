// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;

namespace osu.Game.Skinning
{
    public abstract class PlaySkinComponent<T> : ISkinComponent where T : struct
    {
        protected readonly T Component;

        protected PlaySkinComponent(T component)
        {
            this.Component = component;
        }

        protected virtual string RulesetPrefix => string.Empty;
        protected virtual string ComponentGroup => string.Empty;
        protected virtual string ComponentName => Component.ToString();

        string ISkinComponent.ComponentGroup =>
            string.Join("/", new[] { "Play", RulesetPrefix, ComponentGroup }.Where(s => !string.IsNullOrEmpty(s)));

        public string LookupName => $"{ComponentGroup}/{ComponentName}";
    }
}
