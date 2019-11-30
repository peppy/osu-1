// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations.Schema;
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
        public KeyBinding KeyBinding { get; set; } = new KeyBinding();

        [Column("Keys")]
        public string KeysString
        {
            get => KeyBinding.KeyCombination.ToString();
            private set => KeyBinding.KeyCombination = value;
        }

        [Column("Action")]
        public int IntAction
        {
            get => (int)KeyBinding.Action;
            set => KeyBinding.Action = value;
        }
    }
}
