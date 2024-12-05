﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Validators;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Behaviors;

internal enum AttackModeOrder
{
    Start,
    End
}

internal interface IAddExtraAttack
{
    // sort sub features [used on race claw attacks]
    public int Priority();
    public void TryAddExtraAttack(RulesetCharacter character);
}

internal abstract class AddExtraAttackBase(
    ActionDefinitions.ActionType actionType,
    params IsCharacterValidHandler[] validators) : IAddExtraAttack
{
    // private readonly List<string> additionalTags = new();
    protected readonly ActionDefinitions.ActionType ActionType = actionType;

    public void TryAddExtraAttack(RulesetCharacter character)
    {
        if (!character.IsValid(validators))
        {
            return;
        }

        var attackModes = character.AttackModes;

        var newAttacks = GetAttackModes(character);

        if (newAttacks == null || newAttacks.Count == 0)
        {
            return;
        }

        foreach (var attackMode in newAttacks)
        {
            var same = attackModes.FirstOrDefault(m => ModesEqual(attackMode, m));

            if (same != null)
            {
                //If same attack mode exists, ensure it has max amount of attacks
                same.attacksNumber = Math.Max(attackMode.attacksNumber, same.attacksNumber);
                //and dispose of newly created one
                RulesetAttackMode.AttackModesPool.Return(attackMode);
            }
            else
            {
                var order = GetOrder(character);

                switch (order)
                {
                    case AttackModeOrder.Start:
                        attackModes.Insert(0, attackMode);
                        break;
                    case AttackModeOrder.End:
                        attackModes.Add(attackMode);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(order.ToString());
                }
            }
        }
    }

    public virtual int Priority()
    {
        return 0;
    }

    protected abstract List<RulesetAttackMode> GetAttackModes(RulesetCharacter character);

    protected virtual AttackModeOrder GetOrder(RulesetCharacter character)
    {
        return AttackModeOrder.End;
    }

    //Copied from RulesetAttackMode.IsComparableForNetwork, but not checking for attack number
    private static bool ModesEqual([NotNull] RulesetAttackMode a, RulesetAttackMode b)
    {
        //added all these locals for debug log
        var actionType = a.actionType == b.actionType;
        var sourceDefinition = a.sourceDefinition == b.sourceDefinition;
        var sourceObject = a.sourceObject == b.sourceObject;
        var slotName = a.slotName == b.slotName;
        var ranged = a.ranged == b.ranged;
        var thrown = a.thrown == b.thrown;
        var reach = a.reach == b.reach;
        var reachRange = a.reachRange == b.reachRange;
        var closeRange = a.closeRange == b.closeRange;
        var maxRange = a.maxRange == b.maxRange;
        var toHitBonus = a.toHitBonus == b.toHitBonus;
        //var attacksNumber = a.attacksNumber == b.attacksNumber;
        var useVersatileDamage = a.useVersatileDamage == b.useVersatileDamage;
        var freeOffHand = a.freeOffHand == b.freeOffHand;
        var automaticHit = a.automaticHit == b.automaticHit;
        var afterChargeOnly = a.afterChargeOnly == b.afterChargeOnly;

        return actionType
               && sourceDefinition
               && sourceObject
               && slotName
               && ranged
               && thrown
               && reach
               && reachRange
               && closeRange
               && maxRange
               && toHitBonus
               // && attacksNumber
               && useVersatileDamage
               && freeOffHand
               && automaticHit
               && afterChargeOnly;
    }
}

