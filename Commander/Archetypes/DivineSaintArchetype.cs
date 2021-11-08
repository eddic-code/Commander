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
        private const string PathOfSacrificeT1Desc = "Enemies within 30 feet of you are compelled to attack you instead of your allies. " +
                                                     "You receive a -3 penalty to weapon attack rolls and can no longer fight defensively.";

        private const string PathOfSacrificeT2Desc = "While wearing medium or light armor, you can add your charisma modifier as a bonus to AC.";

        private const string PathOfSacrificeT3Desc = "You become immune to attacks of opportunity.";
        private const string PathOfSacrificeT4Desc = "You gain a +2 dodge bonus to AC.";
        private const string PathOfSacrificeT5Desc = "Enemies affected by Path of Sacrifice now also become shaken.";

        public static void Create()
        {
            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);
            var mysterySelection = Resources.GetBlueprint<BlueprintFeatureSelection>(Guids.OracleMysterySelection);
            var oracleCurseSelection = Resources.GetBlueprint<BlueprintFeatureSelection>(Guids.OracleCurseSelection);
            var oracleAdditionalSpellsSelection = Resources.GetBlueprint<BlueprintFeatureSelection>(Guids.OracleAdditionalSpellsSelection);

            // Saint Mystery
            var saintMystery = CreateSaintMystery();
            var saintMysteryRef = saintMystery.ToReference<BlueprintFeatureReference>();

            // Revelations
            CreateRevelations(saintMysteryRef);

            // Archetype creation.
            var archetype = Helpers.CreateBlueprint<BlueprintArchetype>("DivineSaintArchetype", Guids.DivineSaintArchetype, a =>
            {
                a.SetName("Divine Saint");
                a.SetDescription("A divine saint is an oracle who dedicates herself to the protection of others. Their saintly charisma intimidates their enemies.");
            });

            var archetypeRef = archetype.ToReference<BlueprintArchetypeReference>();
            var pathOfSacrificeT1 = CreatePathOfSacrificeT1(archetypeRef);
            var pathOfSacrificeT2 = CreatePathOfSacrificeT2();

            var levelEntry1 = new LevelEntry
            {
                Level = 1, 
                Features = {CreateGuidedByRevelation(), CreateRelicArmor(), CreateSaintsTouch(), saintMystery, pathOfSacrificeT1}
            };

            var levelEntry2 = new LevelEntry
            {
                Level = 2,
                Features = {pathOfSacrificeT2}
            };

            archetype.ReplaceClassSkills = true;
            archetype.AddSkillPoints = 1;
            archetype.ClassSkills = new[] {StatType.SkillLoreReligion, StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice};
            archetype.AddFeatures = new[] {levelEntry1, levelEntry2};
            archetype.RemoveFeatures = new[] {Helpers.LevelEntry(1, mysterySelection), Helpers.LevelEntry(1, oracleCurseSelection), Helpers.LevelEntry(1, oracleAdditionalSpellsSelection)};

            oracle.m_Archetypes = oracle.m_Archetypes.AddToArray(archetype.ToReference<BlueprintArchetypeReference>()).ToArray();
            Resources.AddBlueprint(archetype);

            var pathOfSacrificeGroup = Helpers.CreateUIGroup(pathOfSacrificeT1, pathOfSacrificeT2);
            oracle.Progression.UIGroups = oracle.Progression.UIGroups.AppendToArray(pathOfSacrificeGroup);
        }

        private static BlueprintFeature CreateRelicArmor()
        {
            var ignorePenaltiesComp = Helpers.Create<IgnoreArmorPenaltiesComp>(n =>
            {
                n.Categories = new HashSet<ArmorProficiencyGroup>{ArmorProficiencyGroup.Light, ArmorProficiencyGroup.Medium};
            });

            var relicArmor = Helpers.CreateBlueprint<BlueprintFeature>("RelicArmor", Guids.RelicArmor, n =>
            {
                n.SetName("Relic Armor");
                n.SetDescription("You can ignore all dexterity bonus caps and speed penalties of light armor, medium armor, light shields, and heavy shields.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.ReapplyOnLevelUp = true;
                n.AddComponents(ignorePenaltiesComp);
            });

            return relicArmor;
        }

        private static BlueprintFeature CreateGuidedByRevelation()
        {
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

            var replaceAcComp = Helpers.Create<ReplaceStatBaseAttribute>(n =>
            {
                n.TargetStat = StatType.AC;
                n.BaseAttributeReplacement = StatType.Charisma;
            });

            var replaceCmdComp = Helpers.Create<ReplaceCMDDexterityStat>(n =>
            {
                n.NewStat = StatType.Charisma;
            });

            var guidedByRevelation = Helpers.CreateBlueprint<BlueprintFeature>("GuidedByRevelation", Guids.GuidedByRevelation, n =>
            {
                n.SetName("Guided By Revelation");
                n.SetDescription("You may add your Charisma modifier, instead of your Dexterity modifier, to your Armor Class and CMD. You may also use your Charisma modifier for Perception and Lore Religion checks.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.ReapplyOnLevelUp = true;
                n.AddComponents(replaceAcComp, replaceCmdComp, replacePerceptionComp, replaceReligionComp);
            });

            return guidedByRevelation;
        }

        private static BlueprintFeature CreateSaintMystery()
        {
            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);
            var lifeFinalRevelation = Resources.GetBlueprint<BlueprintFeature>(Guids.LifeFinalRevelation);
            var removeFear = Resources.GetBlueprint<BlueprintAbility>(Guids.RemoveFear).ToReference<BlueprintAbilityReference>();
            var barkskin = Resources.GetBlueprint<BlueprintAbility>(Guids.Barkskin).ToReference<BlueprintAbilityReference>();
            var heroism = Resources.GetBlueprint<BlueprintAbility>(Guids.Heroism).ToReference<BlueprintAbilityReference>();
            var restoration = Resources.GetBlueprint<BlueprintAbility>(Guids.Restoration).ToReference<BlueprintAbilityReference>();
            var breadthOfLife = Resources.GetBlueprint<BlueprintAbility>(Guids.BreathOfLife).ToReference<BlueprintAbilityReference>();
            var heal = Resources.GetBlueprint<BlueprintAbility>(Guids.Heal).ToReference<BlueprintAbilityReference>();
            var restorationGreater = Resources.GetBlueprint<BlueprintAbility>(Guids.RestorationGreater).ToReference<BlueprintAbilityReference>();
            var healMass = Resources.GetBlueprint<BlueprintAbility>(Guids.HealMass).ToReference<BlueprintAbilityReference>();
            var heroicInvocation = Resources.GetBlueprint<BlueprintAbility>(Guids.HeroicInvocation).ToReference<BlueprintAbilityReference>();

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

            var saintMysterySpells = Helpers.CreateBlueprint<BlueprintFeature>("SaintMysterySpells", Guids.SaintMysterySpells, n =>
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

            var saintMystery = Helpers.CreateBlueprint<BlueprintFeature>("SaintMystery", Guids.SaintMystery, n =>
            {
                n.SetName("Saint Mystery");
                n.SetDescription("Oracles who walk the saintly path.");
                n.Groups = new[] {FeatureGroup.OracleMystery};
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.AddComponents(finalRevelationComp, saintMysterySpellsComp);
            });

            return saintMystery;
        }

        private static void CreateRevelations(BlueprintFeatureReference saintMysteryRef)
        {
            EnableRevelation(saintMysteryRef, Guids.OracleRevelationChannel);
            EnableRevelation(saintMysteryRef, Guids.OracleRevelationSafeCuring);
            EnableRevelation(saintMysteryRef, Guids.OracleRevelationSpiritBoost);
            EnableRevelation(saintMysteryRef, Guids.OracleRevelationWarSight);
            EnableRevelation(saintMysteryRef, Guids.OracleRevelationLifesense);
            EnableRevelation(saintMysteryRef, Guids.OracleRevelationBondedMount);

            var bondedMount = Resources.GetBlueprint<BlueprintFeatureSelection>(Guids.OracleRevelationBondedMount);
            var leopard = Resources.GetBlueprint<BlueprintFeature>(Guids.AnimalCompanionFeatureLeopard).ToReference<BlueprintFeatureReference>();

            var featureList = new List<BlueprintFeatureReference>(bondedMount.m_Features) {leopard};
            bondedMount.m_Features = featureList.ToArray();
            featureList = new List<BlueprintFeatureReference>(bondedMount.m_AllFeatures) {leopard};
            bondedMount.m_AllFeatures = featureList.ToArray();
        }

        private static BlueprintFeature CreateSaintsTouch()
        {
            // Resource
            var resource = Helpers.CreateBlueprint<BlueprintAbilityResource>("SaintsTouchResource", Guids.SaintsTouchResource, n =>
            {
                n.m_MaxAmount = new BlueprintAbilityResource.Amount
                {
                    IncreasedByStat = true,
                    ResourceBonusStat = StatType.Charisma,
                    BaseValue = 2
                };

                n.m_Max = 50;
                n.LocalizedName = new LocalizedString();
                n.LocalizedDescription = new LocalizedString();
            });

            var resouceComp = Helpers.Create<AddAbilityResources>(n =>
            {
                n.m_Resource = resource.ToReference<BlueprintAbilityResourceReference>();
                n.RestoreAmount = true;
            });

            // Ability
            var animation = Resources.GetBlueprint<BlueprintAbility>(Guids.LayOnHandsOthers).GetComponent<AbilitySpawnFx>()?.PrefabLink;
            var icon = Resources.GetBlueprint<BlueprintFeature>(Guids.KiDiamondSoulFeature).m_Icon;

            var spawnFx = Helpers.Create<AbilitySpawnFx>(n =>
            {
                n.PrefabLink = animation;
                n.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
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

            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);

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

            var cureAbility = Helpers.CreateBlueprint<BlueprintAbility>("SaintsTouchAbility", Guids.SaintsTouchAbility, n =>
            {
                n.SetName("Saint's Touch");
                n.SetDescription("Cures ({g|Encyclopedia:Class_Level}class level{/g}/2 + 1){g|Encyclopedia:Dice}d8{/g} points of {g|Encyclopedia:Damage}damage{/g} + 1 point per {g|Encyclopedia:Class_Level}class level{/g}. Can be used a number of times per day equal to 2 plus your charisma modifier.");
                n.CanTargetFriends = true;
                n.CanTargetSelf = true;
                n.Type = AbilityType.Extraordinary;
                n.Range = AbilityRange.Touch;
                n.EffectOnAlly = AbilityEffectOnUnit.Helpful;
                n.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
                n.ActionType = UnitCommand.CommandType.Standard;
                n.AvailableMetamagic = Metamagic.CompletelyNormal | Metamagic.Maximize | Metamagic.Empower | Metamagic.Reach | Metamagic.Quicken;
                n.LocalizedDuration = new LocalizedString();
                n.LocalizedSavingThrow = new LocalizedString();
                n.m_DescriptionShort = new LocalizedString();
                n.m_Icon = icon;
                n.AddComponents(spawnFx, resourceLogicComp, abilityEffectComp, contextRankConfig);
            });

            var cureAbilityRef = cureAbility.ToReference<BlueprintAbilityReference>();
            var abilityComp = Helpers.Create<AddFacts>(n => n.m_Facts = new[] {cureAbility.ToReference<BlueprintUnitFactReference>()});

            var saintsCure = Helpers.CreateBlueprint<BlueprintFeature>("SaintsTouchFeature", Guids.SaintsTouchFeature, n =>
            {
                n.SetName("Saint's Touch");
                n.SetDescription("Cures ({g|Encyclopedia:Class_Level}class level{/g}/2 + 1){g|Encyclopedia:Dice}d8{/g} points of {g|Encyclopedia:Damage}damage{/g} + 1 point per {g|Encyclopedia:Class_Level}class level{/g}. Can be used a number of times per day equal to 2 plus your charisma modifier.");
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(abilityComp, resouceComp);
            });

            // Boundless Healing
            var boundlessHealing = Resources.GetBlueprint<BlueprintFeature>(Guids.BoundlessHealing);
            var autoMetamagic = boundlessHealing.GetComponent<AutoMetamagic>();

            if (autoMetamagic != null)
            {
                autoMetamagic.Abilities = new List<BlueprintAbilityReference>(autoMetamagic.Abilities) {cureAbilityRef};
            }

            // Safe Curing
            var safeCuring = Resources.GetBlueprint<BlueprintFeature>(Guids.OracleRevelationSafeCuring);
            var ignoreComp = safeCuring.GetComponent<IgnoreAttacksOfOpportunityForSpellList>();

            if (ignoreComp != null)
            {
                ignoreComp.m_Abilities = new List<BlueprintAbilityReference>(ignoreComp.m_Abilities) {cureAbilityRef};
            }

            return saintsCure;
        }

        private static BlueprintFeature CreatePathOfSacrificeT1(BlueprintArchetypeReference divineSaintArchetypeRef)
        {
            var icon = Resources.GetBlueprint<BlueprintFeature>(Guids.AuraOfRighteousness).m_Icon;

            var debuff = Helpers.CreateBuff("PathOfSacrificeDebuffT1", Guids.PathOfSacrificeDebuffT1, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription("This target is compelled to attack the divine saint.");
                n.m_Flags = BlueprintBuff.Flags.Harmful;
                n.Stacking = StackingType.Replace;
            });

            var debuffAreaEffect = Helpers.Create<AbilityAreaEffectBuff>(n =>
            {
                n.m_Buff = debuff.ToReference<BlueprintBuffReference>();
                n.Condition = new ConditionsChecker {Conditions = new Condition[] {new ContextConditionIsEnemy()}};
            });

            var debuffArea = Helpers.CreateBlueprint<BlueprintAbilityAreaEffect>("PathOfSacrificeDebuffAreaT1", Guids.PathOfSacrificeDebuffAreaT1, n =>
            {
                n.AffectEnemies = true;
                n.AggroEnemies = false;
                n.Shape = AreaEffectShape.Cylinder;
                n.Size = new Feet(30);
                n.AddComponents(debuffAreaEffect);
            });

            var areaComp = Helpers.Create<AddAreaEffect>(c =>
                c.m_AreaEffect = debuffArea.ToReference<BlueprintAbilityAreaEffectReference>());

            var penaltyComp = Helpers.Create<WeaponMultipleCategoriesAttackBonus>(n =>
            {
                n.Categories = new[] {WeaponCategory.Touch, WeaponCategory.Ray};
                n.AttackBonus = -3;
                n.ExceptForCategories = true;
                n.Descriptor = ModifierDescriptor.UntypedStackable;
            });

            var buff = Helpers.CreateBuff("PathOfSacrificeBuffT1", Guids.PathOfSacrificeBuffT1, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT1Desc);
                n.m_Flags = BlueprintBuff.Flags.StayOnDeath;
                n.m_Icon = icon;
                n.AddComponents(areaComp, penaltyComp);
            });

            var priorityTargetComp = Helpers.Create<PriorityTarget>(c =>
                c.PriorityFact = buff.ToReference<BlueprintUnitFactReference>());

            debuff.AddComponent(priorityTargetComp);

            var recalculateComp = Helpers.Create<RecalculateOnStatChange>(n =>
            {
                n.Stat = StatType.Charisma;
            });

            var saintsPresenceComp = Helpers.Create<SaintsPresenceComp>();

            var acFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeAc", Guids.PathOfSacrificeAc, n =>
            {
                n.SetName("Path of Sacrifice - AC Bonus");
                n.SetDescription("While wearing medium or light armor, you can add your charisma modifier as a bonus to AC.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.ReapplyOnLevelUp = true;
                n.m_Icon = icon;
                n.AddComponents(saintsPresenceComp, recalculateComp);
            });

            var feature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeT1", Guids.PathOfSacrificeT1, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT1Desc);
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(Helpers.Create<AddFacts>(c => c.m_Facts = new[]{buff.ToReference<BlueprintUnitFactReference>(), acFeature.ToReference<BlueprintUnitFactReference>()}));
            });

            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);

            var mainFeatureComp = Helpers.Create<AddFeatureOnClassLevel>(n =>
            {
                n.m_Class = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Archetypes = new[] {divineSaintArchetypeRef};
                n.m_Feature = feature.ToReference<BlueprintFeatureReference>();
                n.Level = 7;
                n.BeforeThisLevel = true;
            });

            var mainFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeMainT1", Guids.PathOfSacrificeMainT1, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT1Desc);
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(mainFeatureComp);
            });

            return mainFeature;
        }

        private static BlueprintFeature CreatePathOfSacrificeT2()
        {
            var icon = Resources.GetBlueprint<BlueprintFeature>(Guids.AuraOfRighteousness).m_Icon;

            var recalculateComp = Helpers.Create<RecalculateOnStatChange>(n =>
            {
                n.Stat = StatType.Charisma;
            });

            var saintsPresenceComp = Helpers.Create<SaintsPresenceComp>();

            var acFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeAc", Guids.PathOfSacrificeAc, n =>
            {
                n.SetName("Path of Sacrifice - AC Bonus");
                n.SetDescription("While wearing medium or light armor, you can add your charisma modifier as a bonus to AC.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.ReapplyOnLevelUp = true;
                n.m_Icon = icon;
                n.AddComponents(saintsPresenceComp, recalculateComp);
            });

            return acFeature;
        }

        private static BlueprintFeature CreatePathOfSacrificeT3(BlueprintArchetypeReference divineSaintArchetypeRef)
        {
            var icon = Resources.GetBlueprint<BlueprintFeature>(Guids.AuraOfRighteousness).m_Icon;

            var debuff = Helpers.CreateBuff("PathOfSacrificeDebuffT2", Guids.PathOfSacrificeDebuffT2, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription("This target is compelled to attack the divine saint.");
                n.m_Flags = BlueprintBuff.Flags.Harmful;
                n.Stacking = StackingType.Replace;
            });

            var debuffAreaEffect = Helpers.Create<AbilityAreaEffectBuff>(n =>
            {
                n.m_Buff = debuff.ToReference<BlueprintBuffReference>();
                n.Condition = new ConditionsChecker {Conditions = new Condition[] {new ContextConditionIsEnemy()}};
            });

            var debuffArea = Helpers.CreateBlueprint<BlueprintAbilityAreaEffect>("PathOfSacrificeDebuffAreaT2", Guids.PathOfSacrificeDebuffAreaT2, n =>
            {
                n.AffectEnemies = true;
                n.AggroEnemies = false;
                n.Shape = AreaEffectShape.Cylinder;
                n.Size = new Feet(30);
                n.AddComponents(debuffAreaEffect);
            });

            var areaComp = Helpers.Create<AddAreaEffect>(c =>
                c.m_AreaEffect = debuffArea.ToReference<BlueprintAbilityAreaEffectReference>());

            var penaltyComp = Helpers.Create<WeaponMultipleCategoriesAttackBonus>(n =>
            {
                n.Categories = new[] {WeaponCategory.Touch, WeaponCategory.Ray};
                n.AttackBonus = -3;
                n.ExceptForCategories = true;
                n.Descriptor = ModifierDescriptor.UntypedStackable;
            });

            var buff = Helpers.CreateBuff("PathOfSacrificeBuffT2", Guids.PathOfSacrificeBuffT2, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT2Desc);
                n.m_Flags = BlueprintBuff.Flags.StayOnDeath;
                n.m_Icon = icon;
                n.AddComponents(areaComp, penaltyComp);
            });

            var priorityTargetComp = Helpers.Create<PriorityTarget>(c =>
                c.PriorityFact = buff.ToReference<BlueprintUnitFactReference>());

            debuff.AddComponent(priorityTargetComp);

            var recalculateComp = Helpers.Create<RecalculateOnStatChange>(n =>
            {
                n.Stat = StatType.Charisma;
            });

            var saintsPresenceComp = Helpers.Create<SaintsPresenceComp>();

            var acFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeAc", Guids.PathOfSacrificeAc, n =>
            {
                n.SetName("Path of Sacrifice - AC Bonus");
                n.SetDescription("While wearing medium or light armor, you can add your charisma modifier as a bonus to AC.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.ReapplyOnLevelUp = true;
                n.m_Icon = icon;
                n.AddComponents(saintsPresenceComp, recalculateComp);
            });

            var feature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeT2", Guids.PathOfSacrificeT2, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT2Desc);
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(Helpers.Create<AddFacts>(c => c.m_Facts = new[]{buff.ToReference<BlueprintUnitFactReference>(), acFeature.ToReference<BlueprintUnitFactReference>()}));
            });

            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);

            var mainFeatureComp = Helpers.Create<AddFeatureOnClassLevel>(n =>
            {
                n.m_Class = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Archetypes = new[] {divineSaintArchetypeRef};
                n.m_Feature = feature.ToReference<BlueprintFeatureReference>();
                n.Level = 11;
                n.BeforeThisLevel = true;
            });

            var mainFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeMainT2", Guids.PathOfSacrificeMainT2, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT2Desc);
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(mainFeatureComp);
            });

            return mainFeature;
        }

        private static BlueprintFeature CreateTogglePathOfSacrifice()
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

            var areaComp = Helpers.Create<AddAreaEffect>(c =>
                c.m_AreaEffect = debuffArea.ToReference<BlueprintAbilityAreaEffectReference>());

            var buff = Helpers.CreateBuff("OathOfSacrificeBuff", "996ad375ab424020b588dd005b248a69", n =>
            {
                n.SetName("Oath of Sacrifice");
                n.SetDescription("Enemies within 30 feet are compelled to attack this target instead of others.");
                n.m_Flags = BlueprintBuff.Flags.StayOnDeath;
                n.m_Icon = icon;
                n.AddComponents(areaComp);
            });

            var priorityTargetComp = Helpers.Create<PriorityTarget>(c =>
                c.PriorityFact = buff.ToReference<BlueprintUnitFactReference>());

            debuff.AddComponent(priorityTargetComp);

            var ability = Helpers.CreateBlueprint<BlueprintActivatableAbility>("PathOfSacrificeAbility", "2307580c4d0b40bdab600df34fb2833a", n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription("Enemies within 30 feet of you are compelled to attack you instead of your allies.");
                n.DeactivateIfCombatEnded = false;
                n.DeactivateIfOwnerDisabled = false;
                n.DeactivateIfOwnerUnconscious = false;
                n.ActivationType = AbilityActivationType.Immediately;
                n.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;
                n.m_Icon = icon;
                n.m_Buff = buff.ToReference<BlueprintBuffReference>();
            });

            var abilityFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrifice", "40ceba7923fc4b73af0e7228b977bc66", n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription("Enemies within 30 feet of you are compelled to attack you instead of your allies.");
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
