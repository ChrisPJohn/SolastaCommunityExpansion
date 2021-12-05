﻿using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches
{
    // extends the cost buy table
    [HarmonyPatch(typeof(AttributeDefinitions), "ComputeCostToRaiseAbility")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class AttributeDefinitions_ComputeCostToRaiseAbility 
    {
        internal static void Postfix(int previousValue, ref int __result)
        {
            if (Main.Settings.EnableEpicPoints)
            {
                if (Array.IndexOf<int>(new int[] { 15, 16 }, previousValue) != -1)
                {
                    __result = 3;
                }
                else if (Array.IndexOf<int>(new int[] { 17, 18 }, previousValue) != -1)
                {
                    __result = 4;
                }
            }
        }
    }
}
