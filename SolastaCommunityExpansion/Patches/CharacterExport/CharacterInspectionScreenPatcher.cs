﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches
{
    // uses this patch to trap the input hotkey and start export process
    [HarmonyPatch(typeof(CharacterInspectionScreen), "HandleInput")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class CharacterInspectionScreen_HandleInput
    {
        public static void Prefix(CharacterInspectionScreen __instance, InputCommands.Id command, ref bool __result)
        {
            if (Gui.Game != null && Main.Settings.EnableCharacterExport && !Models.CharacterExportContext.InputModalVisible && command == Settings.CTRL_E)
            {
                Models.CharacterExportContext.ExportInspectedCharacter(__instance.InspectedCharacter.RulesetCharacterHero);
            }
        }
    }
}
