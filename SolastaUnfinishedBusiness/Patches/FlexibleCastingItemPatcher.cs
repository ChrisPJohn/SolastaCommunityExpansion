﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

// creates different slots colors and pop up messages depending on slot types
[HarmonyPatch(typeof(FlexibleCastingItem), "Bind")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class FlexibleCastingItem_Bind
{
    internal static void Postfix(
        FlexibleCastingItem __instance,
        int slotLevel,
        int remainingSlots,
        int maxSlots)
    {
        var flexibleCastingModal = __instance.GetComponentInParent<FlexibleCastingModal>();

        if (flexibleCastingModal.caster is not RulesetCharacterHero caster)
        {
            return;
        }

        if (!SharedSpellsContext.IsMulticaster(caster))
        {
            return;
        }

        MulticlassGameUiContext.PaintPactSlots(
            caster, maxSlots, remainingSlots, slotLevel, __instance.slotStatusTable);
    }
}

// ensures slot colors are white before getting back to pool
[HarmonyPatch(typeof(FlexibleCastingItem), "Unbind")]
internal static class FlexibleCastingItem_Unbind
{
    internal static void Prefix(FlexibleCastingItem __instance)
    {
        MulticlassGameUiContext.PaintSlotsWhite(__instance.slotStatusTable);
    }
}
