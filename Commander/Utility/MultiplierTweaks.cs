// ReSharper disable InconsistentNaming

using HarmonyLib;
using Kingmaker.UnitLogic;
using UnityEngine;

namespace Commander.Utility
{
    public static class MultiplierTweaks
    {
        [HarmonyPatch(typeof(EncumbranceHelper), "GetHeavy")]
        internal static class EncumbranceHelperGetHeavyPatch
        {
            internal static void Postfix(ref int __result)
            {
                __result = Mathf.RoundToInt(__result * 6);
            }
        }
    }
}
