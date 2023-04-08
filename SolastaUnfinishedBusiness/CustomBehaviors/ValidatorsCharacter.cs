﻿using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.WeaponTypeDefinitions;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

internal delegate bool IsCharacterValidHandler(RulesetCharacter character);

internal static class ValidatorsCharacter
{
    internal static readonly IsCharacterValidHandler HasAttacked = character => character.ExecutedAttacks > 0;

    internal static readonly IsCharacterValidHandler HasLessThan25PercentHealth = character =>
        (float)character.CurrentHitPoints / (character.CurrentHitPoints + character.MissingHitPoints) <= 0.25f;

    internal static readonly IsCharacterValidHandler HasNoArmor = character => !character.IsWearingArmor();

    internal static readonly IsCharacterValidHandler HasNoShield = character => !character.IsWearingShield();

    internal static readonly IsCharacterValidHandler HasShield = character => character.IsWearingShield();

    internal static readonly IsCharacterValidHandler HasLightArmor = character =>
        HasArmorCategory(character, EquipmentDefinitions.LightArmorCategory);

    internal static readonly IsCharacterValidHandler HasHeavyArmor = character =>
        HasArmorCategory(character, EquipmentDefinitions.HeavyArmorCategory);

    internal static readonly IsCharacterValidHandler DoesNotHaveHeavyArmor = character =>
        !HasArmorCategory(character, EquipmentDefinitions.HeavyArmorCategory);

    internal static readonly IsCharacterValidHandler HasLightSourceOffHand = character =>
        character is RulesetCharacterHero && character.GetOffhandWeapon()?.ItemDefinition.IsLightSourceItem == true;

    internal static readonly IsCharacterValidHandler HasFreeHand = character =>
        character.HasFreeHandSlot() &&
        !ValidatorsWeapon.HasAnyWeaponTag(character.GetMainWeapon(), TagsDefinitions.WeaponTagTwoHanded);

    internal static readonly IsCharacterValidHandler HasTwoHandedQuarterstaff = character =>
        ValidatorsWeapon.IsWeaponType(character.GetMainWeapon(), QuarterstaffType) && IsFreeOffhand(character);

    internal static readonly IsCharacterValidHandler HasTwoHandedRangedWeapon = character =>
        ValidatorsWeapon.IsWeaponType(character.GetMainWeapon(),
            LongbowType, ShortbowType, HeavyCrossbowType, LightCrossbowType);

    internal static readonly IsCharacterValidHandler HasTwoHandedVersatileWeapon = character =>
        ValidatorsWeapon.HasAnyWeaponTag(character.GetMainWeapon(), TagsDefinitions.WeaponTagVersatile) &&
        IsFreeOffhand(character);

    internal static readonly IsCharacterValidHandler HasMeleeWeaponInMainHand = character =>
        ValidatorsWeapon.IsMelee(character.GetMainWeapon());

    internal static readonly IsCharacterValidHandler IsUnarmedInMainHand = character =>
        ValidatorsWeapon.IsUnarmed(character.GetMainWeapon()?.ItemDefinition, null);

    internal static readonly IsCharacterValidHandler IsNotInBrightLight = character =>
        HasAnyOfLightingStates(
            LocationDefinitions.LightingState.Darkness,
            LocationDefinitions.LightingState.Unlit,
            LocationDefinitions.LightingState.Dim)(character);

    internal static IsCharacterValidHandler HasAnyOfConditions(params string[] conditions)
    {
        return character => conditions.Any(character.HasConditionOfType);
    }

    internal static IsCharacterValidHandler HasNoneOfConditions(params string[] conditions)
    {
        return character => !conditions.Any(character.HasConditionOfType);
    }

    private static IsCharacterValidHandler HasAnyOfLightingStates(
        params LocationDefinitions.LightingState[] lightingStates)
    {
        return character =>
        {
            var gameLocationCharacter = GameLocationCharacter.GetFromActor(character);

            return gameLocationCharacter != null && lightingStates.Contains(gameLocationCharacter.LightingState);
        };
    }

    internal static IsCharacterValidHandler HasMainHandWeaponType(params WeaponTypeDefinition[] weaponTypeDefinition)
    {
        return character => ValidatorsWeapon.IsWeaponType(character.GetMainWeapon(), weaponTypeDefinition);
    }

    internal static IsCharacterValidHandler HasOffhandWeaponType(params WeaponTypeDefinition[] weaponTypeDefinition)
    {
        return character => ValidatorsWeapon.IsWeaponType(character.GetOffhandWeapon(), weaponTypeDefinition);
    }

    internal static IsCharacterValidHandler HasWeaponType(params WeaponTypeDefinition[] weaponTypeDefinition)
    {
        return character =>
            ValidatorsWeapon.IsWeaponType(character.GetMainWeapon(), weaponTypeDefinition) ||
            ValidatorsWeapon.IsWeaponType(character.GetOffhandWeapon(), weaponTypeDefinition);
    }

    internal static IsCharacterValidHandler HasUsedWeaponType(WeaponTypeDefinition weaponTypeDefinition)
    {
        return character =>
        {
            var gameLocationCharacter = GameLocationCharacter.GetFromActor(character);

            return gameLocationCharacter != null &&
                   gameLocationCharacter.UsedSpecialFeatures.ContainsKey(weaponTypeDefinition.Name);
        };
    }

    internal static void RegisterWeaponTypeUsed(
        GameLocationCharacter gameLocationCharacter,
        RulesetAttackMode attackMode)
    {
        if (attackMode?.SourceDefinition is not ItemDefinition itemDefinition)
        {
            return;
        }

        var type = itemDefinition.IsWeapon
            ? itemDefinition.WeaponDescription.WeaponType
            : itemDefinition.ArmorDescription.ArmorType;

        gameLocationCharacter.UsedSpecialFeatures.TryAdd(type, 0);
        gameLocationCharacter.UsedSpecialFeatures[type]++;
    }

    //
    // BOOL VALIDATORS
    //

    internal static bool IsFreeOffhandVanilla(RulesetCharacter character)
    {
        var offHand = character.GetOffhandWeapon();

        // does character has free offhand in TA's terms as used in RefreshAttackModes for Monk bonus unarmed attack?
        return offHand == null || !offHand.ItemDefinition.IsWeapon;
    }

    internal static bool IsFreeOffhand(RulesetCharacter character)
    {
        return character.GetOffhandWeapon() == null;
    }

    internal static bool HasConditionWithSubFeatureOfType<T>(this RulesetCharacter character) where T : class
    {
        return character.conditionsByCategory
            .Any(keyValuePair => keyValuePair.Value
                .Any(rulesetCondition => rulesetCondition.ConditionDefinition.HasSubFeatureOfType<T>()));
    }

    private static bool HasArmorCategory(RulesetCharacter character, string category)
    {
        // required for wildshape scenarios
        if (character is not RulesetCharacterHero)
        {
            return false;
        }

        var equipedItem = character.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeTorso]
            .EquipedItem;

        if (equipedItem == null || !equipedItem.ItemDefinition.IsArmor)
        {
            return false;
        }

        var armorDescription = equipedItem.ItemDefinition.ArmorDescription;
        var element = DatabaseHelper.GetDefinition<ArmorTypeDefinition>(armorDescription.ArmorType);

        return DatabaseHelper.GetDefinition<ArmorCategoryDefinition>(element.ArmorCategory)
            .IsPhysicalArmor && element.ArmorCategory == category;
    }
}
