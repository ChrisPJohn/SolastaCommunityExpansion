﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;
using UnityEngine.UI;

namespace SolastaUnfinishedBusiness.Patches.LevelUp;

[HarmonyPatch(typeof(FeatSubPanel), "Bind")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class FeatSubPanel_Bind
{
    internal static void Prefix([NotNull] FeatSubPanel __instance)
    {
        //PATCH: avoids a restart when enabling / disabling feats on the Mod UI panel
        var dbFeatDefinition = DatabaseRepository.GetDatabase<FeatDefinition>();

        __instance.relevantFeats.SetRange(dbFeatDefinition.Where(x => !x.GuiPresentation.Hidden));

        //PATCH: sorts the feats panel by Title
        if (Main.Settings.EnableSortingFeats)
        {
            __instance.relevantFeats.Sort((a, b) =>
                String.Compare(a.FormatTitle(), b.FormatTitle(), StringComparison.CurrentCultureIgnoreCase));
        }

        while (__instance.table.childCount < __instance.relevantFeats.Count)
        {
            Gui.GetPrefabFromPool(__instance.itemPrefab, __instance.table);
        }

        while (__instance.table.childCount > __instance.relevantFeats.Count)
        {
            Gui.ReleaseInstanceToPool(__instance.table.GetChild(__instance.table.childCount - 1).gameObject);
        }
    }
}

//PATCH: enforces the feat selection panel to always display same-width columns
[HarmonyPatch(typeof(FeatSubPanel), "SetState")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class FeatSubPanel_SetState
{
    [NotNull]
    internal static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
    {
        var forceRebuildLayoutImmediateMethod = typeof(LayoutRebuilder)
            .GetMethod("ForceRebuildLayoutImmediate", BindingFlags.Static | BindingFlags.Public);
        var forceSameWidthMethod = new Action<RectTransform, bool>(ForceSameWidth).Method;

        var code = instructions.ToList();
        var index = code.FindIndex(x => x.Calls(forceRebuildLayoutImmediateMethod));

        code[index] = new CodeInstruction(OpCodes.Ldarg_1);
        code.Insert(index + 1, new CodeInstruction(OpCodes.Call, forceSameWidthMethod));

        return code;
    }

    private static void ForceSameWidth(RectTransform table, bool active)
    {
        const int COLUMNS = 3;
        const int WIDTH = 300;
        const int HEIGHT = 44;
        const int SPACING = 5;

        if (active && Main.Settings.EnableSameWidthFeatSelection)
        {
            var hero = Global.ActiveLevelUpHero;
            var buildingData = hero?.GetHeroBuildingData();

            if (buildingData == null)
            {
                return;
            }

            var trainedFeats = buildingData.LevelupTrainedFeats.SelectMany(x => x.Value).ToList();

            trainedFeats.AddRange(hero.TrainedFeats);

            var j = 0;
            var rect = table.GetComponent<RectTransform>();

            rect.sizeDelta = new Vector2(rect.sizeDelta.x, ((table.childCount / COLUMNS) + 1) * (HEIGHT + SPACING));

            for (var i = 0; i < table.childCount; i++)
            {
                var child = table.GetChild(i);
                var featItem = child.GetComponent<FeatItem>();

                if (trainedFeats.Contains(featItem.GuiFeatDefinition.FeatDefinition))
                {
                    continue;
                }

                var x = j % COLUMNS;
                var y = j / COLUMNS;
                var posX = x * (WIDTH + (SPACING * 2));
                var posY = -y * (HEIGHT + SPACING);

                rect = child.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(posX, posY);
                rect.sizeDelta = new Vector2(WIDTH, HEIGHT);

                j++;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(table);
    }
}
