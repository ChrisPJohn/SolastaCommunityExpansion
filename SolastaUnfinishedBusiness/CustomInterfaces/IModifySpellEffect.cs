﻿namespace SolastaUnfinishedBusiness.CustomInterfaces;

public interface IModifySpellEffect
{
    EffectDescription ModifyEffect(SpellDefinition spell, EffectDescription effect, RulesetCharacter caster);
}
