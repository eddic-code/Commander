// ReSharper disable InconsistentNaming

using HarmonyLib;
using Kingmaker.View;

namespace Commander.Utility
{
    public static class MoveThroughOthers
    {
        [HarmonyPatch(typeof(UnitMovementAgent), nameof(UnitMovementAgent.AvoidanceDisabled), MethodType.Getter)]
        internal static class UnitMovementAgentAvoidanceDisabledPatch
        {
            [HarmonyPostfix]
            public static void Postfix(UnitMovementAgent __instance, ref bool __result)
            {
                var entityData = __instance.Unit?.EntityData;

                if (entityData != null && entityData.IsPlayerFaction) 
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(UnitMovementAgent), "IsSoftObstacle", typeof(UnitMovementAgent))]
        internal static class UnitMovementAgent_IsSoftObstacle_Patch 
        {
            [HarmonyPrefix]
            public static bool Prefix(UnitMovementAgent __instance, ref bool __result) 
            {
                var entityData = __instance.Unit?.EntityData;
                if (entityData != null && entityData.IsPlayerFaction) { return true; }

                __result = !__instance.CombatMode;

                return false;
            }
        }
    }
}
