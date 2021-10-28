using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;

namespace Commander.Components
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [TypeId("77425d627bf04a79a4caba7ec41e3f84")]
    public class AddMutualImmunityToCriticalHits :  UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateDamage>, ITargetRulebookHandler<RuleAttackRoll>, 
        IInitiatorRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookHandler<RuleAttackRoll>, ITargetRulebookHandler<RuleDealStatDamage>, IInitiatorRulebookHandler<RuleDealStatDamage>
    {
        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            evt.CritImmunity = true;
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
            
        }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            evt.ImmuneToCriticalHit = true;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            
        }

        public void OnEventAboutToTrigger(RuleDealStatDamage evt)
        {
            evt.CriticalModifier = null;
        }

        public void OnEventDidTrigger(RuleDealStatDamage evt)
        {
            
        }
    }
}
