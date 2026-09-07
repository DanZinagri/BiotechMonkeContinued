using System.Collections.Generic;
using Verse;

namespace BiotechMonkeContinued
{
    // Stores only the toggles the user has changed away from their catalogue default, so new
    // toggles pick up their default and the settings file stays tiny.
    public class BmcSettings : ModSettings
    {
        private Dictionary<string, bool> overrides = new Dictionary<string, bool>();

        public bool IsEnabled(string key)
        {
            if (overrides.TryGetValue(key, out bool value))
            {
                return value;
            }
            ToggleEntry entry = ToggleCatalogue.Get(key);
            return entry != null && entry.defaultOn;
        }

        public void SetEnabled(string key, bool enabled)
        {
            ToggleEntry entry = ToggleCatalogue.Get(key);
            if (entry != null && entry.defaultOn == enabled)
            {
                overrides.Remove(key);
            }
            else
            {
                overrides[key] = enabled;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref overrides, "overrides", LookMode.Value, LookMode.Value);
            if (overrides == null)
            {
                overrides = new Dictionary<string, bool>();
            }
        }
    }
}
