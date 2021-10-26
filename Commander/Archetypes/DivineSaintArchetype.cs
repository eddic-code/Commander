using System;
using System.Collections.Generic;
using System.Linq;
using Commander.Components;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;

namespace Commander.Archetypes
{
    internal static class DivineSaintArchetype
    {
        public static void Create()
        {
            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>("20ce9bf8af32bee4c8557a045ab499b1");
            var lifeFinalRevelation = Resources.GetBlueprint<BlueprintFeature>("ee23b52c6a06c0b48a09a7a23071aa52");
            var mysterySelection = Resources.GetBlueprint<BlueprintFeatureSelection>("5531b975dcdf0e24c98f1ff7e017e741");
            var oracleCurseSelection = Resources.GetBlueprint<BlueprintFeatureSelection>("b0a5118b4fb793241bc7042464b23fab");
            var oracleAdditionalSpellsSelection = Resources.GetBlueprint<BlueprintFeatureSelection>("2cd080fc181122c4a9c5a705abe8ad47");

            // Saint Mystery
            // 1 - Remove Sickness
            var removeFear = Resources.GetBlueprint<BlueprintAbility>("55a037e514c0ee14a8e3ed14b47061de").ToReference<BlueprintAbilityReference>();
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
                n.m_Spell = removeFear;
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
                    removeFear, barkskin, heroism, restoration, breadthOfLife, heal, restorationGreater, healMass,
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

            var saintsCure = CreateEnlight();
            var oathOfSacrifice = CreateOathOfSacrifice();
             
            archetype.ReplaceClassSkills = true;
            archetype.AddSkillPoints = 1;
            archetype.ClassSkills = new[] { StatType.SkillLoreReligion, StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice };
            archetype.AddFeatures = new[] { Helpers.LevelEntry(1, saintsPresence), Helpers.LevelEntry(1, saintsIntuition), Helpers.LevelEntry(1, saintMystery), Helpers.LevelEntry(2, saintsCure), Helpers.LevelEntry(1, oathOfSacrifice)};
            archetype.RemoveFeatures = new[] { Helpers.LevelEntry(1, mysterySelection), Helpers.LevelEntry(1, oracleCurseSelection), Helpers.LevelEntry(1, oracleAdditionalSpellsSelection)};

            oracle.m_Archetypes = oracle.m_Archetypes.AddToArray(archetype.ToReference<BlueprintArchetypeReference>()).ToArray();
            Resources.AddBlueprint(archetype);
        }

        private static BlueprintFeature CreateEnlight()
        {
            // Resource
            var resource = Helpers.CreateBlueprint<BlueprintAbilityResource>("DivineSaintEnlightResource", "4f087da382e148d6a5e09e798ccabd54", n =>
            {
                n.m_MaxAmount = new BlueprintAbilityResource.Amount
                {
                    IncreasedByStat = true,
                    ResourceBonusStat = StatType.Charisma,
                    BaseValue = 4
                };

                n.m_Max = 30;
                n.LocalizedName = new LocalizedString();
                n.LocalizedDescription = new LocalizedString();
            });

            var resouceComp = Helpers.Create<AddAbilityResources>(n =>
            {
                n.m_Resource = resource.ToReference<BlueprintAbilityResourceReference>();
                n.RestoreAmount = true;
            });

            // Ability
            var cureLightWounds = Resources.GetBlueprint<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f");
            var icon = Resources.GetBlueprint<BlueprintFeature>("01182bcee8cb41640b7fa1b1ad772421").m_Icon;

            var spawnFx = Helpers.Create<AbilitySpawnFx>(n =>
            {
                n.PrefabLink = cureLightWounds.GetComponent<AbilitySpawnFx>()?.PrefabLink;
                n.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                n.DestroyOnCast = true;
                n.PositionAnchor = AbilitySpawnFxAnchor.None;
                n.OrientationAnchor = AbilitySpawnFxAnchor.None;
            });

            var resourceLogicComp = Helpers.Create<AbilityResourceLogic>(n =>
            {
                n.m_RequiredResource = resource.ToReference<BlueprintAbilityResourceReference>();
                n.m_IsSpendResource = true;
                n.Amount = 1;
            });

            var healValue = new ContextDiceValue
            {
                DiceCountValue = new ContextValue {ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.DamageDice}, 
                DiceType = DiceType.D8,
                BonusValue = new ContextValue {ValueType = ContextValueType.Rank}
            };

            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>("20ce9bf8af32bee4c8557a045ab499b1");

            var contextRankConfig = Helpers.Create<ContextRankConfig>(n =>
            {
                n.m_Progression = ContextRankProgression.OnePlusDiv2;
                n.m_BaseValueType = ContextRankBaseValueType.ClassLevel;
                n.m_Type = AbilityRankType.DamageDice;
                n.m_Class = new[]{oracle.ToReference<BlueprintCharacterClassReference>()};
            });

            var healAction = Helpers.Create<ContextActionHealTarget>(n =>
            {
                n.Value = healValue;
            });

            var abilityEffectComp = Helpers.Create<AbilityEffectRunAction>(n =>
            {
                n.AddAction(healAction);
            });

            var cureAbility = Helpers.CreateBlueprint<BlueprintAbility>("DivineSaintEnlightAbility", "0c15d594bd19418f92e8a2804031b557", n =>
            {
                n.SetName("Enlight");
                n.SetDescription("You bestow miraculous light to the target that cures ({g|Encyclopedia:Class_Level}class level{/g}/2 + 1){g|Encyclopedia:Dice}d8{/g} points of {g|Encyclopedia:Damage}damage{/g} + 1 point per {g|Encyclopedia:Class_Level}class level{/g}.");
                n.CanTargetFriends = true;
                n.CanTargetSelf = true;
                n.Range = AbilityRange.Medium;
                n.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
                n.ActionType = UnitCommand.CommandType.Swift;
                n.AvailableMetamagic = Metamagic.CompletelyNormal | Metamagic.Maximize;
                n.LocalizedDuration = new LocalizedString();
                n.LocalizedSavingThrow = new LocalizedString();
                n.m_DescriptionShort = new LocalizedString();
                n.m_Icon = icon;
                n.AddComponents(spawnFx, resourceLogicComp, abilityEffectComp, contextRankConfig);
            });

            var abilityComp = Helpers.Create<AddFacts>(n => n.m_Facts = new[] {cureAbility.ToReference<BlueprintUnitFactReference>()});

            var saintsCure = Helpers.CreateBlueprint<BlueprintFeature>("DivineSaintEnlightFeature", "90ce1d39fc1c45dd8b6d5e6cc026f344", n =>
            {
                n.SetName("Enlight");
                n.SetDescription("You bestow miraculous light to the target that cures ({g|Encyclopedia:Class_Level}class level{/g}/2 + 1){g|Encyclopedia:Dice}d8{/g} points of {g|Encyclopedia:Damage}damage{/g} + 1 point per {g|Encyclopedia:Class_Level}class level{/g}.");
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(abilityComp, resouceComp);
            });

            return saintsCure;
        }

