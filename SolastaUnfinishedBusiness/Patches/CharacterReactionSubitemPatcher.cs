﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

internal static class CharacterReactionSubitemPatcher
{
    // creates different slots colors and pop up messages depending on slot types
    [HarmonyPatch(typeof(CharacterReactionSubitem), "Bind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class Bind_Patch
    {
        public static void Postfix(
            CharacterReactionSubitem __instance,
            RulesetSpellRepertoire spellRepertoire,
            int slotLevel)
        {
            var heroWithSpellRepertoire = SharedSpellsContext.GetHero(spellRepertoire.CharacterName);

            if (heroWithSpellRepertoire is null)
            {
                return;
            }

            spellRepertoire.GetSlotsNumber(slotLevel, out var totalSlotsRemainingCount, out var totalSlotsCount);

            MulticlassGameUiContext.PaintPactSlots(
                heroWithSpellRepertoire, totalSlotsCount, totalSlotsRemainingCount, slotLevel,
                __instance.slotStatusTable);
        }
    }

    [HarmonyPatch(typeof(CharacterReactionSubitem), "Unbind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class Unbind_Patch
    {
        public static void Prefix(CharacterReactionSubitem __instance)
        {
            //PATCH: ensures slot colors are white before getting back to pool
            MulticlassGameUiContext.PaintSlotsWhite(__instance.slotStatusTable);

            //PATCH: disables tooltip on Unbind.
            //default implementation doesn't use tooltips, so we are cleaning up after custom warcaster and bundled power binds
            var toggle = __instance.toggle.GetComponent<RectTransform>();

            toggle.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 34);

            var background = toggle.FindChildRecursive("Background");

            if (background == null)
            {
                return;
            }

            if (background.TryGetComponent<GuiTooltip>(out var tooltip))
            {
                tooltip.Disabled = true;
            }
        }
    }
}
