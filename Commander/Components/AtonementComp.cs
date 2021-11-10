using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Commander.Components
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [TypeId("2d40f35fdfcb4fff876c27d3bee2fdd8")]
    public class AtonementComp : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleHealDamage>
    {
        private static BlueprintBuff _atonementBuff;

        private static BlueprintBuff AtonementBuff
        {
            get
            {
                return _atonementBuff ??=
                    ResourcesLibrary.TryGetBlueprint(new BlueprintGuid(Guid.Parse(Guids.AtonementBuff))) as
                        BlueprintBuff;
            }
        }

        public void OnEventAboutToTrigger(RuleHealDamage evt)
        {
            
        }

        public void OnEventDidTrigger(RuleHealDamage evt)
        {
            var value = Math.Max(1, (int) (evt.ValueWithoutReduction * 0.25));
            var timeSpan = TimeSpan.FromSeconds(6 * 3);
            var abilityParams = new AbilityParams {RankBonus = value};

            evt.Target.AddBuff(AtonementBuff, evt.Initiator, timeSpan, abilityParams);

            Main.Log($"Fast healing {value}.");
        }
    }
}
