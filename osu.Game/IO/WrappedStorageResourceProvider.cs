// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace osu.Game.IO
{
    /// <summary>
    /// A resource provider which allows a custom override for accessing files.
    /// </summary>
    public class WrappedStorageResourceProvider : IStorageResourceProvider
    {
        private readonly IStorageResourceProvider resources;

        private readonly IResourceStore<byte[]> overridingResourceStore;

        public WrappedStorageResourceProvider(IStorageResourceProvider resources, IResourceStore<byte[]> overridingResourceStore)
        {
            this.resources = resources;
            this.overridingResourceStore = overridingResourceStore;
        }

        public AudioManager AudioManager => resources.AudioManager;

        public IResourceStore<byte[]> Files => overridingResourceStore ?? resources.Files;

        public IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore)
        {
            return resources.CreateTextureLoaderStore(underlyingStore);
        }
    }
}
