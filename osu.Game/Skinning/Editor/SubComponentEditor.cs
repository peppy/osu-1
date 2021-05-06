// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning.Editor
{
    internal class SubComponentEditor : RoundedContentEditorScreen
    {
        [Cached(typeof(ISkinnableComponent))]
        private readonly ISkinnableComponent component;

        public SubComponentEditor(ISkinnableComponent component)
            : base(EditorScreenMode.Design)
        {
            this.component = component;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(new Drawable[]
            {
                new SectionsContainer<SetupSection>
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new SetupSection[]
                    {
                        new SpriteEditor(),
                    }
                },
            });
        }
    }

    internal class SpriteEditor : SetupSection
    {
        [Resolved]
        private ISkinnableComponent component { get; set; }

        public override LocalisableString Title => "Sprites";

        private readonly List<Texture> trackedTextures = new List<Texture>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            checkForComponents();
        }

        private void checkForComponents()
        {
            Drawable drawableComponent = (Drawable)component;

            foreach (var s in drawableComponent.ChildrenOfType<Sprite>())
            {
                var texture = s.Texture;

                if (trackedTextures.Contains(texture))
                    continue;

                trackedTextures.Add(texture);

                Add(new Sprite
                {
                    Texture = texture,
                });
            }

            // We'd hope to eventually be running this in a more sensible way, but this handles situations where new drawables become present (ie. during ongoing gameplay)
            // or when drawables in the target are loaded asynchronously and may not be immediately available when this BlueprintContainer is loaded.
            Scheduler.AddDelayed(checkForComponents, 1000);
        }
    }
}
