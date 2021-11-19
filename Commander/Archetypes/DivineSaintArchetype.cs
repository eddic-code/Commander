using System.Collections.Generic;
using System.Linq;
using Commander.Components;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
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
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;

namespace Commander.Archetypes
{
    internal static class DivineSaintArchetype
    {
        private const string PathOfSacrificeT1Desc = "Enemies within 20 feet of you are compelled to attack you instead of your allies. " +
                                                     "You receive a -4 penalty to weapon attack rolls and can no longer fight defensively. " +
                                                     "You can add your charisma modifier as a bonus to AC while wearing medium or light armor." +
                                                     "\nAt 5th level, you become immune to attacks of opportunity." +
                                                     "\nAt 9th level, you gain a +20 bonus to hit points." +
                                                     "\nAt 13th level, enemies affected by Path of Sacrifice now also become shaken.";

        public static void Create()
        {
            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);
            var mysterySelection = Resources.GetBlueprint<BlueprintFeatureSelection>(Guids.OracleMysterySelection);
            var oracleCurseSelection = Resources.GetBlueprint<BlueprintFeatureSelection>(Guids.OracleCurseSelection);
            var oracleAdditionalSpellsSelection = Resources.GetBlueprint<BlueprintFeatureSelection>(Guids.OracleAdditionalSpellsSelection);

            // Skills
            var saintsTouch = CreateSaintsTouch();

            // Saint Mystery
            var saintMystery = CreateSaintMystery();
            var saintMysteryRef = saintMystery.ToReference<BlueprintFeatureReference>();

            // Revelations
            var revelationSelection = Resources.GetBlueprint<BlueprintFeatureSelection>("60008a10ad7ad6543b1f63016741a5d2");

            CreateRevelations(saintMysteryRef);
            AddToSelection(revelationSelection, CreateClemency(saintMysteryRef));
            AddToSelection(revelationSelection, CreateAtonement(saintMysteryRef));
            AddToSelection(revelationSelection, CreateAsylum(saintMysteryRef));
            AddToSelection(revelationSelection, CreatePenance(saintMysteryRef));
            AddToSelection(revelationSelection, CreateAbsolution(saintMysteryRef));
            AddToSelection(revelationSelection, CreateLuminositeEternelle(saintMysteryRef));
            AddToSelection(revelationSelection, CreateAegis(saintMysteryRef));

            // Archetype creation.
            var archetype = Helpers.CreateBlueprint<BlueprintArchetype>("DivineSaintArchetype", Guids.DivineSaintArchetype, a =>
            {
                a.SetName("Divine Saint");
                a.SetDescription("A divine saint is an oracle who dedicates herself to the protection of others. Their saintly charisma intimidates their enemies.");
            });

            var archetypeRef = archetype.ToReference<BlueprintArchetypeReference>();
            var pathOfSacrificeT1 = CreatePathOfSacrificeT1(archetypeRef);
            var pathOfSacrificeT2 = CreatePathOfSacrificeT2();
            var pathOfSacrificeT3 = CreatePathOfSacrificeT3(archetypeRef);
            var pathOfSacrificeT4 = CreatePathOfSacrificeT4(archetypeRef);
            var pathOfSacrificeT5 = CreatePathOfSacrificeT5();

            var levelEntry1 = new LevelEntry
            {
                Level = 1, 
                Features = {CreateGuidedByRevelation(), saintMystery, pathOfSacrificeT1}
            };

            var levelEntry2 = new LevelEntry
            {
                Level = 2,
                Features = {pathOfSacrificeT2, CreateRelicArmor(), saintsTouch}
            };

            var levelEntry5 = new LevelEntry
            {
                Level = 5,
                Features = {pathOfSacrificeT3}
            };

            var levelEntry9 = new LevelEntry
            {
                Level = 9,
                Features = {pathOfSacrificeT4}
            };

            var levelEntry13 = new LevelEntry
            {
                Level = 13,
                Features = {pathOfSacrificeT5}
            };

            archetype.ReplaceClassSkills = true;
            archetype.AddSkillPoints = 1;
            archetype.ClassSkills = new[] {StatType.SkillLoreReligion, StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice};
            archetype.AddFeatures = new[] {levelEntry1, levelEntry2, levelEntry5, levelEntry9, levelEntry13};
            archetype.RemoveFeatures = new[] {Helpers.LevelEntry(1, mysterySelection), Helpers.LevelEntry(1, oracleCurseSelection), Helpers.LevelEntry(1, oracleAdditionalSpellsSelection)};

            oracle.m_Archetypes = oracle.m_Archetypes.AddToArray(archetype.ToReference<BlueprintArchetypeReference>()).ToArray();

            var pathOfSacrificeGroup = Helpers.CreateUIGroup(pathOfSacrificeT1, pathOfSacrificeT2, pathOfSacrificeT3, pathOfSacrificeT4, pathOfSacrificeT5);
            oracle.Progression.UIGroups = oracle.Progression.UIGroups.AppendToArray(pathOfSacrificeGroup);
        }

