using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BiotechMonkeContinued
{
    // The constructor reads the settings file once at startup so PatchOperationToggled can
    // consult it while XML patches are applied. Nothing else here runs during play; the rest
    // only executes while the settings window is open.
    public class BiotechMonkeContinuedMod : Mod
    {
        public static BmcSettings Settings;

        private const string XmlCacheModId = "vr.missilegirl";
        private static readonly Color NoticeColor = new Color(1f, 0.66f, 0.32f);

        public BiotechMonkeContinuedMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<BmcSettings>();
        }

        public static bool IsEnabled(string key)
        {
            ToggleEntry entry = ToggleCatalogue.Get(key);
            if (entry != null && entry.ForcedOnBy != null)
            {
                return true;
            }
            if (Settings != null)
            {
                return Settings.IsEnabled(key);
            }
            return entry != null && entry.defaultOn;
        }

        public override string SettingsCategory()
        {
            return "Biotech Monke (Continued)";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            List<ToggleEntry> entries = ToggleCatalogue.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                ToggleEntry entry = entries[i];
                string forcedBy = entry.ForcedOnBy;
                if (forcedBy != null)
                {
                    // Locked on: the user's stored preference is kept but ignored while the
                    // forcing mod is present, so removing that mod restores their choice.
                    bool locked = true;
                    Rect row = listing.GetRect(Text.LineHeight);
                    Widgets.CheckboxLabeled(row, entry.label, ref locked, disabled: true);
                    TooltipHandler.TipRegion(row, entry.description);
                    GUI.color = Color.gray;
                    listing.Label("    Locked on: " + forcedBy + " supplies its own fur and mask.");
                    GUI.color = Color.white;
                    continue;
                }

                bool enabled = Settings.IsEnabled(entry.key);
                bool before = enabled;
                listing.CheckboxLabeled(entry.label, ref enabled, entry.description);
                if (enabled != before)
                {
                    Settings.SetEnabled(entry.key, enabled);
                }
            }

            listing.Gap(8f);
            GUI.color = Color.gray;
            listing.Label("Changes take effect after restarting the game.");
            GUI.color = Color.white;

            string notice = PatchNotice();
            if (notice != null)
            {
                listing.Gap(4f);
                GUI.color = NoticeColor;
                listing.Label(notice);
                GUI.color = Color.white;
            }

            listing.End();
        }

        // The toggled patches record when they run. If none ran this launch, the game's XML
        // patch pass never happened - an XML cache served the old combined document - and the
        // toggles are frozen at whatever they were when that cache was written.
        private static string PatchNotice()
        {
            if (PatchOperationToggled.RanThisLaunch.Count > 0)
            {
                return null;
            }
            bool cacheModActive = ModLister.GetActiveModWithIdentifier(XmlCacheModId, ignorePostfix: true) != null;
            return cacheModActive
                ? "These patches did not run this launch: Missile Girl is serving cached XML. After changing a "
                  + "toggle, clear the cache from Missile Girl's settings and restart."
                : "These patches did not run this launch. If an XML-caching mod is active, clear its cache and restart.";
        }
    }
}
