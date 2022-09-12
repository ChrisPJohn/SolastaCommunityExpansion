﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaUnfinishedBusiness.CustomUI;

namespace SolastaUnfinishedBusiness.Patches;

internal static class TooltipFeatureDescriptionPatcher
{
    //PATCH: adds crafting details to recipe tooltips
    [HarmonyPatch(typeof(TooltipFeatureDescription), "Bind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class Bind_Patch
    {
        internal static void Postfix(TooltipFeatureDescription __instance, ITooltip tooltip)
        {
            Tooltips.UpdateCraftingTooltip(__instance, tooltip);
        }
    }
}
