﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

internal static class ShieldAttack
{
    public static void UseOffhandForShieldAttackAnimation(RulesetAttackMode attackMode, ref string animation,
        ref bool isThrown,
        ref bool leftHand)
    {
        if (!ShieldStrikeContext.IsShield(attackMode.SourceDefinition as ItemDefinition))
        {
            return;
        }

        leftHand = true;
        isThrown = false;
        animation = ShieldStrikeContext.ShieldWeaponType.AnimationTag;
    }

    //replaces calls to ItemDefinition's isWeapon and Wea[ponDescription getter with custom ones that account for shield
    public static IEnumerable<CodeInstruction> MakeShieldCountAsMelee(IEnumerable<CodeInstruction> instructions)
    {
        var weaponDescription = typeof(ItemDefinition).GetMethod("get_WeaponDescription");
        var isWeapon = typeof(ItemDefinition).GetMethod("get_IsWeapon");
        var customWeaponDescription = new Func<ItemDefinition, WeaponDescription>(CustomWeaponDescription).Method;
        var customIsWeapon = new Func<ItemDefinition, bool>(CustomIsWeapon).Method;

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(weaponDescription))
            {
                yield return new CodeInstruction(OpCodes.Call, customWeaponDescription);
            }
            else if (instruction.Calls(isWeapon))
            {
                yield return new CodeInstruction(OpCodes.Call, customIsWeapon);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static WeaponDescription CustomWeaponDescription(ItemDefinition item)
    {
        return ShieldStrikeContext.IsShield(item)
            ? ShieldStrikeContext.ShieldWeaponDescription
            : item.WeaponDescription;
    }

    private static bool CustomIsWeapon(ItemDefinition item)
    {
        return item.IsWeapon || ShieldStrikeContext.IsShield(item);
    }
}
