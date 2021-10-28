using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Commander.Components
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [TypeId(Guids.AsylumReactiveEffectComp)]
    public class AsylumReactiveEffectComp : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>
    {
        private static BlueprintBuff _defensiveBuff;

        private static BlueprintBuff DefensiveBuff
        {
            get
            {
                return _defensiveBuff ??=
                    ResourcesLibrary.TryGetBlueprint(new BlueprintGuid(Guid.Parse(Guids.AsylumDefensiveBuff))) as
                        BlueprintBuff;
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

            Owner.AddBuff(DefensiveBuff, Context);
        }
    }
}
