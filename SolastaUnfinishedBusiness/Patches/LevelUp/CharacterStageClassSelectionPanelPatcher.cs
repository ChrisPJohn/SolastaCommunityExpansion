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

namespace SolastaUnfinishedBusiness.Patches.LevelUp;

//PATCH: sorts the class panel by Title
[HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "Compare")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class CharacterStageClassSelectionPanel_Compare
{
    internal static void Postfix(CharacterClassDefinition left, CharacterClassDefinition right, ref int __result)
    {
        if (Main.Settings.EnableSortingClasses)
        {
            __result = String.Compare(left.FormatTitle(), right.FormatTitle(),
                StringComparison.CurrentCultureIgnoreCase);
        }
    }
}

//PATCH: flags displaying the class panel / apply in/out logic (MULTICLASS)
[HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "OnBeginShow")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class CharacterStageClassSelectionPanel_OnBeginShow
{
    internal static void Prefix([NotNull] CharacterStageClassSelectionPanel __instance)
    {
        // avoids a restart when enabling / disabling classes on the Mod UI panel
        if (!LevelUpContext.IsLevelingUp(__instance.currentHero))
        {
            var visibleClasses = DatabaseRepository.GetDatabase<CharacterClassDefinition>()
                .Where(x => !x.GuiPresentation.Hidden);

            __instance.compatibleClasses.SetRange(visibleClasses.OrderBy(x => x.FormatTitle()));
            return;
        }

        LevelUpContext.SetIsClassSelectionStage(__instance.currentHero, true);
        MulticlassInOutRulesContext.EnumerateHeroAllowedClassDefinitions(__instance.currentHero,
            __instance.compatibleClasses,
            out __instance.selectedClass);

        var commonData = __instance.CommonData;

        // NOTE: don't use AttackModesPanel?. which bypasses Unity object lifetime check
        if (commonData.AttackModesPanel)
        {
            commonData.AttackModesPanel.RefreshNow();
        }

        // NOTE: don't use PersonalityMapPanel?. which bypasses Unity object lifetime check
        if (commonData.PersonalityMapPanel)
        {
            commonData.PersonalityMapPanel.RefreshNow();
        }
    }
}

//PATCH: resets IsClassSelectionStage for hero (MULTICLASS)
[HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "OnEndHide")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class CharacterStageClassSelectionPanel_OnEndHide
{
    internal static void Prefix([NotNull] CharacterStageClassSelectionPanel __instance)
    {
        if (__instance.currentHero != null && LevelUpContext.IsLevelingUp(__instance.currentHero))
        {
            LevelUpContext.SetIsClassSelectionStage(__instance.currentHero, false);
        }
    }
}

//PATCH: hides the features list for already acquired classes (MULTICLASS)
[HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "FillClassFeatures")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class CharacterStageClassSelectionPanel_FillClassFeatures
{
    public static int Level([NotNull] FeatureUnlockByLevel featureUnlockByLevel, [NotNull] RulesetCharacterHero hero)
    {
        var isLevelingUp = LevelUpContext.IsLevelingUp(hero);
        var selectedClass = LevelUpContext.GetSelectedClass(hero);

        if (!isLevelingUp)
        {
            return featureUnlockByLevel.Level;
        }

        var levels = 0;

        if (selectedClass != null
            && hero.ClassesAndLevels.TryGetValue(selectedClass, out levels)
            && featureUnlockByLevel.Level != levels + 1)
        {
            return int.MaxValue;
        }

        if (levels == 0)
        {
            return featureUnlockByLevel.Level;
        }

        return featureUnlockByLevel.Level - 1;
    }

    [NotNull]
    internal static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
    {
        var levelMethod = typeof(FeatureUnlockByLevel).GetMethod("get_Level");
        var myLevelMethod = typeof(CharacterStageClassSelectionPanel_FillClassFeatures).GetMethod("Level");
        var currentHeroField =
            typeof(CharacterStageClassSelectionPanel).GetField("currentHero",
                BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(levelMethod))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, currentHeroField);
                yield return new CodeInstruction(OpCodes.Call, myLevelMethod);
            }
            else
            {
                yield return instruction;
            }
        }
    }
}

//PATCH: hides the equipment panel group (MULTICLASS)
[HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "Refresh")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class CharacterStageClassSelectionPanel_Refresh
{
    public static bool SetActive([NotNull] RulesetCharacterHero currentHero)
    {
        return !LevelUpContext.IsLevelingUp(currentHero);
    }

    [NotNull]
    internal static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
    {
        var setActiveFound = 0;
        var setActiveMethod = typeof(GameObject).GetMethod("SetActive");
        var mySetActiveMethod = typeof(CharacterStageClassSelectionPanel_Refresh).GetMethod("SetActive");
        var currentHeroField =
            typeof(CharacterStageClassSelectionPanel).GetField("currentHero",
                BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(setActiveMethod) && ++setActiveFound == 4)
            {
                yield return new CodeInstruction(OpCodes.Pop);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, currentHeroField);
                yield return new CodeInstruction(OpCodes.Call, mySetActiveMethod);
            }

            yield return instruction;
        }
    }
}
