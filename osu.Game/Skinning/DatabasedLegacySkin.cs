// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Game.Configuration;

namespace osu.Game.Skinning
{
    public class DatabasedLegacySkin : LegacySkin, ISkinSource
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
            : this(skin, new LegacySkinResourceStore<SkinFileInfo>(skin, storage), audioManager, new DatabasedSkinConfiguration(skin, settings))
        {
        }

        private DatabasedLegacySkin(SkinInfo skin, LegacySkinResourceStore<SkinFileInfo> storage, AudioManager audioManager, DatabasedSkinConfiguration settings)
            : base(skin, storage, audioManager, settings)
        {
            settings.ConfigurationChanged += () => SourceChanged?.Invoke();
        }

        public event Action SourceChanged;
    }
}
