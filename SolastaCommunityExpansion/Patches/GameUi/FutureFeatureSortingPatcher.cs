﻿using HarmonyLib;
using SolastaModApi.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SolastaCommunityExpansion.Patches
{
    internal static class FutureFeatureSortingPatcher
    {
        [HarmonyPatch(typeof(CharacterStageSubclassSelectionPanel), "FillSubclassFeatures")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class CharacterStageSubclassSelectionPanel_FillSubclassFeatures
        {
            internal static void Prefix(CharacterSubclassDefinition subclassDefinition)
            {
                if (!Main.Settings.FutureFeatureSorting)
                {
                    return;
                }
                subclassDefinition.FeatureUnlocks.Sort((a, b) => a.Level - b.Level);
            }
        }

        [HarmonyPatch(typeof(CharacterStageDeitySelectionPanel), "FillSubclassFeatures")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class CharacterStageDeitySelectionPanel_FillSubclassFeatures
        {
            internal static void Prefix(CharacterSubclassDefinition currentSubclassDefinition)
            {
                if (!Main.Settings.FutureFeatureSorting)
                {
                    return;
                }
                currentSubclassDefinition.FeatureUnlocks.Sort((a, b) => a.Level - b.Level);
            }
        }

        [HarmonyPatch(typeof(ArchetypesPreviewModal), "Bind")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class ArchetypesPreviewModal_Bind
        {
            internal static void Postfix(ArchetypesPreviewModal __instance)
            {
                if (!Main.Settings.FutureFeatureSorting)
                {
                    return;
                }
                List<CharacterSubclassDefinition> subclasses = __instance.GetField<List<CharacterSubclassDefinition>>("subclasses");
                foreach (CharacterSubclassDefinition subclassDefinition in subclasses)
                {
                    subclassDefinition.FeatureUnlocks.Sort((a, b) => a.Level - b.Level);
                }
            }
        }

        [HarmonyPatch(typeof(CharacterStageClassSelectionPanel), "FillClassFeatures")]
        [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
        internal static class CharacterStageClassSelectionPanel_FillSubclassFeatures
        {
            internal static void Prefix(CharacterClassDefinition classDefinition)
            {
                if (!Main.Settings.FutureFeatureSorting)
                {
                    return;
                }
                classDefinition.FeatureUnlocks.Sort((a, b) => a.Level - b.Level);
            }
        }
    }
}
