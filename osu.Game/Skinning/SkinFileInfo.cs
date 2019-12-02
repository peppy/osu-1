// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;
using osu.Game.IO;
using Realms;

namespace osu.Game.Skinning
{
    public class SkinFileInfo : RealmObject, INamedFileInfo, IHasPrimaryKey
    {
        public string ID { get; set; }

        [Ignored]
        public string FetchedID { get; set; }

        public int SkinInfoID { get; set; }

        public int FileInfoID { get; set; }

        public FileInfo FileInfo { get; set; }

        // [System.ComponentModel.DataAnnotations.Required]
        public string Filename { get; set; }
    }
}
