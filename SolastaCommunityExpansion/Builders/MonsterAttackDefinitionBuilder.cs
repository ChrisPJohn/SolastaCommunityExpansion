﻿using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Builders
{
    public class MonsterAttackDefinitionBuilder : BaseDefinitionBuilder<MonsterAttackDefinition>
    {
        public MonsterAttackDefinitionBuilder(string name, string guid, MonsterAttackDefinition baseDefinition) :
            base(baseDefinition, name, guid)
        {
        }

        public MonsterAttackDefinitionBuilder SetToHitBonus(int value)
        {
            Definition.SetToHitBonus(value);
            return this;
        }

        public MonsterAttackDefinitionBuilder SetDamageBonusOfFirstDamageForm(int value)
        {
            var form = Definition.EffectDescription.GetFirstFormOfType(EffectForm.EffectFormType.Damage);
            form.DamageForm.SetBonusDamage(value);
            return this;
        }
    }
}

