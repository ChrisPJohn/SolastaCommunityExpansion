﻿using System.Linq;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

public class RangedAttackInMeleeDisadvantageRemover
{
    private readonly IsWeaponValidHandler isWeaponValid;
    private readonly CharacterValidator[] validators;

    public RangedAttackInMeleeDisadvantageRemover(IsWeaponValidHandler isWeaponValid,
        params CharacterValidator[] validators)
    {
        this.isWeaponValid = isWeaponValid;
        this.validators = validators;
    }

    public RangedAttackInMeleeDisadvantageRemover(params CharacterValidator[] validators)
        : this(WeaponValidators.AlwaysValid, validators)
    {
    }

    private bool CanApply(RulesetCharacter character, RulesetAttackMode attackMode)
    {
        if (isWeaponValid != null && !isWeaponValid.Invoke(attackMode, null, character))
        {
            return false;
        }

        return character.IsValid(validators);
    }

    /**
     * Patches `GameLocationBattleManager.CanAttack`
     * Removes ranged attack in melee disadvantage if there's specific feature present and active
     */
    public static void CheckToRemoveRangedDisadvantage(BattleDefinitions.AttackEvaluationParams attackParams)
    {
        if (attackParams.attackProximity != BattleDefinitions.AttackProximity.PhysicalRange)
        {
            return;
        }

        var character = attackParams.attacker?.RulesetCharacter;

        if (character == null)
        {
            return;
        }

        var features = character.GetSubFeaturesByType<RangedAttackInMeleeDisadvantageRemover>();

        if (!features.Any(f => f.CanApply(character, attackParams.attackMode)))
        {
            return;
        }

        attackParams.attackModifier.attackAdvantageTrends.RemoveAll(t =>
            t.value == -1
            && t.sourceType == RuleDefinitions.FeatureSourceType.Proximity
            && t.sourceName == RuleDefinitions.ProximityRangeEnemyNearby);
    }
}