internal sealed class AddExtraUnarmedAttack : AddExtraAttackBase
{
    internal AddExtraUnarmedAttack(
        ActionDefinitions.ActionType actionType,
        params IsCharacterValidHandler[] validators) : base(actionType, validators)
    {
        // Empty
    }

    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacter character)
    {
        var hero = character as RulesetCharacterHero;
        var monster = character as RulesetCharacterMonster;

        if (hero == null && monster == null)
        {
            return null;
        }

        var originalHero = character.GetOriginalHero();
        var mainHand = hero.GetMainWeapon();
        // although IsUnarmed can take null this is a special case: isUnarmedWeapon vs isUnarmed only
        var isUnarmedWeapon = mainHand != null && ValidatorsWeapon.IsUnarmed(mainHand);
        var strikeDefinition = isUnarmedWeapon
            ? mainHand.ItemDefinition
            : originalHero != null
                ? originalHero.UnarmedStrikeDefinition
                : DatabaseHelper.ItemDefinitions.UnarmedStrikeBase;

        var attackModifiers = hero?.attackModifiers ?? monster?.attackModifiers;

        var attackMode = character.TryRefreshAttackMode(
            ActionType,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            ValidatorsCharacter.IsFreeOffhandVanilla(hero),
            true,
            EquipmentDefinitions.SlotTypeMainHand,
            attackModifiers,
            character.FeaturesOrigin,
            isUnarmedWeapon ? mainHand : null
        );

        return [attackMode];
    }
}

internal sealed class AddExtraMainHandAttack : AddExtraAttackBase
{
    internal AddExtraMainHandAttack(
        ActionDefinitions.ActionType actionType,
        params IsCharacterValidHandler[] validators) : base(actionType, validators)
    {
    }

    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacter character)
    {
        if (character is not RulesetCharacterHero hero)
        {
            return null;
        }

        var mainHandItem = hero.GetMainWeapon();

        // don't use ?? on Unity Objects as it bypasses the lifetime check on the underlying object
        var strikeDefinition = mainHandItem?.ItemDefinition;

        if (!strikeDefinition)
        {
            strikeDefinition = hero.UnarmedStrikeDefinition;
        }

        var attackModifiers = hero.attackModifiers;

        var attackMode = hero.RefreshAttackMode(
            ActionType,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            ValidatorsCharacter.IsFreeOffhand(hero),
            true,
            EquipmentDefinitions.SlotTypeMainHand,
            attackModifiers,
            hero.FeaturesOrigin,
            mainHandItem
        );

        attackMode.AttackTags.Add(UpgradeWeaponDice.AbortUpgradeWeaponDice);

        return [attackMode];
    }
}

internal sealed class AddExtraRangedAttack : AddExtraAttackBase
{
    private readonly IsWeaponValidHandler _weaponValidator;

    internal AddExtraRangedAttack(
        ActionDefinitions.ActionType actionType,
        IsWeaponValidHandler weaponValidator,
        params IsCharacterValidHandler[] validators) : base(actionType, validators)
    {
        _weaponValidator = weaponValidator;
    }

    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacter character)
    {
        if (character is not RulesetCharacterHero hero)
        {
            return null;
        }

        var item = hero.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;

        if (item == null || !_weaponValidator.Invoke(null, item, hero))
        {
            return null;
        }

        var strikeDefinition = item.ItemDefinition;
        var attackMode = hero.RefreshAttackMode(
            ActionType,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            ValidatorsCharacter.IsFreeOffhand(hero),
            true,
            EquipmentDefinitions.SlotTypeMainHand,
            hero.attackModifiers,
            hero.FeaturesOrigin,
            item
        );

        attackMode.Reach = false;
        attackMode.Ranged = true;
        attackMode.Thrown = ValidatorsWeapon.HasAnyWeaponTag(item.ItemDefinition, TagsDefinitions.WeaponTagThrown);
        attackMode.AttackTags.Remove(TagsDefinitions.WeaponTagMelee);

        return [attackMode];
    }
}

internal sealed class AddPolearmFollowUpAttack : AddExtraAttackBase
{
    private readonly WeaponTypeDefinition _weaponTypeDefinition;