        private static BlueprintFeature CreateAegis(BlueprintFeatureReference mystery)
        {
            var prerequisites = Helpers.Create<PrerequisiteFeaturesFromList>(n =>
            {
                n.m_Features = new[] {mystery};
            });

            var clemency = Helpers.CreateBlueprint<BlueprintFeature>("Aegis", Guids.Aegis, n =>
            {
                n.SetName("Aegis");
                n.SetDescription("All damage received is reduced by 25%.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.Groups = new[] {FeatureGroup.OracleRevelation};
                n.AddComponents(prerequisites, new AegisComp());
            });

            return clemency;
        }

        private static BlueprintFeature CreateAbsolution(BlueprintFeatureReference mystery)
        {
            var absolutionComp = new AbsolutionComp();

            var prerequisites = Helpers.Create<PrerequisiteFeaturesFromList>(n =>
            {
                n.m_Features = new[] {mystery};
            });

            var clemency = Helpers.CreateBlueprint<BlueprintFeature>("Absolution", Guids.Absolution, n =>
            {
                n.SetName("Absolution");
                n.SetDescription("Whenever you attack, even if you miss, you heal for 5% of your maximum hit points.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.Groups = new[] {FeatureGroup.OracleRevelation};
                n.AddComponents(prerequisites, absolutionComp);
            });

            return clemency;
        }

        private static BlueprintFeature CreatePenance(BlueprintFeatureReference mystery)
        {
            var bonus = new DispelCasterLevelCheckBonus
            {
                Value = new ContextValue {ValueType = ContextValueType.Simple, Value = 4}
            };

            var dispelMagicTarget = Resources.GetBlueprint<BlueprintAbility>("143775c49ae6b7446b805d3b2e702298")
                .ToReference<BlueprintAbilityReference>();

            var dispelMagicPoint = Resources.GetBlueprint<BlueprintAbility>("9f6daa93291737c40b8a432c374226a7")
                .ToReference<BlueprintAbilityReference>();

            var autoQuicken = new AutoMetamagic
            {
                Abilities = new List<BlueprintAbilityReference>{dispelMagicTarget, dispelMagicPoint},
                Metamagic = Metamagic.Quicken,
                m_AllowedAbilities = AutoMetamagic.AllowedType.Any
            };

            var prerequisites = Helpers.Create<PrerequisiteFeaturesFromList>(n =>
            {
                n.m_Features = new[] {mystery};
            });

            var icon = Resources.GetBlueprint<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a").m_Icon;

            var buff = Helpers.CreateBuff("PenanceBuff", Guids.PenanceBuff, n =>
            {
                n.SetName("Penance");
                n.SetDescription("Gain a +4 bonus to Dispel Magic and Greater Dispel Magic checks. Dispel Magic can now be cast as a swift action.");
                n.m_Flags = BlueprintBuff.Flags.StayOnDeath;
                n.m_Icon = icon;
                n.Stacking = StackingType.Ignore;
                n.AddComponents(bonus);
            });

            var addFact = Helpers.Create<AddFacts>(c => c.m_Facts = new[]{buff.ToReference<BlueprintUnitFactReference>()});

            var clemency = Helpers.CreateBlueprint<BlueprintFeature>("Penance", Guids.Penance, n =>
            {
                n.SetName("Penance");
                n.SetDescription("Gain a +4 bonus to Dispel Magic and Greater Dispel Magic checks. Dispel Magic can now be cast as a swift action.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.Groups = new[] {FeatureGroup.OracleRevelation};
                n.AddComponents(prerequisites, addFact, autoQuicken);
            });

            return clemency;
        }

        private static BlueprintFeature CreateAsylum(BlueprintFeatureReference mystery)
        {
            // Hidden Trigger Buff
            var comp = new AsylumBuffComp();

            var hiddenBuff = Helpers.CreateBlueprint<BlueprintBuff>("AsylumHiddenBuff", Guids.AsylumHiddenBuff, n =>
            {
                n.SetName("AsylumHiddenBuff");
                n.SetDescription("");
                n.m_Flags = BlueprintBuff.Flags.StayOnDeath | BlueprintBuff.Flags.HiddenInUi;
                n.AddComponents(comp);
            });

            // Resource
            var resource = Helpers.CreateBlueprint<BlueprintAbilityResource>("AsylumResource", Guids.AsylumResource, n =>
            {
                n.m_MaxAmount = new BlueprintAbilityResource.Amount
                {
                    IncreasedByStat = false,
                    BaseValue = 3
                };

                n.m_Max = 3;
                n.LocalizedName = new LocalizedString();
                n.LocalizedDescription = new LocalizedString();
            });

            var resourceLogic = new ActivatableAbilityResourceLogic
            {
                SpendType = ActivatableAbilityResourceLogic.ResourceSpendType.Never,
                m_RequiredResource = resource.ToReference<BlueprintAbilityResourceReference>(),
            };

            var resouceComp = Helpers.Create<AddAbilityResources>(n =>
            {
                n.m_Resource = resource.ToReference<BlueprintAbilityResourceReference>();
                n.RestoreAmount = true;
            });

            // Asylum Buff
            var startFx = Resources.GetBlueprint<BlueprintBuff>("b6570b8cbb32eaf4ca8255d0ec3310b0").FxOnStart;
            var icon = Resources.GetBlueprint<BlueprintFeature>(Guids.InvulnerableRagerDamageReduction).m_Icon;

            var customUnitProperty = Helpers.CreateBlueprint<BlueprintUnitProperty>("AsylumProp", "77b854af82a6486494bb568e98915c7a", n =>
            {
                n.AddComponents(new AsylumPropertyGetter());
            });

            var drComp = new AddDamageResistancePhysical
            {
                Alignment = DamageAlignment.Good, 
                Material = PhysicalDamageMaterial.Adamantite,
                Reality = DamageRealityType.Ghost,
                Value = new ContextValue{ValueType = ContextValueType.CasterCustomProperty, m_CustomProperty = customUnitProperty.ToReference<BlueprintUnitPropertyReference>()}
            };

            Helpers.CreateBlueprint<BlueprintBuff>("AsylumDefensiveBuff", Guids.AsylumDefensiveBuff, n =>
            {
                n.SetName("Asylum");
                n.SetDescription("Gain an amount of DR/- equal to your armor and shield bonus to AC (to a maximum equal to your oracle level) for two rounds.");
                n.AddComponents(drComp);
                n.m_Icon = icon;
                n.FxOnStart = startFx;
            });

            // Toggle Ability
            const string desc = "Whenever you receive damage and your health drops below 50%, you gain an amount of DR/- equal to your armor and shield bonus to AC (to a maximum equal to your oracle level) for two rounds. This ability can trigger three times per day.";

            var ability = Helpers.CreateBlueprint<BlueprintActivatableAbility>("AsylumToggleAbility", Guids.AsylumToggleAbility, n =>
            {
                n.SetName("Asylum");
                n.SetDescription(desc);
                n.DeactivateIfCombatEnded = false;
                n.DeactivateIfOwnerDisabled = false;
                n.DeactivateIfOwnerUnconscious = false;
                n.DeactivateImmediately = true;
                n.ActivationType = AbilityActivationType.WithUnitCommand;
                n.m_ActivateWithUnitCommand = UnitCommand.CommandType.Swift;
                n.m_Icon = icon;
                n.m_Buff = hiddenBuff.ToReference<BlueprintBuffReference>();
                n.AddComponents(resourceLogic);
                n.IsOnByDefault = true;
            });

            // Ability Feature
            var prerequisites = Helpers.Create<PrerequisiteFeaturesFromList>(n =>
            {
                n.m_Features = new[] {mystery};
            });

            var abilityComp = Helpers.Create<AddFacts>(c => c.m_Facts = new[]{ability.ToReference<BlueprintUnitFactReference>()});

            var abilityFeature = Helpers.CreateBlueprint<BlueprintFeature>("Asylum", Guids.Asylum, n =>
            {
                n.SetName("Asylum");
                n.SetDescription(desc);
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.Groups = new[] {FeatureGroup.OracleRevelation};
                n.AddComponents(abilityComp, prerequisites, resouceComp);
            });

            return abilityFeature;
        }

        private static BlueprintFeature CreateClemency(BlueprintFeatureReference mystery)
        {
            var immunityComp = new AddMutualImmunityToCriticalHits();

            var prerequisites = Helpers.Create<PrerequisiteFeaturesFromList>(n =>
            {
                n.m_Features = new[] {mystery};
            });

            var clemency = Helpers.CreateBlueprint<BlueprintFeature>("Clemency", Guids.Clemency, n =>
            {
                n.SetName("Clemency");
                n.SetDescription("You become immune to critical strikes. However, you can no longer deal critical strikes yourself.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.Groups = new[] {FeatureGroup.OracleRevelation};
                n.AddComponents(immunityComp, prerequisites);
            });

            return clemency;
        }

        private static BlueprintFeature CreateAtonement(BlueprintFeatureReference mystery)
        {
            var atonementComp = new AtonementComp();
            var icon = Resources.GetBlueprint<BlueprintFeature>(Guids.KiDiamondSoulFeature).m_Icon;
            var saintsTouch = Resources.GetBlueprint<BlueprintAbility>(Guids.SaintsTouchAbility)
                .ToReference<BlueprintAbilityReference>();

            Helpers.CreateBuff("AtonementBuff", Guids.AtonementBuff, n =>
            {
                n.SetName("Atonement");
                n.SetDescription("Fast healing for 20% of Saint's Touch value.");
                n.m_Flags = BlueprintBuff.Flags.RemoveOnRest | BlueprintBuff.Flags.IsFromSpell;
                n.m_Icon = icon;
                n.Stacking = StackingType.Ignore;
                n.AddComponents(new AtonementBuffComp());
            });

            var prerequisites = Helpers.Create<PrerequisiteFeaturesFromList>(n =>
            {
                n.m_Features = new[] {mystery};
            });

            var autoQuicken = new AutoMetamagic
            {
                Abilities = new List<BlueprintAbilityReference>{saintsTouch},
                Metamagic = Metamagic.Quicken,
                m_AllowedAbilities = AutoMetamagic.AllowedType.Any
            };

            return Helpers.CreateBlueprint<BlueprintFeature>("Atonement", Guids.Atonement, n =>
            {
                n.SetName("Atonement");
                n.SetDescription("Your Saint's Touch now causes the target to gain an amount of fast healing equal to 20% of the damage healed for three rounds. It also becomes a swift action.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.Groups = new[] {FeatureGroup.OracleRevelation};
                n.AddComponents(atonementComp, prerequisites, autoQuicken);
            });
        }

        private static BlueprintFeature CreateLuminositeEternelle(BlueprintFeatureReference mystery)
        {
            const string desc = "You can create a luminous standard that protects a specified area as a full round action. All allies within the area receive a sacred bonus equal to your charisma modifier on all saving throws" +
                                "and AC while they remain inside the area. This ability lasts for one hour per oracle level and can be used once per day.";

            var guardedHearth = Resources.GetBlueprint<BlueprintAbility>("76291e62d2496ad41824044aba3077ea");

            // Resource
            var resource = Helpers.CreateBlueprint<BlueprintAbilityResource>("LuminositeEternelleResource", Guids.LuminositeEternelleResource, n =>
            {
                n.m_MaxAmount = new BlueprintAbilityResource.Amount
                {
                    IncreasedByStat = false,
                    BaseValue = 1
                };

                n.m_Max = 1;
                n.LocalizedName = new LocalizedString();
                n.LocalizedDescription = new LocalizedString();
            });

            // Buff
            var fortitudeComp = new AddStatBonusAbilityValue
            {
                Descriptor = ModifierDescriptor.Sacred,
                Stat = StatType.SaveFortitude,
                Value = new ContextValue {ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.StatBonus}
            };

            var reflexComp = new AddStatBonusAbilityValue
            {
                Descriptor = ModifierDescriptor.Sacred,
                Stat = StatType.SaveReflex,
                Value = new ContextValue {ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.StatBonus}
            };

            var willComp = new AddStatBonusAbilityValue
            {
                Descriptor = ModifierDescriptor.Sacred,
                Stat = StatType.SaveWill,
                Value = new ContextValue {ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.StatBonus}
            };

            var acComp = new AddStatBonusAbilityValue
            {
                Descriptor = ModifierDescriptor.Sacred,
                Stat = StatType.AC,
                Value = new ContextValue {ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.StatBonus}
            };

            var rankConfig = new ContextRankConfig
            {
                m_Type = AbilityRankType.StatBonus,
                m_BaseValueType = ContextRankBaseValueType.StatBonus,
                m_Stat = StatType.Charisma,
                m_Max = 20
            };

            var buff = Helpers.CreateBuff("LuminositeEternelleBuff", Guids.LuminositeEternelleBuff, n =>
            {
                n.SetName("Luminosite Eternelle");
                n.SetDescription(desc);
                n.IsClassFeature = true;
                n.m_Icon = guardedHearth.m_Icon;
                n.AddComponents(fortitudeComp, reflexComp, willComp, acComp, rankConfig);
            });

            // Area
            var areaEffectBuff = new AbilityAreaEffectBuff
            {
                m_Buff = buff.ToReference<BlueprintBuffReference>(),
                Condition = new ConditionsChecker {Conditions = new Condition[] {new ContextConditionIsAlly()}}
            };

            var guardedHearthArea = Resources.GetBlueprint<BlueprintAbilityAreaEffect>("3635b48c6e8d54947bbd27c1be818677");

            var areaEffect = Helpers.CreateBlueprint<BlueprintAbilityAreaEffect>("LuminositeEternelleArea", Guids.LuminositeEternelleArea, n =>
            {
                n.AggroEnemies = true;
                n.Shape = AreaEffectShape.Cylinder;
                n.Size = new Feet(30);
                n.Fx = guardedHearthArea.Fx;
                n.AddComponents(areaEffectBuff);
            });

            // Ability
            var spawnArea = new ContextActionSpawnAreaEffect
            {
                m_AreaEffect = areaEffect.ToReference<BlueprintAbilityAreaEffectReference>(),
                DurationValue = new ContextDurationValue
                {
                    m_IsExtendable = true, Rate = DurationRate.Hours, DiceCountValue = new ContextValue(),
                    BonusValue = new ContextValue {ValueType = ContextValueType.Rank}
                }
            };

            var effectRunActions = new AbilityEffectRunAction
            {
                Actions = new ActionList
                {
                    Actions = new GameAction[]{spawnArea}
                }
            };

            var spawnFx = new AbilitySpawnFx
            {
                Anchor = AbilitySpawnFxAnchor.ClickedTarget,
                PositionAnchor = AbilitySpawnFxAnchor.None,
                OrientationAnchor = AbilitySpawnFxAnchor.None,
                PrefabLink = guardedHearth.GetComponent<AbilitySpawnFx>()?.PrefabLink
            };

            var spellComp = new SpellComponent {School = SpellSchool.Enchantment};

            var resourceLogic = new AbilityResourceLogic
            {
                m_IsSpendResource = true,
                Amount = 1,
                m_RequiredResource = resource.ToReference<BlueprintAbilityResourceReference>(),
            };

            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle)
                .ToReference<BlueprintCharacterClassReference>();

            var contextRankConfig = new ContextRankConfig
            {
                m_Type = AbilityRankType.Default,
                m_BaseValueType = ContextRankBaseValueType.ClassLevel,
                m_Max = 20,
                m_Class = new[] {oracle}
            };

            var ability = Helpers.CreateBlueprint<BlueprintAbility>("LuminositeEternelleAbility", Guids.LuminositeEternelleAbility, n =>
            {
                n.m_Icon = guardedHearth.m_Icon;
                n.Type = AbilityType.SpellLike;
                n.Range = AbilityRange.Medium;
                n.CanTargetPoint = true;
                n.CanTargetEnemies = true;
                n.CanTargetFriends = true;
                n.CanTargetSelf = true;
                n.SpellResistance = true;
                n.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
                n.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Point;
                n.ActionType = UnitCommand.CommandType.Standard;
                n.AvailableMetamagic = Metamagic.Quicken | Metamagic.Reach | Metamagic.Extend | Metamagic.Heighten |
                                       Metamagic.CompletelyNormal;
                n.LocalizedDuration = Helpers.CreateString("LuminositeEternelle" + ".Duration", "1 hour/level");
                n.SetName("Luminosite Eternelle");
                n.SetDescription(desc);
                n.AddComponents(spellComp, effectRunActions, spawnFx, resourceLogic, contextRankConfig);
                n.LocalizedSavingThrow = new LocalizedString();
            });

            // Ability Feature
            var classPrerequisite = new PrerequisiteClassLevel
            {
                m_CharacterClass = oracle,
                Level = 11
            };

            var prerequisites = Helpers.Create<PrerequisiteFeaturesFromList>(n =>
            {
                n.m_Features = new[] {mystery};
            });

            var abilityComp = Helpers.Create<AddFacts>(c => c.m_Facts = new[]{ability.ToReference<BlueprintUnitFactReference>()});

            var resouceComp = Helpers.Create<AddAbilityResources>(n =>
            {
                n.m_Resource = resource.ToReference<BlueprintAbilityResourceReference>();
                n.RestoreAmount = true;
            });

            var abilityFeature = Helpers.CreateBlueprint<BlueprintFeature>("LuminositeEternelle", Guids.LuminositeEternelle, n =>
            {
                n.SetName("Luminosite Eternelle");
                n.SetDescription(desc);
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.Groups = new[] {FeatureGroup.OracleRevelation};
                n.AddComponents(abilityComp, prerequisites, classPrerequisite, resouceComp);
            });

            return abilityFeature;
        }

        private static BlueprintFeature CreateRelicArmor()
        {
            var ignorePenaltiesComp = Helpers.Create<IgnoreArmorPenaltiesComp>(n =>
            {
                n.Categories = new HashSet<ArmorProficiencyGroup>{ArmorProficiencyGroup.Light, ArmorProficiencyGroup.Medium, ArmorProficiencyGroup.HeavyShield, ArmorProficiencyGroup.Buckler, ArmorProficiencyGroup.LightShield};
            });

            var relicArmor = Helpers.CreateBlueprint<BlueprintFeature>("RelicArmor", Guids.RelicArmor, n =>
            {
                n.SetName("Relic Armor");
                n.SetDescription("Your maximum dexterity bonus to AC permitted by your armor is increased by an amount equal to half your oracle level. Ignore all penalties to skill checks and speed imposed by medium armor, light armor, heavy shields, light shields, and bucklers.");
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
                    BaseValue = 1
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
                DiceType = DiceType.D4,
                BonusValue = new ContextValue {ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.DamageBonus}
            };

            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);

            var contextRankConfig = Helpers.Create<ContextRankConfig>(n =>
            {
                n.m_Progression = ContextRankProgression.Div2;
                n.m_BaseValueType = ContextRankBaseValueType.ClassLevel;
                n.m_Type = AbilityRankType.DamageDice;
                n.m_Class = new[]{oracle.ToReference<BlueprintCharacterClassReference>()};
                n.m_Max = int.MaxValue;
            });

            var bonusContextRankConfig = Helpers.Create<ContextRankConfig>(n =>
            {
                n.m_Progression = ContextRankProgression.DoublePlusBonusValue;
                n.m_BaseValueType = ContextRankBaseValueType.ClassLevel;
                n.m_Type = AbilityRankType.DamageBonus;
                n.m_Class = new[]{oracle.ToReference<BlueprintCharacterClassReference>()};
                n.m_Max = int.MaxValue;
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
                n.SetDescription("Cures ({g|Encyclopedia:Class_Level}class level{/g}/2){g|Encyclopedia:Dice}d4{/g} points of {g|Encyclopedia:Damage}damage{/g} + 2 points per {g|Encyclopedia:Class_Level}class level{/g}. Can be used a number of times per day equal to 1 plus your charisma modifier.");
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
                n.AddComponents(spawnFx, resourceLogicComp, contextRankConfig, abilityEffectComp, bonusContextRankConfig);
            });

            var cureAbilityRef = cureAbility.ToReference<BlueprintAbilityReference>();
            var abilityComp = Helpers.Create<AddFacts>(n => n.m_Facts = new[] {cureAbility.ToReference<BlueprintUnitFactReference>()});

            var saintsCure = Helpers.CreateBlueprint<BlueprintFeature>("SaintsTouchFeature", Guids.SaintsTouchFeature, n =>
            {
                n.SetName("Saint's Touch");
                n.SetDescription("Cures ({g|Encyclopedia:Class_Level}class level{/g}/2){g|Encyclopedia:Dice}d4{/g} points of {g|Encyclopedia:Damage}damage{/g} + 2 points per {g|Encyclopedia:Class_Level}class level{/g}. Can be used a number of times per day equal to 1 plus your charisma modifier.");
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
            var areaFx = Resources.GetBlueprint<BlueprintAbilityAreaEffect>("2d6c2640974d6314fb5f9c1f6570950f").Fx;

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
                n.m_TargetType = BlueprintAbilityAreaEffect.TargetType.Enemy;
                n.Shape = AreaEffectShape.Cylinder;
                n.Size = new Feet(20);
                n.AddComponents(debuffAreaEffect);
                n.Fx = areaFx;
            });

            var areaComp = Helpers.Create<AddAreaEffect>(c =>
                c.m_AreaEffect = debuffArea.ToReference<BlueprintAbilityAreaEffectReference>());

            var penaltyComp = Helpers.Create<WeaponMultipleCategoriesAttackBonus>(n =>
            {
                n.Categories = new[] {WeaponCategory.Touch, WeaponCategory.Ray};
                n.AttackBonus = -4;
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

            var fightingDefensively = Resources.GetBlueprint<BlueprintFeature>("ca22afeb94442b64fb8536e7a9f7dc11");
            var removeFightingDefensively = Helpers.Create<RemoveFeatureOnApply>(n => n.m_Feature = fightingDefensively.ToReference<BlueprintUnitFactReference>());

            var feature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeT1", Guids.PathOfSacrificeT1, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT1Desc);
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(Helpers.Create<AddFacts>(c => c.m_Facts = new[]{buff.ToReference<BlueprintUnitFactReference>()}));
            });

            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);

            var mainFeatureComp = Helpers.Create<AddFeatureOnClassLevel>(n =>
            {
                n.m_Class = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Archetypes = new[] {divineSaintArchetypeRef};
                n.m_Feature = feature.ToReference<BlueprintFeatureReference>();
                n.Level = 5;
                n.BeforeThisLevel = true;
            });

            var mainFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeMainT1", Guids.PathOfSacrificeMainT1, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription("Enemies within 20 feet of you are compelled to attack you instead of your allies. You receive a -4 penalty to weapon attack rolls and can no longer fight defensively.");
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(mainFeatureComp, removeFightingDefensively);
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
                n.SetDescription("You can add your charisma modifier as a bonus to AC while wearing medium or light armor.");
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
            var areaFx = Resources.GetBlueprint<BlueprintAbilityAreaEffect>("2d6c2640974d6314fb5f9c1f6570950f").Fx;

            var debuff = Helpers.CreateBuff("PathOfSacrificeDebuffT3", Guids.PathOfSacrificeDebuffT3, n =>
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

            var debuffArea = Helpers.CreateBlueprint<BlueprintAbilityAreaEffect>("PathOfSacrificeDebuffAreaT3", Guids.PathOfSacrificeDebuffAreaT3, n =>
            {
                n.AffectEnemies = true;
                n.AggroEnemies = false;
                n.Shape = AreaEffectShape.Cylinder;
                n.Size = new Feet(20);
                n.AddComponents(debuffAreaEffect);
                n.Fx = areaFx;
            });

            var areaComp = Helpers.Create<AddAreaEffect>(c =>
                c.m_AreaEffect = debuffArea.ToReference<BlueprintAbilityAreaEffectReference>());

            var penaltyComp = Helpers.Create<WeaponMultipleCategoriesAttackBonus>(n =>
            {
                n.Categories = new[] {WeaponCategory.Touch, WeaponCategory.Ray};
                n.AttackBonus = -4;
                n.ExceptForCategories = true;
                n.Descriptor = ModifierDescriptor.UntypedStackable;
            });

            var aooImmunityComp = Helpers.Create<AddCondition>(n =>
            {
                n.Condition = UnitCondition.ImmuneToAttackOfOpportunity;
            });

            var buff = Helpers.CreateBuff("PathOfSacrificeBuffT3", Guids.PathOfSacrificeBuffT3, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT1Desc);
                n.m_Flags = BlueprintBuff.Flags.StayOnDeath;
                n.m_Icon = icon;
                n.AddComponents(areaComp, penaltyComp, aooImmunityComp);
            });

            var priorityTargetComp = Helpers.Create<PriorityTarget>(c =>
                c.PriorityFact = buff.ToReference<BlueprintUnitFactReference>());

            debuff.AddComponent(priorityTargetComp);

            var feature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeT3", Guids.PathOfSacrificeT3, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT1Desc);
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(Helpers.Create<AddFacts>(c => c.m_Facts = new[]{buff.ToReference<BlueprintUnitFactReference>()}));
            });

            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);

