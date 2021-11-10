﻿using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Properties;

namespace Commander.Components
{
    [TypeId("7b7311c5b1ca4254b5d7c2ad4a0b0ff3")]
    public class AsylumPropertyGetter : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
            var armor = unit.Stats.AC.m_ArmorAC;
            var shield = unit.Stats.AC.m_ShieldAC;

            return armor + shield;
        }
    }
}