    internal AddPolearmFollowUpAttack(WeaponTypeDefinition weaponTypeDefinition) : base(
        ActionDefinitions.ActionType.Bonus,
        ValidatorsCharacter.HasUsedWeaponType(weaponTypeDefinition),
        ValidatorsCharacter.HasMainHandWeaponType(weaponTypeDefinition))
    {
        _weaponTypeDefinition = weaponTypeDefinition;
    }

    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacter character)
    {
        if (character is not RulesetCharacterHero hero)
        {
            return null;
        }

        var item = hero.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;

        if (item == null ||
            !ValidatorsWeapon.IsWeaponType(item, _weaponTypeDefinition))
        {
            return null;
        }

        var strikeDefinition = item.ItemDefinition;
        var attackMode = hero.RefreshAttackMode(
            ActionType,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            ValidatorsCharacter.IsFreeOffhand(hero),
            true,
            EquipmentDefinitions.SlotTypeMainHand,
            hero.attackModifiers,
            hero.FeaturesOrigin,
            item
        );

        var effectDamageForm = attackMode.EffectDescription.EffectForms
            .FirstOrDefault(x => x.FormType == EffectForm.EffectFormType.Damage);

        if (effectDamageForm == null ||
            // ensures PAM interacts well with GWM
            hero.HasConditionOfCategoryAndType(
                AttributeDefinitions.TagEffect, "ConditionFeatCleavingAttackFinish"))
        {
            return [attackMode];
        }

        effectDamageForm.DamageForm.DamageType = DamageTypeBludgeoning;
        effectDamageForm.DamageForm.DieType = DieType.D4;
        effectDamageForm.DamageForm.DiceNumber = 1;
        effectDamageForm.DamageForm.versatile = false;
        effectDamageForm.DamageForm.versatileDieType = effectDamageForm.DamageForm.DieType;
        attackMode.AttackTags.Add(UpgradeWeaponDice.AbortUpgradeWeaponDice);

        return [attackMode];
    }
}

internal sealed class AddWhirlWindFollowUpAttack : AddExtraAttackBase
{
    private readonly WeaponTypeDefinition _weaponTypeDefinition;

    internal AddWhirlWindFollowUpAttack(WeaponTypeDefinition weaponTypeDefinition) : base(
        ActionDefinitions.ActionType.Bonus,
        ValidatorsCharacter.HasUsedWeaponType(weaponTypeDefinition),
        ValidatorsCharacter.HasMainHandWeaponType(weaponTypeDefinition))
    {
        _weaponTypeDefinition = weaponTypeDefinition;
    }

    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacter character)
    {
        if (character is not RulesetCharacterHero hero)
        {
            return null;
        }

        var item = hero.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;

        if (item == null ||
            !ValidatorsWeapon.IsWeaponType(item, _weaponTypeDefinition))
        {
            return null;
        }

        var strikeDefinition = item.ItemDefinition;
        var attackMode = hero.RefreshAttackMode(
            ActionType,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            ValidatorsCharacter.IsFreeOffhand(hero),
            true,
            EquipmentDefinitions.SlotTypeMainHand,
            hero.attackModifiers,
            hero.FeaturesOrigin,
            item
        );

        var effectDamageForm = attackMode.EffectDescription.EffectForms
            .FirstOrDefault(x => x.FormType == EffectForm.EffectFormType.Damage);

        if (effectDamageForm == null ||
            // ensures WhirlWind interacts well with GWM
            hero.HasConditionOfCategoryAndType(
                AttributeDefinitions.TagEffect, "ConditionFeatCleavingAttackFinish"))
        {
            return [attackMode];
        }

        effectDamageForm.DamageForm.DieType = DieType.D4;
        effectDamageForm.DamageForm.DiceNumber = 1;
        effectDamageForm.DamageForm.versatile = false;
        effectDamageForm.DamageForm.versatileDieType = effectDamageForm.DamageForm.DieType;
        attackMode.AttackTags.Add(UpgradeWeaponDice.AbortUpgradeWeaponDice);

        return [attackMode];
    }
}

internal sealed class AddBonusShieldAttack : AddExtraAttackBase
{
    internal AddBonusShieldAttack() : base(ActionDefinitions.ActionType.Bonus)
    {
        // Empty
    }

