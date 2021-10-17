using Commander.Content;
using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;

namespace Commander
{
    [HarmonyPatch(typeof(BlueprintsCache), "Init")]
    internal static class BlueprintsCacheInitPatch
    {
        private static bool _loaded;

        internal static void Postfix()
        {
            if (_loaded) { return; }
            _loaded = true;

            DivineSaintArchetype.Create();
        }
    }
}