        private static BlueprintFeature CreateOathOfSacrifice()
        {
            var icon = Resources.GetBlueprint<BlueprintFeature>("6bd4a71232014254e80726f3a3756962").m_Icon;
            var areaFx = Resources.GetBlueprint<BlueprintAbilityAreaEffect>("1be964f750eea8748a76e92744746efb").Fx;
            var debuffStartFx = Resources.GetBlueprint<BlueprintBuff>("b6570b8cbb32eaf4ca8255d0ec3310b0").FxOnStart;

            var debuff = Helpers.CreateBuff("OathOfSacrificeDebuff", "b6d154805fed4771b8c4c7f027ec742a", n =>
            {
                n.SetName("Oath of Sacrifice");
                n.SetDescription("This target is compelled to attack the divine saint.");
                n.m_Flags = BlueprintBuff.Flags.Harmful;
                n.Stacking = StackingType.Replace;
                n.FxOnStart = debuffStartFx;
            });

            var debuffAreaEffect = Helpers.Create<AbilityAreaEffectBuff>(n =>
            {
                n.m_Buff = debuff.ToReference<BlueprintBuffReference>();
                n.Condition = new ConditionsChecker {Conditions = new Condition[] {new ContextConditionIsEnemy()}};
            });

            var debuffArea = Helpers.CreateBlueprint<BlueprintAbilityAreaEffect>("OathOfSacrificeDebuffArea", "0603057c29db4cb3bee9fd8081f48e72", n =>
            {
                n.AffectEnemies = true;
                n.AggroEnemies = false;
                n.Shape = AreaEffectShape.Cylinder;
                n.Size = new Feet(30);
                n.Fx = areaFx;
                n.AddComponents(debuffAreaEffect);
            });

            var buff = Helpers.CreateBuff("OathOfSacrificeBuff", "996ad375ab424020b588dd005b248a69", n =>
            {
                n.SetName("Oath of Sacrifice");
                n.SetDescription("Enemies within 20 feet are compelled to attack this target instead of others.");
                n.m_Flags = BlueprintBuff.Flags.StayOnDeath;
                n.m_Icon = icon;
                n.AddComponents(Helpers.Create<AddAreaEffect>(c => c.m_AreaEffect = debuffArea.ToReference<BlueprintAbilityAreaEffectReference>()));
            });

            var priorityTargetComp = Helpers.Create<PriorityTarget>(c =>
                c.PriorityFact = buff.ToReference<BlueprintUnitFactReference>());

            debuff.AddComponent(priorityTargetComp);

            var ability = Helpers.CreateBlueprint<BlueprintActivatableAbility>("OathOfSacrificeAbility", "2307580c4d0b40bdab600df34fb2833a", n =>
            {
                n.SetName("Oath of Sacrifice");
                n.SetDescription("Enemies within 20 feet of the saint become compelled to attack her instead of her allies.");
                n.DeactivateIfCombatEnded = false;
                n.DeactivateIfOwnerDisabled = false;
                n.DeactivateIfOwnerUnconscious = false;
                n.ActivationType = AbilityActivationType.Immediately;
                n.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;
                n.m_Icon = icon;
                n.m_Buff = buff.ToReference<BlueprintBuffReference>();
            });

            var abilityFeature = Helpers.CreateBlueprint<BlueprintFeature>("OathOfSacrifice", "40ceba7923fc4b73af0e7228b977bc66", n =>
            {
                n.SetName("Oath of Sacrifice");
                n.SetDescription("Enemies within 20 feet of the saint become compelled to attack her instead of her allies.");
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(Helpers.Create<AddFacts>(c => c.m_Facts = new[]{ability.ToReference<BlueprintUnitFactReference>()}));
            });

            return abilityFeature;
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