    [CanBeNull]
    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacter character)
    {
        if (character is not RulesetCharacterHero hero)
        {
            return null;
        }

        var offHandItem = hero.GetOffhandWeapon();

        if (offHandItem == null ||
            !ValidatorsWeapon.IsShield(offHandItem.ItemDefinition))
        {
            return null;
        }

        var acModifier = offHandItem.ItemDefinition.StaticProperties
            .Where(x => x.Type == ItemPropertyDescription.PropertyType.Feature)
            .Select(x => x.FeatureDefinition)
            .OfType<FeatureDefinitionAttributeModifier>()
            .Where(x => x.ModifiedAttribute == AttributeDefinitions.ArmorClass)
            .Select(x => x.ModifierValue)
            .AddItem(0)
            .Max();
        var attackModifiers = hero.attackModifiers;
        var attackMode = hero.RefreshAttackMode(
            ActionDefinitions.ActionType.Bonus,
            offHandItem.ItemDefinition,
            ShieldStrike.ShieldWeaponDescription,
            ValidatorsCharacter.IsFreeOffhand(hero),
            true,
            EquipmentDefinitions.SlotTypeOffHand,
            attackModifiers,
            hero.FeaturesOrigin,
            offHandItem);

        var damageForm = attackMode.EffectDescription.FindFirstDamageForm();

        if (damageForm != null)
        {
            var duelingTrend = damageForm.DamageBonusTrends.FirstOrDefault(x => x.sourceName == "Dueling");

            if (duelingTrend.sourceName == "Dueling")
            {
                damageForm.BonusDamage -= 2;
                // ReSharper disable once UsageOfDefaultStructEquality
                damageForm.DamageBonusTrends.Remove(duelingTrend);
            }

            if (acModifier > 0)
            {
                var magicalTrend = new TrendInfo(acModifier,
                    FeatureSourceType.Equipment, offHandItem.ItemDefinition.Name, offHandItem.ItemDefinition);

                attackMode.ToHitBonus += acModifier;
                attackMode.ToHitBonusTrends.Add(magicalTrend);
                damageForm.BonusDamage += acModifier;
                damageForm.DamageBonusTrends.Add(magicalTrend);
            }
        }

        if (offHandItem.ItemDefinition.Magical)
        {
            attackMode.AddAttackTagAsNeeded(TagsDefinitions.MagicalWeapon);
        }

        return [attackMode];
    }
}

internal sealed class AddBonusTorchAttack : AddExtraAttackBase
{
    private readonly FeatureDefinitionPower _torchPower;

    internal AddBonusTorchAttack(FeatureDefinitionPower torchPower) : base(ActionDefinitions.ActionType.Bonus)
    {
        _torchPower = torchPower;
    }

    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacter character)
    {
        if (character is not RulesetCharacterHero hero)
        {
            return null;
        }

        var item = hero.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem;

        if (item == null || !ValidatorsCharacter.HasLightSourceOffHand(hero))
        {
            return null;
        }

        var strikeDefinition = item.ItemDefinition;
        var attackMode = hero.RefreshAttackMode(
            ActionType,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            ValidatorsCharacter.IsFreeOffhand(hero),
            true,
            EquipmentDefinitions.SlotTypeOffHand,
            hero.attackModifiers,
            hero.FeaturesOrigin,
            item
        );

        attackMode.Reach = false;
        attackMode.Ranged = false;
        attackMode.Thrown = false;
        attackMode.AutomaticHit = true;
        attackMode.EffectDescription.Clear();
        attackMode.EffectDescription.Copy(_torchPower.EffectDescription);

        var proficiencyBonus = hero.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);
        var dexterity = hero.TryGetAttributeValue(AttributeDefinitions.Dexterity);

        attackMode.EffectDescription.fixedSavingThrowDifficultyClass =
            8 + proficiencyBonus + AttributeDefinitions.ComputeAbilityScoreModifier(dexterity);

        return [attackMode];
    }
}
