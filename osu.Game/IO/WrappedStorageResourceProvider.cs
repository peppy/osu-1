// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

#nullable enable

namespace osu.Game.IO
{
    /// <summary>
    /// A resource provider which allows a custom override for accessing files.
    /// </summary>
    public class WrappedStorageResourceProvider : IStorageResourceProvider
    {
        private readonly IStorageResourceProvider? resources;

        public WrappedStorageResourceProvider(IStorageResourceProvider? resources, IResourceStore<byte[]> overridingResourceStore)
        {
            this.resources = resources;

            Files = overridingResourceStore;
        }

        public AudioManager? AudioManager => resources?.AudioManager ?? null;

        public IResourceStore<byte[]> Files { get; }

        public IResourceStore<TextureUpload>? CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) =>
            resources?.CreateTextureLoaderStore(underlyingStore) ?? null;
    }
}
