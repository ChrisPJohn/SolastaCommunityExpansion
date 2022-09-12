﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

internal static class ReadyActionSelectionPanelPatcher
{
    [HarmonyPatch(typeof(ReadyActionSelectionPanel), "Bind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class Bind_Patch
    {
        internal static void Prefix(ReadyActionSelectionPanel __instance)
        {
            //PATCH: adds toggle button to ready action panel for 'force preferred cantrip' feature
            CustomReactionsContext.SetupForcePreferredToggle(__instance.preferredCantripSelectionGroup);
        }
    }
}
