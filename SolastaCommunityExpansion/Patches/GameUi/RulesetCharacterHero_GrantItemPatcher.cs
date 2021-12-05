﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches
{
    [HarmonyPatch(typeof(RulesetCharacterHero), "GrantItem")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RulesetCharacterHero_GrantItem
    {
        public static void Prefix(ref bool tryToEquip)
        {
            if (Main.Settings.DisableAutoEquip)
            {
                tryToEquip = false;
            }
        }
    }
}
