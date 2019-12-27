﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class LegacyBeatmapSkin : LegacySkin
    {
        public LegacyBeatmapSkin(RealmWrapper<BeatmapInfo> beatmap, IResourceStore<byte[]> storage, AudioManager audioManager)
            : base(createSkinInfo(beatmap), new LegacySkinResourceStore<BeatmapSetFileInfo>(() => beatmap.Get().BeatmapSet, storage), audioManager, beatmap.Get().Path)
        {
            // Disallow default colours fallback on beatmap skins to allow using parent skin combo colours. (via SkinProvidingContainer)
            Configuration.AllowDefaultComboColoursFallback = false;
        }

        private static SkinInfo createSkinInfo(BeatmapInfo beatmap) =>
            new SkinInfo { Name = beatmap.ToString(), Creator = beatmap.Metadata.Author.ToString() };
    }
}
