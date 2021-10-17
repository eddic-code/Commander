using System;
using System.Linq;
using Commander.Components;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;

namespace Commander.Archetypes
{
    internal static class DivineSaintArchetype
    {
        public static void Create()
        {
            // Features creation.
            var saintsPrecenseComp = Helpers.Create<SaintsPresenceComp>();

            var saintsPresence = Helpers.CreateBlueprint<BlueprintFeature>("SaintsPresenceComp", "daeb002e013f4965a4a690bc4e3876e5", n =>
            {
                n.SetName("Saint's Presence");
                n.SetDescription("While wearing medium or light armor, the divine saint can add their charisma bonus to their AC.");
                n.IsClassFeature = true;
                n.Ranks = 1;
                n.AddComponent(saintsPrecenseComp);
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
            archetype.AddFeatures = new[] { Helpers.LevelEntry(1, saintsPresence) };

            oracle.m_Archetypes = oracle.m_Archetypes.AddToArray(archetype.ToReference<BlueprintArchetypeReference>()).ToArray();
            Resources.AddBlueprint(archetype);
        }
    }
}
