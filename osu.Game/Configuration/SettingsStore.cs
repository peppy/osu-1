// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Database;
using osu.Game.Skinning;

namespace osu.Game.Configuration
{
    public class SettingsStore : DatabaseBackedStore
    {
        public event Action SettingChanged;

        public SettingsStore(DatabaseContextFactory contextFactory)
            : base(contextFactory)
        {
        }

        /// <summary>
        /// Retrieve <see cref="DatabasedSetting"/>s for a specified ruleset/variant content.
        /// </summary>
        /// <param name="rulesetId">The ruleset's internal ID.</param>
        /// <param name="variant">An optional variant.</param>
        /// <param name="skinId">An optional <see cref="SkinInfo"/> ID.</param>
        /// <returns></returns>
        public List<DatabasedSetting> Query(int? rulesetId = null, int? variant = null, int? skinId = null) =>
            ContextFactory.Get().DatabasedSetting.Where(b => b.RulesetID == rulesetId && b.Variant == variant && b.SkinInfoID == skinId).ToList();

        public void Update(DatabasedSetting setting)
        {
            using (ContextFactory.GetForWrite())
            {
                var newValue = setting.Value;
                Refresh(ref setting);
                setting.Value = newValue;
            }

            SettingChanged?.Invoke();
        }

        public void Delete(DatabasedSetting setting)
        {
            using (var usage = ContextFactory.GetForWrite())
                usage.Context.Remove(setting);
        }
    }
}
