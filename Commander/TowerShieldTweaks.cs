// ReSharper disable InconsistentNaming

using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem;

namespace Commander
{
    [HarmonyPatch(typeof(BlueprintsCache), "Load")]
    public static class BlueprintsCacheLoadPatch
    {
        private static BlueprintShieldType _towerShieldType;
        private static BlueprintItemWeaponReference _heavyShieldWeaponRef;

        private static BlueprintShieldType TowerShieldType => _towerShieldType ??= Resources.GetBlueprint<BlueprintShieldType>("5f0f4b6e480e7054b8592b5a8b55854a");

        private static BlueprintItemWeaponReference HeavyShieldWeaponRef => _heavyShieldWeaponRef ??= Resources.GetBlueprint<BlueprintItemWeapon>("ff8047f887565284e93773b4a698c393")
            .ToReference<BlueprintItemWeaponReference>();

        public static void Postfix(ref SimpleBlueprint __result)
        {
            if (__result is BlueprintItemShield shield && shield.Type == TowerShieldType 
                && !string.IsNullOrWhiteSpace(shield.m_DisplayNameText) && !shield.m_WeaponComponent.Equals(HeavyShieldWeaponRef))
            {
                shield.m_WeaponComponent = HeavyShieldWeaponRef;
                Main.Log(shield.m_DisplayNameText);
            }
        }
    }
}
