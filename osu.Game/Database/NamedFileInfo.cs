// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations;
using osu.Game.IO;

namespace osu.Game.Database
{
    /// <summary>
    /// Represent a join model which gives a filename and scope to a <see cref="FileInfo"/>.
    /// </summary>
    public abstract class NamedFileInfo : IHasPrimaryKey
    {
        public int ID { get; set; }

        // An explicit foreign key property isn't required but is recommended and may be helpful to have
        public int FileInfoID { get; set; }

        public FileInfo FileInfo { get; set; }

        [Required]
        public string Filename { get; set; }
    }
}
