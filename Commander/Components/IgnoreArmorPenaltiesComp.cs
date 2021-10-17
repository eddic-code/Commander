using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace Commander.Components
{
    [TypeId("b55d5f359b294f14ad2a74856b6c6c58")]
    public class IgnoreArmorPenaltiesComp : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateArmorMaxDexBonusLimit>
    {
        public HashSet<ArmorProficiencyGroup> Categories;

        public override void OnTurnOn() 
        {
            base.OnTurnOn();

            if (Owner.Body.Armor.HasArmor && Owner.Body.Armor.Armor.Blueprint.IsArmor) 
            {
                Owner.Body.Armor.Armor.RecalculateStats();
                Owner.Body.Armor.Armor.RecalculateMaxDexBonus();
            }

            if (Owner.Body.SecondaryHand.HasShield && Owner.Body.SecondaryHand.MaybeShield != null) 
            {
                Owner.Body.SecondaryHand.MaybeShield.ArmorComponent.RecalculateStats();
                Owner.Body.SecondaryHand.MaybeShield.ArmorComponent.RecalculateMaxDexBonus();
            }
        }

        public void OnEventAboutToTrigger(RuleCalculateArmorMaxDexBonusLimit evt)
        {
            
        }

        public void OnEventDidTrigger(RuleCalculateArmorMaxDexBonusLimit evt)
        {
            if (!evt.Armor.Blueprint.IsShield && Categories.Contains(evt.Armor.ArmorType())) 
            {
                evt.Result = null;
                return;
            }

            if (evt.Armor.Blueprint.IsShield) 
            {
                evt.Result = null;
            }
        }
    }
}
