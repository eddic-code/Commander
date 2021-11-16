using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;

namespace Commander.Components
{
    [TypeId("26102f6edd2245aab8a3db34ab74a7e5")]
    public class AegisComp : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>
    {
        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (Owner.State.IsDead || Owner.HPLeft > Owner.MaxHP / 2) { return; }

            Main.Log($"Aegis Damage: {evt.Result}");

            evt.Result = evt.Result <= 1 ? evt.Result : evt.Result / 2;
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {

        }
    }
}
