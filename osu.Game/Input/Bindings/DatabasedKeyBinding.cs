// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Game.Database;
using Realms;

namespace osu.Game.Input.Bindings
{
    [Table("KeyBinding")]
    public class DatabasedKeyBinding : RealmObject, IHasPrimaryKey
    {
        public string ID { get; set; }

        public int? RulesetID { get; set; }

        public int? Variant { get; set; }

        [Ignored]
        public KeyBinding KeyBinding
        {
            get => new KeyBinding(KeyBindingString.Split("=>").First(), KeyBindingString.Split("=>").Last());
            set => KeyBindingString = value.ToString();
        }

        public string KeyBindingString { get; set; }
    }
}
