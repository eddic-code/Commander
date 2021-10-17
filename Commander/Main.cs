using HarmonyLib;
using UnityModManagerNet;

namespace Commander
{
    public static class Main 
    {
        public static bool Enabled;

        public static bool Load(UnityModManager.ModEntry modEntry) 
        {
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll();
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) 
        {
            Enabled = value;
            return true;
        }
    }
}
