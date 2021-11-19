using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace Commander.Components
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [TypeId("26102f6edd2245aab8a3db34ab74a7e5")]
    public class AbsolutionComp : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackRoll>
    {
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            var value = Math.Max(1, (int)(Owner.MaxHP * 0.05f));

            GameHelper.HealDamage(Owner, Owner, value);
        }
    }
}
