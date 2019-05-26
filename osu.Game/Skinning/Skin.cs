// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Skinning
{
    public abstract class Skin : IDisposable, ISkin, IEquatable<Skin>
    {
        public readonly SkinInfo SkinInfo;

        public ISkinConfiguration Configuration { get; protected set; }

        public abstract Drawable GetDrawableComponent(string componentName);

        public abstract SampleChannel GetSample(string sampleName);

        public abstract Texture GetTexture(string componentName);

        public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : ISkinConfiguration
            => Configuration is TConfiguration conf ? query.Invoke(conf) : default;

        protected Skin(SkinInfo skin)
        {
            SkinInfo = skin;
        }

        #region Disposal

        ~Skin()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;
        }

        #endregion

        public bool Equals(Skin other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Equals(SkinInfo, other.SkinInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((Skin)obj);
        }

        public override int GetHashCode() => (SkinInfo != null ? SkinInfo.GetHashCode() : 0);
    }
}
