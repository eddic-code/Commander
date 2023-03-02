using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Commander.Components
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [TypeId("26102f6edd2245aab8a3db34ab74a7e5")]
    public class AbsolutionComp : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackRoll>
    {
        private static BlueprintBuff _cdBuff;

        private static BlueprintBuff CdBuff
        {
            get
            {
                return _cdBuff ??=
                    ResourcesLibrary.TryGetBlueprint(new BlueprintGuid(Guid.Parse(Guids.AbsolutionCdBuff))) as
                        BlueprintBuff;
            }
        }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            var buff = Owner.Buffs.GetBuff(CdBuff);
            if (buff != null && buff.TimeLeft > TimeSpan.Zero) { return; }

            var value = Math.Max(1, (int)(Owner.MaxHP * 0.08f));

            var target = Game.Instance.Player.PartyAndPets
                .Where(n => n.HPLeft < n.MaxHP)
                .OrderBy(n => n.HPLeft)
                .FirstOrDefault();

            if (target == null) { return; }

            GameHelper.HealDamage(Owner, target, value, null);

            Owner.AddBuff(CdBuff, Context, TimeSpan.FromSeconds(6));
        }
    }
}
