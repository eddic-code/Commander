using System;
using System.Linq;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;

namespace Commander.Content
{
    internal static class DivineSaintArchetype
    {
        public static void Create()
        {
            var oracle = Resources.GetBlueprint<BlueprintCharacterClass>("20ce9bf8af32bee4c8557a045ab499b1");

            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "DivineSaintArchetype";
                a.AssetGuid = new BlueprintGuid(new Guid("7510c55390964ebb9f7e212f4674c3eb"));
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Divine Saint");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A divine saint is an oracle who dedicates themselves to the protection of others. Their saintly charisma intimidates their enemies.");
            });

            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = new[] { StatType.SkillLoreReligion, StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice };

            var powerlessProphecy = Resources.GetBlueprint<BlueprintFeature>("88c89ab44a36a67438347e257f70c8fe");
            archetype.AddFeatures = new[] { Helpers.LevelEntry(1, powerlessProphecy) };

            oracle.m_Archetypes = oracle.m_Archetypes.AddToArray(archetype.ToReference<BlueprintArchetypeReference>()).ToArray();
            Resources.AddBlueprint(archetype);
        }
    }
}
