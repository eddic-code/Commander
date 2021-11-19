using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;

namespace Commander.Components
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [TypeId("7811b745174944e592884da2c0aaea2e")]
    public class AegisComp : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateDamage>
    {
        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            foreach (var damage in evt.DamageBundle)
            {
                damage.BonusPercent += damage.BonusPercent - 25;
            }
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
            foreach (var damage in evt.CalculatedDamage)
            {
                Main.Log($"Damage: {damage.FinalValue} | Rolled: {damage.RolledValue} | Reduction: {damage.Reduction} | ValueWithoutReduction: {damage.ValueWithoutReduction} | Bonus %: {damage.Source.BonusPercent}");
            }
        }
    }
}