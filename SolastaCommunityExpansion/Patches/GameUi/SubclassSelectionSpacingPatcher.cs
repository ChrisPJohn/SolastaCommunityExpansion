﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaModApi.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace SolastaUIUpdates.Patches
{
    // spacing
    internal static class SubclassSelectionSpacingPatcher
    {
        [HarmonyPatch(typeof(CharacterStageSubclassSelectionPanel), "EnterStage")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class CharacterStageSubclassSelectionPanel_EnterStage
        {
            public static void Postfix(CharacterStageSubclassSelectionPanel __instance)
            {
                Transform subclassesTable = __instance.GetField<Transform>("subclassesTable");
                GridLayoutGroup subclassGrid = subclassesTable.GetComponent<GridLayoutGroup>();
                subclassGrid.spacing = new Vector2(subclassGrid.spacing.x, 60f);
            }
        }
    }
}
