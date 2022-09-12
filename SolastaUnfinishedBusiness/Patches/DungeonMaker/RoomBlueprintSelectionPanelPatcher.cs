﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches.DungeonMaker;

//PATCH: better rooms sorting
[HarmonyPatch(typeof(RoomBlueprintSelectionPanel), "Compare")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class RoomBlueprintSelectionPanel_Compare
{
    internal static bool Prefix(RoomBlueprint left, RoomBlueprint right, ref int __result)
    {
        if (!Main.Settings.EnableSortingDungeonMakerAssets)
        {
            return true;
        }

        __result = DmProEditorContext.Compare(left, right);

        return false;
    }
}
