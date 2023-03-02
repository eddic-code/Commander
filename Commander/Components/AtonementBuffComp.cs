using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;

namespace Commander.Components
{
    [AllowedOn(typeof(BlueprintBuff))]
    [TypeId("89b2cb56b1034c2c96f18fa3ab74537b")]
    public class AtonementBuffComp : UnitBuffComponentDelegate, ITickEachRound
    {
        public void OnNewRound()
        {
            var value = Context.Params.RankBonus;

            if (!Owner.State.IsDead && Owner.Damage > 0)
            {
                GameHelper.HealDamage(Owner, Owner, value, null);
            }
        }
    }
}