            var mainFeatureComp = Helpers.Create<AddFeatureOnClassLevel>(n =>
            {
                n.m_Class = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Archetypes = new[] {divineSaintArchetypeRef};
                n.m_Feature = feature.ToReference<BlueprintFeatureReference>();
                n.Level = 9;
                n.BeforeThisLevel = true;
            });

            var fightingDefensively = Resources.GetBlueprint<BlueprintFeature>("ca22afeb94442b64fb8536e7a9f7dc11");
            var removeFightingDefensively = Helpers.Create<RemoveFeatureOnApply>(n => n.m_Feature = fightingDefensively.ToReference<BlueprintUnitFactReference>());

            var mainFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeMainT3", Guids.PathOfSacrificeMainT3, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription("You become immune to attacks of opportunity.");
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(mainFeatureComp, removeFightingDefensively);
            });

            return mainFeature;
        }

        private static BlueprintFeature CreatePathOfSacrificeT4(BlueprintArchetypeReference divineSaintArchetypeRef)
        {
            var icon = Resources.GetBlueprint<BlueprintFeature>(Guids.AuraOfRighteousness).m_Icon;
            var areaFx = Resources.GetBlueprint<BlueprintAbilityAreaEffect>("2d6c2640974d6314fb5f9c1f6570950f").Fx;

            var debuff = Helpers.CreateBuff("PathOfSacrificeDebuffT4", Guids.PathOfSacrificeDebuffT4, n =>
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

            var debuffArea = Helpers.CreateBlueprint<BlueprintAbilityAreaEffect>("PathOfSacrificeDebuffAreaT4", Guids.PathOfSacrificeDebuffAreaT4, n =>
            {
                n.AffectEnemies = true;
                n.AggroEnemies = false;
                n.Shape = AreaEffectShape.Cylinder;
                n.Size = new Feet(20);
                n.AddComponents(debuffAreaEffect);
                n.Fx = areaFx;
            });

            var areaComp = Helpers.Create<AddAreaEffect>(c =>
                c.m_AreaEffect = debuffArea.ToReference<BlueprintAbilityAreaEffectReference>());

            var penaltyComp = Helpers.Create<WeaponMultipleCategoriesAttackBonus>(n =>
            {
                n.Categories = new[] {WeaponCategory.Touch, WeaponCategory.Ray};
                n.AttackBonus = -4;
                n.ExceptForCategories = true;
                n.Descriptor = ModifierDescriptor.UntypedStackable;
            });

            var dodgeBonusComp = Helpers.Create<AddStatBonus>(n =>
            {
                n.Stat = StatType.HitPoints;
                n.Value = 20;
                n.Descriptor = ModifierDescriptor.UntypedStackable;
            });

            var aooImmunityComp = Helpers.Create<AddCondition>(n =>
            {
                n.Condition = UnitCondition.ImmuneToAttackOfOpportunity;
            });

            var buff = Helpers.CreateBuff("PathOfSacrificeBuffT4", Guids.PathOfSacrificeBuffT4, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT1Desc);
                n.m_Flags = BlueprintBuff.Flags.StayOnDeath;
                n.m_Icon = icon;
                n.AddComponents(areaComp, penaltyComp, aooImmunityComp, dodgeBonusComp);
            });

            var priorityTargetComp = Helpers.Create<PriorityTarget>(c =>
                c.PriorityFact = buff.ToReference<BlueprintUnitFactReference>());

            debuff.AddComponent(priorityTargetComp);

            var feature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeT4", Guids.PathOfSacrificeT4, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT1Desc);
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(Helpers.Create<AddFacts>(c => c.m_Facts = new[]{buff.ToReference<BlueprintUnitFactReference>()}));
            });

            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>(Guids.Oracle);

            var mainFeatureComp = Helpers.Create<AddFeatureOnClassLevel>(n =>
            {
                n.m_Class = oracle.ToReference<BlueprintCharacterClassReference>();
                n.m_Archetypes = new[] {divineSaintArchetypeRef};
                n.m_Feature = feature.ToReference<BlueprintFeatureReference>();
                n.Level = 13;
                n.BeforeThisLevel = true;
            });

            var fightingDefensively = Resources.GetBlueprint<BlueprintFeature>("ca22afeb94442b64fb8536e7a9f7dc11");
            var removeFightingDefensively = Helpers.Create<RemoveFeatureOnApply>(n => n.m_Feature = fightingDefensively.ToReference<BlueprintUnitFactReference>());

            var mainFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeMainT4", Guids.PathOfSacrificeMainT4, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription("You gain a +20 bonus to hit points.");
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(mainFeatureComp, removeFightingDefensively);
            });

            return mainFeature;
        }

        private static BlueprintFeature CreatePathOfSacrificeT5()
        {
            var icon = Resources.GetBlueprint<BlueprintFeature>(Guids.AuraOfRighteousness).m_Icon;
            var areaFx = Resources.GetBlueprint<BlueprintAbilityAreaEffect>("2d6c2640974d6314fb5f9c1f6570950f").Fx;

            var debuff = Helpers.CreateBuff("PathOfSacrificeDebuffT5", Guids.PathOfSacrificeDebuffT5, n =>
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

            var debuffArea = Helpers.CreateBlueprint<BlueprintAbilityAreaEffect>("PathOfSacrificeDebuffAreaT5", Guids.PathOfSacrificeDebuffAreaT5, n =>
            {
                n.AffectEnemies = true;
                n.AggroEnemies = false;
                n.Shape = AreaEffectShape.Cylinder;
                n.Size = new Feet(20);
                n.AddComponents(debuffAreaEffect);
                n.Fx = areaFx;
            });

            var areaComp = Helpers.Create<AddAreaEffect>(c =>
                c.m_AreaEffect = debuffArea.ToReference<BlueprintAbilityAreaEffectReference>());

            var penaltyComp = Helpers.Create<WeaponMultipleCategoriesAttackBonus>(n =>
            {
                n.Categories = new[] {WeaponCategory.Touch, WeaponCategory.Ray};
                n.AttackBonus = -4;
                n.ExceptForCategories = true;
                n.Descriptor = ModifierDescriptor.UntypedStackable;
            });

            var dodgeBonusComp = Helpers.Create<AddStatBonus>(n =>
            {
                n.Stat = StatType.HitPoints;
                n.Value = 20;
                n.Descriptor = ModifierDescriptor.UntypedStackable;
            });

            var aooImmunityComp = Helpers.Create<AddCondition>(n =>
            {
                n.Condition = UnitCondition.ImmuneToAttackOfOpportunity;
            });

            var buff = Helpers.CreateBuff("PathOfSacrificeBuffT5", Guids.PathOfSacrificeBuffT5, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription(PathOfSacrificeT1Desc);
                n.m_Flags = BlueprintBuff.Flags.StayOnDeath;
                n.m_Icon = icon;
                n.AddComponents(areaComp, penaltyComp, aooImmunityComp, dodgeBonusComp);
            });

            var priorityTargetComp = Helpers.Create<PriorityTarget>(c =>
                c.PriorityFact = buff.ToReference<BlueprintUnitFactReference>());

            var shakenEffectComp = Helpers.Create<AddCondition>(n =>
            {
                n.Condition = UnitCondition.Shaken;
            });

            debuff.AddComponents(priorityTargetComp, shakenEffectComp);

            var fightingDefensively = Resources.GetBlueprint<BlueprintFeature>("ca22afeb94442b64fb8536e7a9f7dc11");
            var removeFightingDefensively = Helpers.Create<RemoveFeatureOnApply>(n => n.m_Feature = fightingDefensively.ToReference<BlueprintUnitFactReference>());

            var mainFeature = Helpers.CreateBlueprint<BlueprintFeature>("PathOfSacrificeMainT5", Guids.PathOfSacrificeMainT5, n =>
            {
                n.SetName("Path of Sacrifice");
                n.SetDescription("Enemies affected by Path of Sacrifice now also become shaken.");
                n.IsClassFeature = true;
                n.m_Icon = icon;
                n.Ranks = 1;
                n.AddComponents(Helpers.Create<AddFacts>(c => c.m_Facts = new[]{buff.ToReference<BlueprintUnitFactReference>()}), removeFightingDefensively);
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

        private static void AddToSelection(BlueprintFeatureSelection selection, SimpleBlueprint feature)
        {
            var list = selection.m_AllFeatures.ToList();
            list.Add(feature.ToReference<BlueprintFeatureReference>());
            selection.m_AllFeatures = list.ToArray();
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
