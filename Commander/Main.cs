using System;
using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem;
using UnityModManagerNet;

namespace Commander
{
    internal static class Main 
    {
        public static ModSettings Settings;
        public static bool Enabled;

        public static bool Load(UnityModManager.ModEntry modEntry) 
        {
            Settings = UnityModManager.ModSettings.Load<ModSettings>(modEntry);
            ModSettings.ModEntry = modEntry;
            modEntry.OnToggle = OnToggle;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll();
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) 
        {
            Enabled = value;
            return true;
        }

        public static void Log(string msg)
        {
            ModSettings.ModEntry.Logger.Log(msg);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(string msg)
        {
            ModSettings.ModEntry.Logger.Log(msg);
        }

        public static void LogPatch(string action, [NotNull] IScriptableObjectWithAssetId bp)
        {
            Log($"{action}: {bp.AssetGuid} - {bp.name}");
        }

        public static void LogHeader(string msg)
        {
            Log($"--{msg.ToUpper()}--");
        }

        public static Exception Error(string message)
        {
            Log(message);
            return new InvalidOperationException(message);
        }

        public static void OnUpdate(UnityModManager.ModEntry modEntry, float z)
        {
            GameSpeedTweaks.OnUpdate();
        }
    }
}
