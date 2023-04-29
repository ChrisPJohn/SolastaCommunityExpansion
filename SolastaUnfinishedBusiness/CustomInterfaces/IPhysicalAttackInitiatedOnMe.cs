﻿using System.Collections;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.CustomInterfaces;

public interface IPhysicalAttackInitiatedOnMe
{
    [UsedImplicitly]
    IEnumerator OnAttackInitiatedOnMe(
        GameLocationBattleManager __instance,
        CharacterAction action,
        GameLocationCharacter attacker,
        GameLocationCharacter defender,
        ActionModifier attackModifier,
        RulesetAttackMode attackerAttackMode);
}
