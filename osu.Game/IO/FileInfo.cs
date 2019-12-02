// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Game.Database;
using Realms;

namespace osu.Game.IO
{
    public class FileInfo : RealmObject, IHasPrimaryKey
    {
        public string ID { get; set; }

        [Ignored]
        public string FetchedID { get; set; }

        public string Hash { get; set; }

        public string StoragePath => Path.Combine(Hash.Remove(1), Hash.Remove(2), Hash);

        public int ReferenceCount { get; set; }
    }
}
