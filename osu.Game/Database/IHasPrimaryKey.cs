// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using osu.Game.IO.Serialization;

namespace osu.Game.Database
{
    public interface IHasPrimaryKey : IJsonSerializable
    {
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        string ID { get; set; }
    }
}
