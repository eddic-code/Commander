using System;
using System.Linq;
using Commander.Components;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.FactLogic;

namespace Commander.Archetypes
{
    internal static class DivineSaintArchetype
    {
        public static void Create()
        {
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

            var saintsIntuition = Helpers.CreateBlueprint<BlueprintFeature>("SaintsIntuition", "5d4ff3da5da6401aba8a068d1aa11ad3", n =>
            {
                n.SetName("Saint's Intuition");
                n.SetDescription("Use your charisma as a bonus for AC, perception, and religion.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.AddComponents(replaceAcComp, replacePerceptionComp, replaceReligionComp, replaceCmdComp);
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
                n.AddComponents(saintsPresenceComp, recalculateComp);
            });

            // Archetype creation.
            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>("20ce9bf8af32bee4c8557a045ab499b1");

            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DivineSaintArchetype";
                a.AssetGuid = new BlueprintGuid(new Guid("7510c55390964ebb9f7e212f4674c3eb"));
                a.LocalizedName = Helpers.CreateBlueprintName(a.name, "Divine Saint");
                a.LocalizedDescription = Helpers.CreateBlueprintDescription(a.name, "A divine saint is an oracle who dedicates themselves to the protection of others. Their saintly charisma intimidates their enemies.");
            });

            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = new[] { StatType.SkillLoreReligion, StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice };
            archetype.AddFeatures = new[] { Helpers.LevelEntry(1, saintsPresence), Helpers.LevelEntry(1, saintsIntuition) };

            oracle.m_Archetypes = oracle.m_Archetypes.AddToArray(archetype.ToReference<BlueprintArchetypeReference>()).ToArray();
            Resources.AddBlueprint(archetype);
        }
    }
}
