using System;
using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;

namespace Commander.Components
{
    public class AsylumBuffComp : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleDealDamage>
    {
        private static BlueprintBuff _defensiveBuff;
        private static BlueprintAbilityResource _resource;

        private static BlueprintBuff DefensiveBuff
        {
            get
            {
                return _defensiveBuff ??=
                    ResourcesLibrary.TryGetBlueprint(new BlueprintGuid(Guid.Parse(Guids.AsylumDefensiveBuff))) as
                        BlueprintBuff;
            }
        }

        private static BlueprintAbilityResource Resource
        {
            get
            {
                return _resource ??=
                    ResourcesLibrary.TryGetBlueprint(new BlueprintGuid(Guid.Parse(Guids.AsylumResource))) as
                        BlueprintAbilityResource;
            }
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt.Result <= 0 || Owner.State.IsDead || Owner.State.IsUnconscious || !Owner.CombatState.IsInCombat) { return; }

            var buff = Owner.Buffs.GetBuff(DefensiveBuff);
            if (buff != null && buff.TimeLeft > TimeSpan.Zero) { return; }

            if (Owner.HPLeft > Owner.MaxHP * 0.5f) { return; }

            Owner.AddBuff(DefensiveBuff, Context, TimeSpan.FromSeconds(12));
            Owner.Resources.Spend(Resource, 1);
        }
    }
}
