// ReSharper disable InconsistentNaming

using System;
using HarmonyLib;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace Commander.Utility
{
    [HarmonyPatch(typeof(RuleRollDice), "Roll")]
    public static class RollTweaks
    {
        private static void Postfix(RuleRollDice __instance)
        {
            if (__instance.DiceFormula.Dice != DiceType.D20) { return; }

            var initiator = __instance.Initiator;
            if (!initiator.IsPlayerFaction || initiator.IsInCombat) { return; }

            __instance.m_Result = Math.Max(10, __instance.Result);
        }
    }
}
