using System;
using System.Collections.Generic;
using System.Linq;
using Commander.Components;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;

namespace Commander.Archetypes
{
    internal static class DivineSaintArchetype
    {
        public static void Create()
        {
            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>("20ce9bf8af32bee4c8557a045ab499b1");
            var lifeFinalRevelation = Resources.GetBlueprint<BlueprintFeature>("ee23b52c6a06c0b48a09a7a23071aa52");
            var mysterySelection = Resources.GetBlueprint<BlueprintFeatureSelection>("5531b975dcdf0e24c98f1ff7e017e741");

            // Saint Mystery
            // 1 - Remove Sickness
            var removeSickness = Resources.GetBlueprint<BlueprintAbility>("f6f95242abdfac346befd6f4f6222140").ToReference<BlueprintAbilityReference>();
            var barkskin = Resources.GetBlueprint<BlueprintAbility>("5b77d7cc65b8ab74688e74a37fc2f553").ToReference<BlueprintAbilityReference>();
            var heroism = Resources.GetBlueprint<BlueprintAbility>("5ab0d42fb68c9e34abae4921822b9d63").ToReference<BlueprintAbilityReference>();
            var restoration = Resources.GetBlueprint<BlueprintAbility>("f2115ac1148256b4ba20788f7e966830").ToReference<BlueprintAbilityReference>();
            var breadthOfLife = Resources.GetBlueprint<BlueprintAbility>("d5847cad0b0e54c4d82d6c59a3cda6b0").ToReference<BlueprintAbilityReference>();
            var heal = Resources.GetBlueprint<BlueprintAbility>("5da172c4c89f9eb4cbb614f3a67357d3").ToReference<BlueprintAbilityReference>();
            var restorationGreater = Resources.GetBlueprint<BlueprintAbility>("fafd77c6bfa85c04ba31fdc1c962c914").ToReference<BlueprintAbilityReference>();
            var healMass = Resources.GetBlueprint<BlueprintAbility>("867524328b54f25488d371214eea0d90").ToReference<BlueprintAbilityReference>();
            var heroicInvocation = Resources.GetBlueprint<BlueprintAbility>("43740dab07286fe4aa00a6ee104ce7c1").ToReference<BlueprintAbilityReference>();
            
            var spell1 = Helpers.Create<AddKnownSpell>(n =>
            {
                n.SpellLevel = 1;
                n.m_CharacterClass = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Spell = removeSickness;
            });

            // 2 - Barkskin
            var spell2 = Helpers.Create<AddKnownSpell>(n =>
            {
                n.SpellLevel = 2;
                n.m_CharacterClass = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Spell = barkskin;
            });

            // 3 - Heroism
            var spell3 = Helpers.Create<AddKnownSpell>(n =>
            {
                n.SpellLevel = 3;
                n.m_CharacterClass = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Spell = heroism;
            });

            // 4 - Restoration
            var spell4 = Helpers.Create<AddKnownSpell>(n =>
            {
                n.SpellLevel = 4;
                n.m_CharacterClass = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Spell = restoration;
            });

            // 5 - Breadth of Life
            var spell5 = Helpers.Create<AddKnownSpell>(n =>
            {
                n.SpellLevel = 5;
                n.m_CharacterClass = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Spell = breadthOfLife;
            });

            // 6 - Heal
            var spell6 = Helpers.Create<AddKnownSpell>(n =>
            {
                n.SpellLevel = 6;
                n.m_CharacterClass = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Spell = heal;
            });

            // 7 - Restoration, Greater
            var spell7 = Helpers.Create<AddKnownSpell>(n =>
            {
                n.SpellLevel = 7;
                n.m_CharacterClass = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Spell = restorationGreater;
            });

            // 8 - Heal, Mass
            var spell8 = Helpers.Create<AddKnownSpell>(n =>
            {
                n.SpellLevel = 8;
                n.m_CharacterClass = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Spell = healMass;
            });

            // 9 - Heroic Invocation
            var spell9 = Helpers.Create<AddKnownSpell>(n =>
            {
                n.SpellLevel = 9;
                n.m_CharacterClass = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Spell = heroicInvocation;
            });

            var spellList = Helpers.Create<AddSpellsToDescription>(n =>
            {
                n.m_Spells = new[]
                {
                    removeSickness, barkskin, heroism, restoration, breadthOfLife, heal, restorationGreater, healMass,
                    heroicInvocation
                };
            });

            var saintMysterySpells = Helpers.CreateBlueprint<BlueprintFeature>("SaintMysterySpells", "9376ccdc3150488ab7a92ba5c377bd4c", n =>
            {
                n.IsClassFeature = true;
                n.HideInCharacterSheetAndLevelUp = true;
                n.Ranks = 1;
                n.AddComponents(spell1, spell2, spell3, spell4, spell5, spell6, spell7, spell8, spell9, spellList);
            });

            var saintMysterySpellsComp = Helpers.Create<AddFeatureOnClassLevel>(n =>
            {
                n.Level = 2;
                n.m_Class = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Feature = saintMysterySpells.ToReference<BlueprintFeatureReference>();
            });

            var finalRevelationComp = Helpers.Create<AddFeatureOnClassLevel>(n =>
            {
                n.Level = 20;
                n.m_Class = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Feature = lifeFinalRevelation.ToReference<BlueprintFeatureReference>();
            });

            var saintMystery = Helpers.CreateBlueprint<BlueprintFeature>("SaintMystery", "37457aa3c5dc4791a544c66ea832c3b5", n =>
            {
                n.SetName("Saint Mystery");
                n.SetDescription("Oracles who walk the saintly path.");
                n.Groups = new[] {FeatureGroup.OracleMystery};
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.AddComponents(finalRevelationComp, saintMysterySpellsComp);
            });

            // Revelations.
            var saintMysteryRef = saintMystery.ToReference<BlueprintFeatureReference>();

            // Channel
            EnableRevelation(saintMysteryRef, "ade57ae9bbe55c142a012c2453b3088c");
            // Life Link
            EnableRevelation(saintMysteryRef, "ef97c9bcc1c54ea7993ef8b2489c908a");
            // Safe Curing
            EnableRevelation(saintMysteryRef, "3fa75c1a809882a4697db75daf8803e3");
            // Spirit Boost
            EnableRevelation(saintMysteryRef, "8cf1bc6fe4d14304392496ff66023271");
            // War Sight
            EnableRevelation(saintMysteryRef, "84f5169d964185741b97e95a1f1f2a79");
            // Weapon Mastery
            EnableRevelation(saintMysteryRef, "0a4c3556355747241b2d3dcc0a88ec10");
            // Lifesense
            EnableRevelation(saintMysteryRef, "17e537c174c7f0f4c9422c5ab5e3c2b8");
            // Firestorm
            EnableRevelation(saintMysteryRef, "3fdc528f56566984fbbe0baaef0027a2");
            // Firestorm
            EnableRevelation(saintMysteryRef, "973a22b02c793ca49b48652e3d70ae80");
            // BondedMount
            // Animal Companion Selection Change
            var bondedRevelation = Resources.GetBlueprint<BlueprintFeatureSelection>("0234d0dd1cead22428e71a2500afa2e1");
            var prerequisites = bondedRevelation.GetComponent<PrerequisiteFeaturesFromList>();

            if (prerequisites != null)
            {
                var list = new List<BlueprintFeatureReference>(prerequisites.m_Features) {saintMysteryRef};
                prerequisites.m_Features = list.ToArray();
            }

            var leopard = Resources.GetBlueprint<BlueprintFeature>("2ee2ba60850dd064e8b98bf5c2c946ba").ToReference<BlueprintFeatureReference>();
            var smilodon = Resources.GetBlueprint<BlueprintFeature>("126712ef923ab204983d6f107629c895").ToReference<BlueprintFeatureReference>();
            var smilodonPreorder = Resources.GetBlueprint<BlueprintFeature>("44f4d77689434e07a5a44dcb65b25f71").ToReference<BlueprintFeatureReference>();
            var featureList = new List<BlueprintFeatureReference>(bondedRevelation.m_Features) {leopard, smilodon, smilodonPreorder};
            bondedRevelation.m_Features = featureList.ToArray();
            featureList = new List<BlueprintFeatureReference>(bondedRevelation.m_AllFeatures) {leopard, smilodon, smilodonPreorder};
            bondedRevelation.m_AllFeatures = featureList.ToArray();

            // Saint's Intuition
            var replaceAcComp = Helpers.Create<ReplaceStatBaseAttribute>(n =>
            {
                n.TargetStat = StatType.AC;
                n.BaseAttributeReplacement = StatType.Charisma;
            });

            var replacePerceptionComp = Helpers.Create<ReplaceStatBaseAttribute>(n =>
            {
                n.TargetStat = StatType.SkillPerception;
                n.BaseAttributeReplacement = StatType.Charisma;
            });

            var replaceReligionComp = Helpers.Create<ReplaceStatBaseAttribute>(n =>
            {
                n.TargetStat = StatType.SkillLoreReligion;
                n.BaseAttributeReplacement = StatType.Charisma;
            });

            var replaceCmdComp = Helpers.Create<ReplaceCMDDexterityStat>(n =>
            {
                n.NewStat = StatType.Charisma;
            });

            var ignorePenaltiesComp = Helpers.Create<IgnoreArmorPenaltiesComp>(n =>
            {
                n.Categories = new HashSet<ArmorProficiencyGroup>{ArmorProficiencyGroup.Light, ArmorProficiencyGroup.Medium};
            });

            var saintsIntuition = Helpers.CreateBlueprint<BlueprintFeature>("SaintsIntuition", "5d4ff3da5da6401aba8a068d1aa11ad3", n =>
            {
                n.SetName("Saint's Intuition");
                n.SetDescription("You may add your Charisma modifier, instead of your Dexterity modifier, to your Armor Class and CMD. The maximum Dexterity bonus to AC does not apply to you when wearing light armor, medium armor, or wearing a shield. You may also use your Charisma bonus for Perception and Lore Religion checks.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.ReapplyOnLevelUp = true;
                n.AddComponents(replaceAcComp, replacePerceptionComp, replaceReligionComp, replaceCmdComp, ignorePenaltiesComp);
            });

            // Saint's Presence
            var saintsPresenceComp = Helpers.Create<SaintsPresenceComp>();

            var recalculateComp = Helpers.Create<RecalculateOnStatChange>(n =>
            {
                n.Stat = StatType.Charisma;
            });

            var saintsPresence = Helpers.CreateBlueprint<BlueprintFeature>("SaintsPresence", "daeb002e013f4965a4a690bc4e3876e5", n =>
            {
                n.SetName("Saint's Presence");
                n.SetDescription("While wearing medium or light armor, the divine saint can add their charisma bonus to their AC.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.ReapplyOnLevelUp = true;
                n.AddComponents(saintsPresenceComp, recalculateComp);
            });

            // Archetype creation.
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DivineSaintArchetype";
                a.AssetGuid = new BlueprintGuid(new Guid("7510c55390964ebb9f7e212f4674c3eb"));
                a.LocalizedName = Helpers.CreateBlueprintName(a.name, "Divine Saint");
                a.LocalizedDescription = Helpers.CreateBlueprintDescription(a.name, "A divine saint is an oracle who dedicates themselves to the protection of others. Their saintly charisma intimidates their enemies.");
            });

            archetype.ReplaceClassSkills = true;
            archetype.AddSkillPoints = 1;
            archetype.ClassSkills = new[] { StatType.SkillLoreReligion, StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice };
            archetype.AddFeatures = new[] { Helpers.LevelEntry(1, saintsPresence), Helpers.LevelEntry(1, saintsIntuition), Helpers.LevelEntry(1, saintMystery)};
            archetype.RemoveFeatures = new[] { Helpers.LevelEntry(1, mysterySelection)};

            oracle.m_Archetypes = oracle.m_Archetypes.AddToArray(archetype.ToReference<BlueprintArchetypeReference>()).ToArray();
            Resources.AddBlueprint(archetype);
        }

        private static void EnableRevelation(BlueprintFeatureReference mystery, string revelationGuid)
        {
            var revelation = Resources.GetBlueprint<BlueprintFeature>(revelationGuid);
            var prerequisites = revelation.GetComponent<PrerequisiteFeaturesFromList>();

            if (prerequisites == null)
            {
                Main.Log($"ERROR: Could not find prerequisites component for {revelationGuid}");
                return;
            }

            var list = new List<BlueprintFeatureReference>(prerequisites.m_Features) {mystery};
            prerequisites.m_Features = list.ToArray();
        }
    }
}
