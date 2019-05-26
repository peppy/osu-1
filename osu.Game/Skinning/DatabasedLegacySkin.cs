// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Game.Configuration;

namespace osu.Game.Skinning
{
    public class DatabasedLegacySkin : LegacySkin
    {
        public new DatabasedSkinConfiguration Configuration => (DatabasedSkinConfiguration)base.Configuration;

        /// <summary>
        /// Create a skin sourcing configuration from database.
        /// </summary>
        /// <param name="skin"></param>
        /// <param name="storage"></param>
        /// <param name="audioManager"></param>
        /// <param name="settings"></param>
        public DatabasedLegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager, SettingsStore settings)
            : base(skin, new LegacySkinResourceStore<SkinFileInfo>(skin, storage), audioManager, new DatabasedSkinConfiguration(skin, settings))
        {
        }
    }
}
