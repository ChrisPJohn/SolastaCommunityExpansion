﻿using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

public class CanUseAttributeForWeapon : IModifyAttackAttributeForWeapon
{
    private readonly CharacterValidator[] _validators;
    private readonly string attribute;
    private readonly IsWeaponValidHandler isWeaponValid;

    public CanUseAttributeForWeapon(string attribute, IsWeaponValidHandler isWeaponValid,
        params CharacterValidator[] validators)
    {
        this.attribute = attribute;
        this.isWeaponValid = isWeaponValid;
        _validators = validators;
    }

    public void ModifyAttribute(RulesetCharacter character, [CanBeNull] RulesetAttackMode attackMode,
        RulesetItem weapon)
    {
        if (attackMode == null)
        {
            return;
        }

        if (!character.IsValid(_validators))
        {
            return;
        }

        if (!isWeaponValid(attackMode, weapon, character))
        {
            return;
        }

        if (character.GetAttribute(attribute).CurrentValue >
            character.GetAttribute(attackMode.AbilityScore).CurrentValue)
        {
            attackMode.AbilityScore = attribute;
        }
    }
}

public abstract class ModifyAttackModeForWeaponBase : IModifyAttackModeForWeapon
{
    private readonly IsWeaponValidHandler isWeaponValid;
    private readonly CharacterValidator[] validators;

    protected ModifyAttackModeForWeaponBase(IsWeaponValidHandler isWeaponValid,
        params CharacterValidator[] validators)
    {
        this.isWeaponValid = isWeaponValid;
        this.validators = validators;
    }

    public void ModifyAttackMode(RulesetCharacter character, [CanBeNull] RulesetAttackMode attackMode,
        RulesetItem weapon)
    {
        if (attackMode == null)
        {
            return;
        }

        if (!character.IsValid(validators))
        {
            return;
        }

        if (!isWeaponValid(attackMode, weapon, character))
        {
            return;
        }

        TryModifyAttackMode(character, attackMode, weapon);
    }

    protected abstract void TryModifyAttackMode(RulesetCharacter character, RulesetAttackMode attackMode,
        RulesetItem weapon);
}

public sealed class UpgradeWeaponDice : ModifyAttackModeForWeaponBase
{
    public delegate (RuleDefinitions.DieType, int) GetWeaponDiceHandler(RulesetCharacter character,
        RulesetItem weapon);

    private readonly GetWeaponDiceHandler getWeaponDice;

    public UpgradeWeaponDice(GetWeaponDiceHandler getWeaponDice, IsWeaponValidHandler isWeaponValid,
        params CharacterValidator[] validators) : base(isWeaponValid, validators)
    {
        this.getWeaponDice = getWeaponDice;
    }

    protected override void TryModifyAttackMode(RulesetCharacter character, [NotNull] RulesetAttackMode attackMode,
        RulesetItem weapon)
    {
        var effectDescription = attackMode.EffectDescription;
        var damage = effectDescription?.FindFirstDamageForm();

        if (damage == null)
        {
            return;
        }

        var (newDie, newNumber) = getWeaponDice(character, weapon);
        var newDamage = RuleDefinitions.DieAverage(newDie) * newNumber;

        var oldDamage = RuleDefinitions.DieAverage(damage.DieType) * damage.DiceNumber;
        var oldDamageVersatile = RuleDefinitions.DieAverage(damage.VersatileDieType) * damage.DiceNumber;


        if (newDamage > oldDamage)
        {
            damage.DieType = newDie;
            damage.DiceNumber = newNumber;
        }

        if (newDamage > oldDamageVersatile)
        {
            damage.VersatileDieType = newDie;
        }
    }
}

public sealed class AddTagToWeaponAttack : ModifyAttackModeForWeaponBase
{
    private readonly string tag;

    public AddTagToWeaponAttack(string tag, IsWeaponValidHandler isWeaponValid,
        params CharacterValidator[] validators) : base(isWeaponValid, validators)
    {
        this.tag = tag;
    }

    protected override void TryModifyAttackMode(RulesetCharacter character, [NotNull] RulesetAttackMode attackMode,
        RulesetItem weapon)
    {
        attackMode.AddAttackTagAsNeeded(tag);
    }
}

public class AddEffectToWeaponAttack : ModifyAttackModeForWeaponBase
{
    private readonly EffectForm effect;

    public AddEffectToWeaponAttack(EffectForm effect, IsWeaponValidHandler isWeaponValid,
        params CharacterValidator[] validators) : base(isWeaponValid, validators)
    {
        this.effect = effect;
    }

    protected override void TryModifyAttackMode(RulesetCharacter character, [NotNull] RulesetAttackMode attackMode,
        RulesetItem weapon)
    {
        attackMode.EffectDescription.AddEffectForms(effect);
    }
}

public sealed class BumpWeaponAttackRangeToMax : ModifyAttackModeForWeaponBase
{
    public BumpWeaponAttackRangeToMax(IsWeaponValidHandler isWeaponValid, params CharacterValidator[] validators)
        : base(isWeaponValid, validators)
    {
    }

    protected override void TryModifyAttackMode(RulesetCharacter character, [NotNull] RulesetAttackMode attackMode,
        RulesetItem weapon)
    {
        attackMode.closeRange = attackMode.maxRange;
    }
}
