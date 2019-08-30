// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class OsuLegacySkin : ISkin
    {
        private readonly ISkin source;

        private Lazy<SkinConfiguration> configuration;

        private Lazy<bool> hasHitCircle;

        /// <summary>
        /// On osu-stable, hitcircles have 5 pixels of transparent padding on each side to allow for shadows etc.
        /// Their hittable area is 128px, but the actual circle portion is 118px.
        /// We must account for some gameplay elements such as slider bodies, where this padding is not present.
        /// </summary>
        private const float legacy_circle_radius = 64 - 5;

        public OsuLegacySkin(ISkinSource source)
        {
            this.source = source;

            source.SourceChanged += sourceChanged;
            sourceChanged();
        }

        private void sourceChanged()
        {
            // these need to be lazy in order to ensure they aren't called before the dependencies have been loaded into our source.
            configuration = new Lazy<SkinConfiguration>(() =>
            {
                var config = new SkinConfiguration();
                if (hasHitCircle.Value)
                    config.SliderPathRadius = legacy_circle_radius;

                // defaults should only be applied for non-beatmap skins (which are parsed via this constructor).
                config.CustomColours["SliderBall"] =
                    source.GetValue<SkinConfiguration, Color4?>(s => s.CustomColours.TryGetValue("SliderBall", out var val) ? val : (Color4?)null)
                    ?? new Color4(2, 170, 255, 255);

                return config;
            });

            hasHitCircle = new Lazy<bool>(() => source.GetTexture("hitcircle") != null);
        }

        private const double default_frame_time = 1000 / 60d;

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (!(component is OsuSkinComponent osuComponent))
                return null;

            switch (osuComponent.Component)
            {
                case OsuSkinComponents.SliderBall:
                    var sliderBallContent = getAnimation("sliderb", true, true, "");

                    if (sliderBallContent != null)
                    {
                        var size = sliderBallContent.Size;

                        sliderBallContent.RelativeSizeAxes = Axes.Both;
                        sliderBallContent.Size = Vector2.One;

                        return new LegacySliderBall(sliderBallContent)
                        {
                            Size = size
                        };
                    }

                    return null;

                case OsuSkinComponents.HitCircle:
                    if (hasHitCircle.Value)
                        return new LegacyMainCirclePiece();

                    return null;
            }

            return null;
        }

        private Drawable getAnimation(string componentName, bool animatable, bool looping, string animationSeparator = "-")
        {
            Texture texture;

            Texture getFrameTexture(int frame) => source.GetTexture($"{componentName}{animationSeparator}{frame}");

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

            if ((texture = source.GetTexture(componentName)) != null)
                return new Sprite { Texture = texture };

            return null;
        }

        public Texture GetTexture(string componentName) => null;

        public SampleChannel GetSample(ISampleInfo sample) => null;

        public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration
            => configuration.Value is TConfiguration conf ? query.Invoke(conf) : default;
    }
}
