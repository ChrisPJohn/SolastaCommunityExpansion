﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace SolastaUnfinishedBusiness.Patches.LevelUp;

//PATCH: adds more spacing in between subclasses badges
[HarmonyPatch(typeof(CharacterStageSubclassSelectionPanel), "EnterStage")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class CharacterStageSubclassSelectionPanel_EnterStage
{
    public static void Postfix([NotNull] CharacterStageSubclassSelectionPanel __instance)
    {
        var subclassesTable = __instance.subclassesTable;
        var subclassGrid = subclassesTable.GetComponent<GridLayoutGroup>();

        subclassGrid.spacing = new Vector2(subclassGrid.spacing.x, 60f);
    }
}
