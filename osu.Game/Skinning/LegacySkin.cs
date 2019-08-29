// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Text;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class LegacySkin : Skin
    {
        protected TextureStore Textures;

        protected IResourceStore<SampleChannel> Samples;

        public LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager)
            : this(skin, new LegacySkinResourceStore<SkinFileInfo>(skin, storage), audioManager, "skin.ini")
        {
            // defaults should only be applied for non-beatmap skins (which are parsed via this constructor).
            if (!Configuration.CustomColours.ContainsKey("SliderBall")) Configuration.CustomColours["SliderBall"] = new Color4(2, 170, 255, 255);
        }

        protected LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager, string filename)
            : base(skin)
        {
            Stream stream = storage.GetStream(filename);
            if (stream != null)
                using (StreamReader reader = new StreamReader(stream))
                    Configuration = new LegacySkinDecoder().Decode(reader);
            else
                Configuration = new SkinConfiguration();

            Samples = audioManager.GetSampleStore(storage);
            Textures = new TextureStore(new TextureLoaderStore(storage));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Textures?.Dispose();
            Samples?.Dispose();
        }

        private const double default_frame_time = 1000 / 60d;

        public override Drawable GetDrawableComponent(ISkinComponent component)
        {
            bool animatable = false;
            bool looping = true;

            switch (component)
            {
                case "Play/osu/cursor":
                    if (GetTexture("cursor") != null)
                        return new LegacyCursor();

                    return null;

                case "Play/osu/sliderfollowcircle":
                    animatable = true;
                    break;

                case "Play/Miss":
                    component = "hit0";
                    animatable = true;
                    looping = false;
                    break;

                case "Play/Meh":
                    component = "hit50";
                    animatable = true;
                    looping = false;
                    break;

                case "Play/Good":
                    component = "hit100";
                    animatable = true;
                    looping = false;
                    break;

                case "Play/Great":
                    component = "hit300";
                    animatable = true;
                    looping = false;
                    break;

                case "Play/osu/number-text":
                    return !hasFont(Configuration.HitCircleFont)
                        ? null
                        : new LegacySpriteText(this, Configuration.HitCircleFont)
                        {
                            Scale = new Vector2(0.96f),
                            // Spacing value was reverse-engineered from the ratio of the rendered sprite size in the visual inspector vs the actual texture size
                            Spacing = new Vector2(-Configuration.HitCircleOverlap * 0.89f, 0)
                        };
            }

            return getAnimation(component.LookupName, animatable, looping);
        }

        public override Texture GetTexture(string componentName)
        {
            componentName = getFallbackName(componentName);

            float ratio = 2;
            var texture = Textures.Get($"{componentName}@2x");

            if (texture == null)
            {
                ratio = 1;
                texture = Textures.Get(componentName);
            }

            if (texture != null)
                texture.ScaleAdjust = ratio;

            return texture;
        }

        public override SampleChannel GetSample(ISampleInfo sampleInfo)
        {
            foreach (var lookup in sampleInfo.LookupNames)
            {
                var sample = Samples.Get(getFallbackName(lookup));

                if (sample != null)
                    return sample;
            }

            if (sampleInfo is HitSampleInfo hsi)
                // Try fallback to non-bank samples.
                return Samples.Get(hsi.Name);

            return null;
        }

        private bool hasFont(string fontName) => GetTexture($"{fontName}-0") != null;

        private string getFallbackName(string component)
        {
            string lastPiece = component.Split('/').Last();
            return component.StartsWith("Gameplay/taiko/") ? "taiko-" + lastPiece : lastPiece;
        }

        private Drawable getAnimation(string name, bool animatable, bool looping, string animationSeparator = "-")
        {
            Texture texture;

            Texture getFrameTexture(int frame) => GetTexture($"{name}{animationSeparator}{frame}");

            TextureAnimation animation = null;

            if (animatable)
            {
                for (int i = 0;; i++)
                {
                    if ((texture = getFrameTexture(i)) == null)
                        break;

                    if (animation == null)
                        animation = new TextureAnimation
                        {
                            DefaultFrameLength = default_frame_time,
                            Repeat = looping
                        };

                    animation.AddFrame(texture);
                }
            }

            if (animation != null)
                return animation;

            if ((texture = GetTexture(name)) != null)
                return new Sprite { Texture = texture };

            return null;
        }

        protected class LegacySkinResourceStore<T> : IResourceStore<byte[]>
            where T : INamedFileInfo
        {
            private readonly IHasFiles<T> source;
            private readonly IResourceStore<byte[]> underlyingStore;

            private string getPathForFile(string filename)
            {
                bool hasExtension = filename.Contains('.');

                var file = source.Files.Find(f =>
                    string.Equals(hasExtension ? f.Filename : Path.ChangeExtension(f.Filename, null), filename, StringComparison.InvariantCultureIgnoreCase));
                return file?.FileInfo.StoragePath;
            }

            public LegacySkinResourceStore(IHasFiles<T> source, IResourceStore<byte[]> underlyingStore)
            {
                this.source = source;
                this.underlyingStore = underlyingStore;
            }

            public Stream GetStream(string name)
            {
                string path = getPathForFile(name);
                return path == null ? null : underlyingStore.GetStream(path);
            }

            public IEnumerable<string> GetAvailableResources() => source.Files.Select(f => f.Filename);

            byte[] IResourceStore<byte[]>.Get(string name) => GetAsync(name).Result;

            public Task<byte[]> GetAsync(string name)
            {
                string path = getPathForFile(name);
                return path == null ? Task.FromResult<byte[]>(null) : underlyingStore.GetAsync(path);
            }

            #region IDisposable Support

            private bool isDisposed;

            protected virtual void Dispose(bool disposing)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                }
            }

            ~LegacySkinResourceStore()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        private class LegacySpriteText : OsuSpriteText
        {
            private readonly LegacyGlyphStore glyphStore;

            public LegacySpriteText(ISkin skin, string font)
            {
                Shadow = false;
                UseFullGlyphHeight = false;

                Font = new FontUsage(font, OsuFont.DEFAULT_FONT_SIZE);
                glyphStore = new LegacyGlyphStore(skin);
            }

            protected override TextBuilder CreateTextBuilder(ITexturedGlyphLookupStore store) => base.CreateTextBuilder(glyphStore);

            private class LegacyGlyphStore : ITexturedGlyphLookupStore
            {
                private readonly ISkin skin;

                public LegacyGlyphStore(ISkin skin)
                {
                    this.skin = skin;
                }

                public ITexturedCharacterGlyph Get(string fontName, char character)
                {
                    var texture = skin.GetTexture($"{fontName}-{character}");

                    if (texture != null)
                        // Approximate value that brings character sizing roughly in-line with stable
                        texture.ScaleAdjust *= 18;

                    if (texture == null)
                        return null;

                    return new TexturedCharacterGlyph(new CharacterGlyph(character, 0, 0, texture.Width, null), texture, 1f / texture.ScaleAdjust);
                }

                public Task<ITexturedCharacterGlyph> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));
            }
        }

        public class LegacyCursor : CompositeDrawable
        {
            public LegacyCursor()
            {
                Size = new Vector2(50);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                InternalChildren = new Drawable[]
                {
                    new NonPlayfieldSprite
                    {
                        Texture = skin.GetTexture("cursormiddle"),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new NonPlayfieldSprite
                    {
                        Texture = skin.GetTexture("cursor"),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            }
        }

        /// <summary>
        /// A sprite which is displayed within the playfield, but historically was not considered part of the playfield.
        /// Performs scale adjustment to undo the scale applied by <see cref="PlayfieldAdjustmentContainer"/> (osu! ruleset specifically).
        /// </summary>
        private class NonPlayfieldSprite : Sprite
        {
            public override Texture Texture
            {
                get => base.Texture;
                set
                {
                    if (value != null)
                        // stable "magic ratio". see OsuPlayfieldAdjustmentContainer for full explanation.
                        value.ScaleAdjust *= 1.6f;
                    base.Texture = value;
                }
            }
        }
    }
}
