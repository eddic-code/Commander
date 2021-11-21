using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Properties;

namespace Commander.Components
{
    [TypeId("7b7311c5b1ca4254b5d7c2ad4a0b0ff3")]
    public class AsylumPropertyGetter : PropertyValueGetter
    {
        private static BlueprintCharacterClass _oracleClass;

        private static BlueprintCharacterClass OracleClass
        {
            get
            {
                return _oracleClass ??=
                    ResourcesLibrary.TryGetBlueprint(new BlueprintGuid(Guid.Parse(Guids.Oracle))) as
                        BlueprintCharacterClass;
            }
        }

        public override int GetBaseValue(UnitEntityData unit)
        {
            var level = unit.Descriptor.Progression.GetClassLevel(OracleClass);

            return level;
        }
    }
}
