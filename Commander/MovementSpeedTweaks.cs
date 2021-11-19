// ReSharper disable InconsistentNaming

using System;
using HarmonyLib;
using Kingmaker.Armies;
using Kingmaker.Assets.Controllers.GlobalMap;
using Kingmaker.Globalmap.State;
using Kingmaker.Globalmap.View;
using UnityEngine;

namespace Commander
{
    [HarmonyPatch(typeof(GlobalMapMovementController), "GetRegionalModifier", new Type[] { })]
        public static class MovementSpeedGetRegionalModifierPatch1 
        {
            public static void Postfix(ref float __result) 
            {
                var speedMultiplier = Mathf.Clamp(1.3f, 0.1f, 100f);
                __result = speedMultiplier * __result;
            }
        }

        [HarmonyPatch(typeof(GlobalMapMovementController), "GetRegionalModifier", typeof(Vector3))]
        public static class MovementSpeedGetRegionalModifierPatch2 
        {
            public static void Postfix(ref float __result) 
            {
                var speedMultiplier = Mathf.Clamp(1.3f, 0.1f, 100f);
                __result = speedMultiplier * __result;
            }
        }

        [HarmonyPatch(typeof(GlobalMapMovementUtility), "MoveAlongEdge", typeof(GlobalMapState), typeof(GlobalMapView), typeof(IGlobalMapTraveler), typeof(float))]
        public static class GlobalMapMovementUtilityMoveAlongEdgePatch 
        {
            public static void Prefix(GlobalMapState state, GlobalMapView view, IGlobalMapTraveler traveler, ref float visualStepDistance) {

                if (traveler is GlobalMapArmyState armyState && armyState.Data.Faction == ArmyFaction.Crusaders) 
                {
                    var speedMultiplier = Mathf.Clamp(1.3f, 0.1f, 100f);
                    visualStepDistance = speedMultiplier * visualStepDistance;
                }
            }
        }

        [HarmonyPatch(typeof(GlobalMapArmyState), "SpendMovementPoints", typeof(float))]
        public static class GlobalMapArmyStateSpendMovementPointsPatch 
        {
            public static void Prefix(GlobalMapArmyState __instance, ref float points)
            {
                if (__instance.Data.Faction != ArmyFaction.Crusaders) { return; }

                var speedMultiplier = Mathf.Clamp(1.3f, 0.1f, 100f);
                points /= speedMultiplier;
            }
        }
}
