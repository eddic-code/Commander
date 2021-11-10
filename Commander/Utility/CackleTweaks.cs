using System;
using HarmonyLib;
using Kingmaker;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Commander.Utility
{
    [HarmonyPatch(typeof(ContextActionReduceBuffDuration), nameof(ContextActionReduceBuffDuration.RunAction))]
    public static class ContextActionReduceBuffDurationRunActionPatch 
    {
        public static bool Prefix(ContextActionReduceBuffDuration __instance)
        {
            var name = __instance.TargetBuff.name;
            if (!Game.Instance.Player.IsInCombat && (name.StartsWith("WitchHex") || name.StartsWith("ShamanHex"))) 
            {
                __instance.Target.Unit?.Buffs.GetBuff(__instance.TargetBuff).IncreaseDuration(new TimeSpan(2, 0, 0));
                return false;
            }

            return true;
        }
    }
}
